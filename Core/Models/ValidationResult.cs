using System;
using System.Collections.Generic;
using System.Linq;

namespace LLMSensitiveDataGoverance.Core.Models
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid { get; set; } = true;

        /// <summary>
        /// Gets or sets the error message if validation failed.
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of validation errors.
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

        /// <summary>
        /// Gets or sets the list of validation warnings.
        /// </summary>
        public List<ValidationWarning> Warnings { get; set; } = new List<ValidationWarning>();

        /// <summary>
        /// Gets or sets the timestamp when the validation was performed.
        /// </summary>
        public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates a failed validation result with an error message.
        /// </summary>
        public static ValidationResult Failure(string errorMessage)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = errorMessage,
                Errors = new List<ValidationError> { new ValidationError { Message = errorMessage } }
            };
        }

        /// <summary>
        /// Creates a failed validation result with multiple errors.
        /// </summary>
        public static ValidationResult Failure(IEnumerable<ValidationError> errors)
        {
            var errorList = errors.ToList();
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = string.Join("; ", errorList.Select(e => e.Message)),
                Errors = errorList
            };
        }

        /// <summary>
        /// Adds an error to the validation result.
        /// </summary>
        public void AddError(string message, string? field = null)
        {
            Errors.Add(new ValidationError { Message = message, Field = field });
            IsValid = false;
            if (string.IsNullOrEmpty(ErrorMessage))
            {
                ErrorMessage = message;
            }
        }

        /// <summary>
        /// Adds a warning to the validation result.
        /// </summary>
        public void AddWarning(string message, string? field = null)
        {
            Warnings.Add(new ValidationWarning { Message = message, Field = field });
        }

        /// <summary>
        /// Returns a string representation of the validation result.
        /// </summary>
        public override string ToString()
        {
            if (IsValid)
            {
                return $"Valid ({Warnings.Count} warnings)";
            }
            return $"Invalid: {ErrorMessage}";
        }
    }

    /// <summary>
    /// Represents a validation error.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// Gets or sets the error message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the field that caused the error.
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Returns a string representation of the validation error.
        /// </summary>
        public override string ToString()
        {
            return Field != null ? $"{Field}: {Message}" : Message;
        }
    }

    /// <summary>
    /// Represents a validation warning.
    /// </summary>
    public class ValidationWarning
    {
        /// <summary>
        /// Gets or sets the warning message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the field that caused the warning.
        /// </summary>
        public string? Field { get; set; }

        /// <summary>
        /// Gets or sets the warning code.
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Returns a string representation of the validation warning.
        /// </summary>
        public override string ToString()
        {
            return Field != null ? $"{Field}: {Message}" : Message;
        }
    }
}
