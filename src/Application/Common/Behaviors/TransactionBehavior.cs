using System;
using System.Collections.Generic;
using System.Text;
using Application.Common.Interfaces.Persistence;
using MediatR;

namespace Application.Common.Behaviors
{
    public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (typeof(TRequest).Name.EndsWith("Query"))
            {
                return await next();
            }
            //cancellationToken.ThrowIfCancellationRequested();
            bool transactionStarted = false;
            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                transactionStarted = true;
                var response = await next();
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return response;
            }
            catch
            {
                if (transactionStarted) await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
