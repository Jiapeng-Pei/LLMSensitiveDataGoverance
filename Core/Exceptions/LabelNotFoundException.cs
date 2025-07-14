using System;

namespace LLMSensitiveDataGoverance.Core.Exceptions
{
    /// <summary>
    /// The exception that is thrown when a sensitivity label is not found.
    /// </summary>
    public class LabelNotFoundException : SensitivityLabelException
    {
        /// <summary>
        /// Gets the identifier that was searched for.
        /// </summary>
        public string? SearchIdentifier { get; }

        /// <summary>
        /// Gets the search type (e.g., "ID", "Name").
        /// </summary>
        public string? SearchType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelNotFoundException"/> class.
        /// </summary>
        public LabelNotFoundException() : base("Sensitivity label not found")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelNotFoundException"/> class with a specified identifier.
        /// </summary>
        /// <param name="identifier">The identifier that was not found.</param>
        public LabelNotFoundException(string identifier) : base($"Sensitivity label '{identifier}' not found")
        {
            SearchIdentifier = identifier;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelNotFoundException"/> class with search details.
        /// </summary>
        /// <param name="identifier">The identifier that was searched for.</param>
        /// <param name="searchType">The type of search performed.</param>
        public LabelNotFoundException(string identifier, string searchType) 
            : base($"Sensitivity label with {searchType} '{identifier}' not found")
        {
            SearchIdentifier = identifier;
            SearchType = searchType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LabelNotFoundException"/> class with inner exception.
        /// </summary>
        /// <param name="identifier">The identifier that was not found.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public LabelNotFoundException(string identifier, Exception innerException) 
            : base($"Sensitivity label '{identifier}' not found", innerException)
        {
            SearchIdentifier = identifier;
        }

        /// <summary>
        /// Returns a string representation of the exception.
        /// </summary>
        public override string ToString()
        {
            var result = base.ToString();
            if (!string.IsNullOrEmpty(SearchIdentifier))
            {
                result = $"Search Identifier: {SearchIdentifier}\n{result}";
            }
            if (!string.IsNullOrEmpty(SearchType))
            {
                result = $"Search Type: {SearchType}\n{result}";
            }
            return result;
        }
    }
}
