using System;

namespace LLMSensitiveDataGoverance.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when an encryption operation fails.
    /// </summary>
    public class EncryptionException : SensitivityLabelException
    {
        /// <summary>
        /// Gets the encryption operation that failed.
        /// </summary>
        public string? Operation { get; }

        /// <summary>
        /// Gets the encryption algorithm that was used.
        /// </summary>
        public string? Algorithm { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionException"/> class.
        /// </summary>
        public EncryptionException() : base("Encryption operation failed")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public EncryptionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionException"/> class with a specified operation and error message.
        /// </summary>
        /// <param name="operation">The encryption operation that failed.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public EncryptionException(string operation, string message) : base(message)
        {
            Operation = operation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionException"/> class with label identifier, operation, and error message.
        /// </summary>
        /// <param name="labelId">The label identifier associated with the exception.</param>
        /// <param name="operation">The encryption operation that failed.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public EncryptionException(string labelId, string operation, string message) 
            : base(labelId, $"Encryption operation '{operation}' failed: {message}")
        {
            Operation = operation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionException"/> class with full details.
        /// </summary>
        /// <param name="labelId">The label identifier associated with the exception.</param>
        /// <param name="operation">The encryption operation that failed.</param>
        /// <param name="algorithm">The encryption algorithm that was used.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public EncryptionException(string labelId, string operation, string algorithm, string message) 
            : base(labelId, $"Encryption operation '{operation}' using algorithm '{algorithm}' failed: {message}")
        {
            Operation = operation;
            Algorithm = algorithm;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptionException"/> class with inner exception.
        /// </summary>
        /// <param name="labelId">The label identifier associated with the exception.</param>
        /// <param name="operation">The encryption operation that failed.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public EncryptionException(string labelId, string operation, string message, Exception innerException) 
            : base(labelId, $"Encryption operation '{operation}' failed: {message}", innerException)
        {
            Operation = operation;
        }

        /// <summary>
        /// Returns a string representation of the exception.
        /// </summary>
        public override string ToString()
        {
            var result = base.ToString();
            if (!string.IsNullOrEmpty(Operation))
            {
                result = $"Operation: {Operation}\n{result}";
            }
            if (!string.IsNullOrEmpty(Algorithm))
            {
                result = $"Algorithm: {Algorithm}\n{result}";
            }
            return result;
        }
    }
}
