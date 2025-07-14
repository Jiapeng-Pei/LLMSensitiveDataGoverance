using System;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a sensitivity label is invalid.
    /// </summary>
    public class InvalidLabelException : SensitivityLabelException
    {
        /// <summary>
        /// Gets the validation result associated with the exception.
        /// </summary>
        public ValidationResult? ValidationResult { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLabelException"/> class.
        /// </summary>
        public InvalidLabelException() : base("Invalid sensitivity label")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLabelException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidLabelException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLabelException"/> class with a specified label identifier and error message.
        /// </summary>
        /// <param name="labelId">The label identifier associated with the exception.</param>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public InvalidLabelException(string labelId, string message) : base(labelId, message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLabelException"/> class with validation result.
        /// </summary>
        /// <param name="labelId">The label identifier associated with the exception.</param>
        /// <param name="validationResult">The validation result containing detailed error information.</param>
        public InvalidLabelException(string labelId, ValidationResult validationResult) 
            : base(labelId, $"Invalid label: {validationResult.ErrorMessage}")
        {
            ValidationResult = validationResult;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidLabelException"/> class with validation result and inner exception.
        /// </summary>
        /// <param name="labelId">The label identifier associated with the exception.</param>
        /// <param name="validationResult">The validation result containing detailed error information.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public InvalidLabelException(string labelId, ValidationResult validationResult, Exception innerException) 
            : base(labelId, $"Invalid label: {validationResult.ErrorMessage}", innerException)
        {
            ValidationResult = validationResult;
        }

        /// <summary>
        /// Returns a string representation of the exception.
        /// </summary>
        public override string ToString()
        {
            var result = base.ToString();
            if (ValidationResult != null && ValidationResult.Errors.Count > 0)
            {
                result += $"\nValidation Errors:\n{string.Join("\n", ValidationResult.Errors.Select(e => $"- {e}"))}";
            }
            return result;
        }
    }
}