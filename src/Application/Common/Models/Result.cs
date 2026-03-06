using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Models
{
    public sealed class Result<T>
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T Value { get; }
        public Error Error { get; }

        private Result(bool isSuccess, T value, Error error)
        {
            if (isSuccess && error != Error.None)
                throw new InvalidOperationException("Success result cannot have an error.");

            if (!isSuccess && error == Error.None)
                throw new InvalidOperationException("Failure result must have an error.");

            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new(true, value, Error.None);
        public static Result<T> Failure(Error error) => new(false, default!, error);

        public static implicit operator Result<T>(T value) => Success(value);
        public static implicit operator Result<T>(Error error) => Failure(error);

        public TResult Match<TResult>(
            Func<T, TResult> onSuccess,
            Func<Error, TResult> onFailure)
        {
            return IsSuccess ? onSuccess(Value) : onFailure(Error);
        }
    }
}
