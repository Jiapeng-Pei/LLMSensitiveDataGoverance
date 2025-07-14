using System;

namespace LLMSensitiveDataGoverance.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when access to a sensitivity label is unauthorized.
    /// </summary>
    public class UnauthorizedLabelAccessException : SensitivityLabelException
    {
        /// <summary>
        /// Gets the user identifier that attempted the access.
        /// </summary>
        public string? UserId { get; }

        /// <summary>
        /// Gets the operation that was attempted.
        /// </summary>
        public string? Operation { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedLabelAccessException"/> class.
        /// </summary>
        public UnauthorizedLabelAccessException() : base("Unauthorized access to sensitivity label")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedLabelAccessException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public UnauthorizedLabelAccessException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedLabelAccessException"/> class with user and label details.
        /// </summary>
        /// <param name="userId">The user identifier that attempted the access.</param>
        /// <param name="labelId">The label identifier that was accessed.</param>
        public UnauthorizedLabelAccessException(string userId, string labelId) 
            : base(labelId, $"User '{userId}' is not authorized to access label '{labelId}'")
        {
            UserId = userId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedLabelAccessException"/> class with full details.
        /// </summary>
        /// <param name="userId">The user identifier that attempted the access.</param>
        /// <param name="labelId">The label identifier that was accessed.</param>
        /// <param name="operation">The operation that was attempted.</param>
        public UnauthorizedLabelAccessException(string userId, string labelId, string operation) 
            : base(labelId, $"User '{userId}' is not authorized to perform operation '{operation}' on label '{labelId}'")
        {
            UserId = userId;
            Operation = operation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauthorizedLabelAccessException"/> class with inner exception.
        /// </summary>
        /// <param name="userId">The user identifier that attempted the access.</param>
        /// <param name="labelId">The label identifier that was accessed.</param>
        /// <param name="operation">The operation that was attempted.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public UnauthorizedLabelAccessException(string userId, string labelId, string operation, Exception innerException) 
            : base(labelId, $"User '{userId}' is not authorized to perform operation '{operation}' on label '{labelId}'", innerException)
        {
            UserId = userId;
            Operation = operation;
        }

        /// <summary>
        /// Returns a string representation of the exception.
        /// </summary>
        public override string ToString()
        {
            var result = base.ToString();
            if (!string.IsNullOrEmpty(UserId))
            {
                result = $"User ID: {UserId}\n{result}";
            }
            if (!string.IsNullOrEmpty(Operation))
            {
                result = $"Operation: {Operation}\n{result}";
            }
            return result;
        }
    }
}