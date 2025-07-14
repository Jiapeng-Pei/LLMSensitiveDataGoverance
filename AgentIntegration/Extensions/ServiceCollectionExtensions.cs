using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Services;
using LLMSensitiveDataGoverance.Core.Repositories;
using LLMSensitiveDataGoverance.AgentIntegration.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;

namespace LLMSensitiveDataGoverance.AgentIntegration.Extensions
{
    /// <summary>
    /// Extension methods for configuring dependency injection for agent integration
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the sensitivity label agent integration services to the DI container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configuration">The configuration instance</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddSensitivityLabelAgentIntegration(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // Configure settings
            services.Configure<AgentSettings>(configuration.GetSection("AgentSettings"));
            services.Configure<LabelConfiguration>(configuration.GetSection("LabelConfiguration"));

            // Register core services
            services.AddScoped<ISensitivityLabelService, SensitivityLabelService>();
            services.AddScoped<ILabelRepository, JsonLabelRepository>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<ILabelValidator, LabelValidationService>();

            // Register agent integration services
            services.AddScoped<AgentLabelProvider>();
            services.AddScoped<LLMResponseProcessor>();
            services.AddScoped<GroundingDataProcessor>();

            return services;
        }

        /// <summary>
        /// Adds the sensitivity label agent integration services with custom configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="configureAgent">Action to configure agent settings</param>
        /// <param name="configureLabel">Action to configure label settings</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddSensitivityLabelAgentIntegration(
            this IServiceCollection services,
            Action<AgentSettings> configureAgent = null,
            Action<LabelConfiguration> configureLabel = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Configure settings with defaults
            if (configureAgent != null)
                services.Configure(configureAgent);

            if (configureLabel != null)
                services.Configure(configureLabel);

            // Register core services
            services.AddScoped<ISensitivityLabelService, SensitivityLabelService>();
            services.AddScoped<ILabelRepository, InMemoryLabelRepository>();
            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<ILabelValidator, LabelValidationService>();

            // Register agent integration services
            services.AddScoped<AgentLabelProvider>();
            services.AddScoped<LLMResponseProcessor>();
            services.AddScoped<GroundingDataProcessor>();

            return services;
        }

        /// <summary>
        /// Adds minimal sensitivity label services for basic agent integration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddBasicSensitivityLabelIntegration(
            this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Register minimal services
            services.AddScoped<ISensitivityLabelService, SensitivityLabelService>();
            services.AddScoped<ILabelRepository, InMemoryLabelRepository>();
            services.AddScoped<AgentLabelProvider>();
            services.AddScoped<LLMResponseProcessor>();

            return services;
        }
    }
}
