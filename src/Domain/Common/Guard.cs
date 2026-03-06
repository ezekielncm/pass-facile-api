namespace Domain.Common
{
    public static class Guard
    {
        public static class Against
        {
            /// <summary>
            /// Guards against null values.
            /// </summary>
            public static void Null<T>(T value, string parameterName, string? message = null)
            {
                if (value is null)
                {
                    throw new ArgumentNullException(parameterName,
                        message ?? $"{parameterName} cannot be null.");
                }
            }

            /// <summary>
            /// Guards against null or empty strings.
            /// </summary>
            public static void NullOrEmpty(string value, string parameterName, string? message = null)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} cannot be null or empty.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against null or empty collections.
            /// </summary>
            public static void NullOrEmpty<T>(IEnumerable<T> value, string parameterName, string? message = null)
            {
                Null(value, parameterName, message);

                if (!value.Any())
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} cannot be empty.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against negative numbers.
            /// </summary>
            public static void Negative(int value, string parameterName, string? message = null)
            {
                if (value < 0)
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} cannot be negative.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against negative or zero numbers.
            /// </summary>
            public static void NegativeOrZero(int value, string parameterName, string? message = null)
            {
                if (value <= 0)
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} must be greater than zero.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against negative decimal values.
            /// </summary>
            public static void Negative(decimal value, string parameterName, string? message = null)
            {
                if (value < 0)
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} cannot be negative.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against values outside a valid range.
            /// </summary>
            public static void OutOfRange<T>(T value, string parameterName, T min, T max, string? message = null)
                where T : IComparable<T>
            {
                if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                {
                    throw new ArgumentOutOfRangeException(
                        parameterName,
                        value,
                        message ?? $"{parameterName} must be between {min} and {max}.");
                }
            }

            /// <summary>
            /// Guards against strings exceeding maximum length.
            /// </summary>
            public static void StringTooLong(string value, string parameterName, int maxLength, string? message = null)
            {
                if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} cannot exceed {maxLength} characters.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against strings shorter than minimum length.
            /// </summary>
            public static void StringTooShort(string value, string parameterName, int minLength, string? message = null)
            {
                if (!string.IsNullOrEmpty(value) && value.Length < minLength)
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} must be at least {minLength} characters.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against dates in the past.
            /// </summary>
            public static void PastDate(DateTime value, string parameterName, string? message = null)
            {
                if (value < DateTime.UtcNow)
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} cannot be in the past.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against dates in the future.
            /// </summary>
            public static void FutureDate(DateTime value, string parameterName, string? message = null)
            {
                if (value > DateTime.UtcNow)
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} cannot be in the future.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against invalid email format.
            /// </summary>
            public static void InvalidEmail(string email, string parameterName, string? message = null)
            {
                if (!string.IsNullOrEmpty(email) && !System.Text.RegularExpressions.Regex.IsMatch(
                    email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} is not a valid email address.",
                        parameterName);
                }
            }

            /// <summary>
            /// Guards against a false condition.
            /// </summary>
            public static void False(bool condition, string message)
            {
                if (!condition)
                {
                    throw new BusinessRuleValidationException("BUSINESS_RULE_VIOLATION", message);
                }
            }

            /// <summary>
            /// Guards against a true condition.
            /// </summary>
            public static void True(bool condition, string message)
            {
                if (condition)
                {
                    throw new BusinessRuleValidationException("BUSINESS_RULE_VIOLATION", message);
                }
            }

            /// <summary>
            /// Guards with a custom validation function.
            /// </summary>
            public static void InvalidInput<T>(T value, Func<T, bool> predicate, string parameterName, string? message = null)
            {
                if (!predicate(value))
                {
                    throw new ArgumentException(
                        message ?? $"{parameterName} has an invalid value.",
                        parameterName);
                }
            }
        }

        /// <summary>
        /// Ensures a business rule is satisfied.
        /// </summary>
        public static void Ensure(bool condition, string errorCode, string errorMessage)
        {
            if (!condition)
            {
                throw new BusinessRuleValidationException(errorCode, errorMessage);
            }
        }
    }
}