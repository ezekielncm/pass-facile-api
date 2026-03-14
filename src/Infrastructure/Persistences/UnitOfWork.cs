using Application.Common.Interfaces.Messaging;
using Application.Common.Interfaces.Persistence;
using Domain.Common;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Persistences
{
    public sealed class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly IEventPublisher _eventPublisher;
        private IDbContextTransaction? _currentTransaction;

        public UnitOfWork(AppDbContext context, IEventPublisher eventPublisher)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _eventPublisher = eventPublisher ?? throw new ArgumentNullException(nameof(eventPublisher));
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Collect domain events from tracked entities using reflection to support the generic AggregateRoot<TId>
            var domainEvents = _context.ChangeTracker
                .Entries()
                .Select(e => e.Entity)
                .Where(e => e is not null)
                .SelectMany(entity =>
                {
                    var prop = entity!.GetType().GetProperty("DomainEvents");
                    var events = prop?.GetValue(entity) as IEnumerable<IDomainEvent>;
                    return events ?? Enumerable.Empty<IDomainEvent>();
                })
                .ToList();

            // Persist changes first
            var result = await _context.SaveChangesAsync(cancellationToken);

            // Publish domain events after successful save
            if (domainEvents.Any())
            {
                await _eventPublisher.PublishAsync(domainEvents);

                // Clear domain events on entities that exposed them
                foreach (var entry in _context.ChangeTracker.Entries())
                {
                    var entity = entry.Entity;
                    var clearMethod = entity?.GetType().GetMethod("ClearDomainEvents");
                    if (clearMethod is not null)
                    {
                        clearMethod.Invoke(entity, null);
                    }
                }
            }

            return result;
        }

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is not null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction is null)
            {

                //throw new InvalidOperationException("No transaction in progress.");
                return;
            }

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }
    }
}
