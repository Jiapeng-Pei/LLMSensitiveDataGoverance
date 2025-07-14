using System;

namespace LLMSensitiveDataGoverance.Core.Exceptions
{
    /// <summary>
    /// The base exception class for sensitivity label-related errors.
    /// </summary>
    public class SensitivityLabelException : Exception
    {
        /// <summary>
        /// Gets the label identifier associated with the exception.
        /// </summary>
        public string? LabelId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitivityLabelException"/> class.
        /// </summary>
        public SensitivityLabelException() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitivityLabelException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public SensitivityLabelException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitivityLabelException"/> class with a specified error message and label identifier.
        /// </summary>
        /// <param name="labelId">The label identifier associated with the exception.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public SensitivityLabelException(string labelId, string message) : base(message)
        {
            LabelId = labelId;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitivityLabelException"/> class with a specified error message and inner exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SensitivityLabelException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SensitivityLabelException"/> class with a specified error message, label identifier, and inner exception.
        /// </summary>
        /// <param name="labelId">The label identifier associated with the exception.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public SensitivityLabelException(string labelId, string message, Exception innerException) : base(message, innerException)
        {
            LabelId = labelId;
        }

        /// <summary>
        /// Returns a string representation of the exception.
        /// </summary>
        public override string ToString()
        {
            var result = base.ToString();
            if (!string.IsNullOrEmpty(LabelId))
            {
                result = $"Label ID: {LabelId}\n{result}";
            }
            return result;
        }
    }
}