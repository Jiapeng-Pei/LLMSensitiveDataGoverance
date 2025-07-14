using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Exceptions;

namespace LLMSensitiveDataGoverance.Core.Services
{
    /// <summary>
    /// Service for encrypting and decrypting sensitive content
    /// </summary>
    public class EncryptionService : IEncryptionService
    {
        private readonly byte[] _defaultKey;
        private readonly byte[] _defaultIV;

        /// <summary>
        /// Initializes a new instance of the EncryptionService
        /// </summary>
        public EncryptionService()
        {
            // In a real implementation, keys should be loaded from secure key management
            // This is a simplified example for demonstration
            _defaultKey = Encoding.UTF8.GetBytes("DefaultKey123456789012345678901234"); // 32 bytes for AES-256
            _defaultIV = Encoding.UTF8.GetBytes("DefaultIV1234567"); // 16 bytes for AES
        }

        /// <summary>
        /// Encrypts content using the label's encryption settings
        /// </summary>
        /// <param name="content">Content to encrypt</param>
        /// <param name="label">Label containing encryption settings</param>
        /// <returns>Encrypted content</returns>
        /// <exception cref="EncryptionException">Thrown when encryption fails</exception>
        public async Task<string> EncryptAsync(string content, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            if (label == null)
                throw new ArgumentNullException(nameof(label));

            if (!label.Protection.RequireEncryption)
                return content;

            try
            {
                using var aes = Aes.Create();
                aes.Key = GetEncryptionKey(label);
                aes.IV = GetEncryptionIV(label);

                using var encryptor = aes.CreateEncryptor();
                var contentBytes = Encoding.UTF8.GetBytes(content);
                var encryptedBytes = encryptor.TransformFinalBlock(contentBytes, 0, contentBytes.Length);

                var result = Convert.ToBase64String(encryptedBytes);
                
                // Add metadata for decryption
                var metadata = $"ENCRYPTED:{label.Id}:{result}";
                return await Task.FromResult(metadata);
            }
            catch (Exception ex)
            {
                throw new EncryptionException(label.Id, "Encrypt", ex.Message);
            }
        }

        /// <summary>
        /// Decrypts content using the label's encryption settings
        /// </summary>
        /// <param name="encryptedContent">Encrypted content to decrypt</param>
        /// <param name="label">Label containing encryption settings</param>
        /// <returns>Decrypted content</returns>
        /// <exception cref="EncryptionException">Thrown when decryption fails</exception>
        public async Task<string> DecryptAsync(string encryptedContent, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(encryptedContent))
                return encryptedContent;

            if (label == null)
                throw new ArgumentNullException(nameof(label));

            if (!encryptedContent.StartsWith("ENCRYPTED:"))
                return encryptedContent;

            try
            {
                var parts = encryptedContent.Split(':', 3);
                if (parts.Length != 3 || parts[1] != label.Id)
                    throw new EncryptionException(label.Id, "Decrypt", "Invalid encrypted content format or label mismatch");

                var encryptedBytes = Convert.FromBase64String(parts[2]);

                using var aes = Aes.Create();
                aes.Key = GetEncryptionKey(label);
                aes.IV = GetEncryptionIV(label);

                using var decryptor = aes.CreateDecryptor();
                var decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

                return await Task.FromResult(Encoding.UTF8.GetString(decryptedBytes));
            }
            catch (Exception ex)
            {
                throw new EncryptionException(label.Id, "Decrypt", ex.Message);
            }
        }

        /// <summary>
        /// Determines if content can be decrypted with the given label
        /// </summary>
        /// <param name="encryptedContent">Encrypted content to check</param>
        /// <param name="label">Label to check against</param>
        /// <returns>True if content can be decrypted, false otherwise</returns>
        public async Task<bool> CanDecryptAsync(string encryptedContent, SensitivityLabel label)
        {
            if (string.IsNullOrEmpty(encryptedContent) || label == null)
                return false;

            if (!encryptedContent.StartsWith("ENCRYPTED:"))
                return false;

            var parts = encryptedContent.Split(':', 3);
            if (parts.Length != 3)
                return false;

            return await Task.FromResult(parts[1] == label.Id);
        }

        /// <summary>
        /// Determines if content should be encrypted based on label settings
        /// </summary>
        /// <param name="label">Label to check</param>
        /// <returns>True if content should be encrypted, false otherwise</returns>
        public async Task<bool> ShouldEncryptAsync(SensitivityLabel label)
        {
            if (label == null)
                return false;

            var shouldEncrypt = label.Protection.RequireEncryption && 
                               label.Priority >= LabelPriority.Confidential;

            return await Task.FromResult(shouldEncrypt);
        }

        /// <summary>
        /// Gets encryption key for the label
        /// </summary>
        private byte[] GetEncryptionKey(SensitivityLabel label)
        {
            // In a real implementation, this would retrieve label-specific keys
            // from a secure key management system
            return _defaultKey;
        }

        /// <summary>
        /// Gets encryption IV for the label
        /// </summary>
        private byte[] GetEncryptionIV(SensitivityLabel label)
        {
            // In a real implementation, this would generate or retrieve 
            // label-specific IVs from a secure key management system
            return _defaultIV;
        }
    }
}
