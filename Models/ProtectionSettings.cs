using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LLMSensitiveDataGoverance.Core.Models
{
    /// <summary>
    /// Represents the protection settings for a sensitivity label,
    /// defining what operations are allowed or restricted.
    /// </summary>
    public class ProtectionSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether the content must be encrypted.
        /// </summary>
        public bool RequireEncryption { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether content extraction is prevented.
        /// </summary>
        public bool PreventExtraction { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether copy/paste operations are prevented.
        /// </summary>
        public bool PreventCopyPaste { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the content can be used for grounding.
        /// </summary>
        public bool PreventGrounding { get; set; } = false;

        /// <summary>
        /// Gets or sets the list of users allowed to access the content.
        /// </summary>
        public List<string> AllowedUsers { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of groups allowed to access the content.
        /// </summary>
        public List<string> AllowedGroups { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the encryption algorithm to use (e.g., "AES-256", "RSA").
        /// </summary>
        [StringLength(50)]
        public string EncryptionAlgorithm { get; set; } = "AES-256";

        /// <summary>
        /// Gets or sets the expiration time for the protection settings.
        /// </summary>
        public DateTime? ExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether watermarking is required.
        /// </summary>
        public bool RequireWatermark { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether audit logging is required.
        /// </summary>
        public bool RequireAuditLog { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum number of access attempts allowed.
        /// </summary>
        public int MaxAccessAttempts { get; set; } = -1; // -1 means unlimited

        /// <summary>
        /// Gets or sets custom protection rules.
        /// </summary>
        public Dictionary<string, bool> CustomRules { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Determines if the protection settings are restrictive.
        /// </summary>
        public bool IsRestrictive => RequireEncryption || PreventExtraction || PreventCopyPaste || PreventGrounding;

        /// <summary>
        /// Returns a string representation of the protection settings.
        /// </summary>
        public override string ToString()
        {
            var restrictions = new List<string>();
            if (RequireEncryption) restrictions.Add("Encryption");
            if (PreventExtraction) restrictions.Add("No Extraction");
            if (PreventCopyPaste) restrictions.Add("No Copy/Paste");
            if (PreventGrounding) restrictions.Add("No Grounding");
            
            return restrictions.Count > 0 ? string.Join(", ", restrictions) : "No Restrictions";
        }
    }
}
