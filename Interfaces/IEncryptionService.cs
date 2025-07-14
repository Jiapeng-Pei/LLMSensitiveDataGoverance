using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Interfaces
{
    /// <summary>
    /// Defines the contract for encryption service operations.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts content based on the sensitivity label's protection settings.
        /// </summary>
        /// <param name="content">The content to encrypt.</param>
        /// <param name="label">The sensitivity label containing encryption settings.</param>
        /// <returns>A task representing the encrypted content.</returns>
        Task<string> EncryptAsync(string content, SensitivityLabel label);

        /// <summary>
        /// Decrypts content based on the sensitivity label's protection settings.
        /// </summary>
        /// <param name="encryptedContent">The encrypted content to decrypt.</param>
        /// <param name="label">The sensitivity label containing decryption settings.</param>
        /// <returns>A task representing the decrypted content.</returns>
        Task<string> DecryptAsync(string encryptedContent, SensitivityLabel label);

        /// <summary>
        /// Determines if the encrypted content can be decrypted with the given label.
        /// </summary>
        /// <param name="encryptedContent">The encrypted content to check.</param>
        /// <param name="label">The sensitivity label to use for decryption.</param>
        /// <returns>A task representing whether decryption is possible.</returns>
        Task<bool> CanDecryptAsync(string encryptedContent, SensitivityLabel label);

        /// <summary>
        /// Determines if content should be encrypted based on the sensitivity label.
        /// </summary>
        /// <param name="label">The sensitivity label to evaluate.</param>
        /// <returns>A task representing whether encryption is required.</returns>
        Task<bool> ShouldEncryptAsync(SensitivityLabel label);

        /// <summary>
        /// Generates an encryption key for the given sensitivity label.
        /// </summary>
        /// <param name="label">The sensitivity label to generate a key for.</param>
        /// <returns>A task representing the generated encryption key.</returns>
        Task<string> GenerateKeyAsync(SensitivityLabel label);

        /// <summary>
        /// Validates if an encryption key is valid for the given sensitivity label.
        /// </summary>
        /// <param name="key">The encryption key to validate.</param>
        /// <param name="label">The sensitivity label to validate against.</param>
        /// <returns>A task representing whether the key is valid.</returns>
        Task<bool> ValidateKeyAsync(string key, SensitivityLabel label);

        /// <summary>
        /// Gets the supported encryption algorithms.
        /// </summary>
        /// <returns>A task representing the collection of supported algorithms.</returns>
        Task<IEnumerable<string>> GetSupportedAlgorithmsAsync();
    }
}
