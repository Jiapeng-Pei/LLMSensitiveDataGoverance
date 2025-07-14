using System;
using System.Collections.Generic;

namespace SensitivityLabelSystem.Core.Models
{
    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the validation was successful.
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the error message if validation failed.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the collection of validation errors.
        /// </summary>
        public IList<string> Errors { get; set; }

        /// <summary>
        /// Gets or sets additional context information about the validation.
        /// </summary>
        public Dictionary<string, object> Context { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResult"/> class.
        /// </summary>
        public ValidationResult()
        {
            Errors = new List<string>();
            Context = new Dictionary<string, object>();
        }

        /// <summary>
        /// Creates a successful validation result.
        /// </summary>
        /// <returns>A successful validation result.</returns>
        public static ValidationResult Success()
        {
            return new ValidationResult { IsValid = true };
        }

        /// <summary>
        /// Creates a failed validation result with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        /// <returns>A failed validation result.</returns>
        public static ValidationResult Failure(string errorMessage)
        {
            return new ValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = errorMessage,
                Errors = new List<string> { errorMessage }
            };
        }

        /// <summary>
        /// Creates a failed validation result with multiple error messages.
        /// </summary>
        /// <param name="errors">The collection of error messages.</param>
        /// <returns>A failed validation result.</returns>
        public static ValidationResult Failure(IEnumerable<string> errors)
        {
            var errorsList = new List<string>(errors);
            return new ValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = string.Join("; ", errorsList),
                Errors = errorsList
            };
        }
    }
}
