using System.Diagnostics;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LLMSensitiveDataGoverance.Examples.PerformanceTesting;

/// <summary>
/// Performance testing suite for LLM Sensitive Data Governance system
/// </summary>
public class PerformanceTestRunner
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PerformanceTestRunner> _logger;
    
    public PerformanceTestRunner(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<PerformanceTestRunner>>();
    }
    
    /// <summary>
    /// Run comprehensive performance tests
    /// </summary>
    public async Task RunAllTestsAsync()
    {
        _logger.LogInformation("Starting Performance Tests");
        
        await RunClassificationPerformanceTest();
        await RunBatchProcessingPerformanceTest();
        await RunConcurrentAccessTest();
        await RunMemoryUsageTest();
        await RunLabelValidationPerformanceTest();
        
        _logger.LogInformation("Performance Tests Completed");
    }
    
    /// <summary>
    /// Test classification performance with various content sizes
    /// </summary>
    private async Task RunClassificationPerformanceTest()
    {
        _logger.LogInformation("=== Classification Performance Test ===");
        
        var labelService = _serviceProvider.GetRequiredService<ISensitivityLabelService>();
        var testSizes = new[] { 100, 1000, 10000, 100000 };
        
        foreach (var size in testSizes)
        {
            var testData = GenerateTestContent(size);
            var groundingData = new GroundingData
            {
                Id = $"perf-test-{size}",
                Content = testData,
                Source = "Performance Test",
                DataType = "text/plain"
            };
            
            var stopwatch = Stopwatch.StartNew();
            var result = await labelService.ClassifyAsync(groundingData);
            stopwatch.Stop();
            
            _logger.LogInformation($"Content Size: {size:N0} chars, Time: {stopwatch.ElapsedMilliseconds}ms, Label: {result.Label.Name}");
        }
    }
    
    /// <summary>
    /// Test batch processing performance
    /// </summary>
    private async Task RunBatchProcessingPerformanceTest()
    {
        _logger.LogInformation("\n=== Batch Processing Performance Test ===");
        
        var labelService = _serviceProvider.GetRequiredService<ISensitivityLabelService>();
        var batchSizes = new[] { 10, 50, 100, 500 };
        
        foreach (var batchSize in batchSizes)
        {
            var testData = GenerateTestDataBatch(batchSize);
            
            var stopwatch = Stopwatch.StartNew();
            var tasks = testData.Select(data => labelService.ClassifyAsync(data));
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();
            
            var avgTimePerItem = (double)stopwatch.ElapsedMilliseconds / batchSize;
            _logger.LogInformation($"Batch Size: {batchSize:N0}, Total Time: {stopwatch.ElapsedMilliseconds}ms, Avg per item: {avgTimePerItem:F2}ms");
        }
    }
    
    /// <summary>
    /// Test concurrent access performance
    /// </summary>
    private async Task RunConcurrentAccessTest()
    {
        _logger.LogInformation("\n=== Concurrent Access Performance Test ===");
        
        var labelService = _serviceProvider.GetRequiredService<ISensitivityLabelService>();
        var concurrencyLevels = new[] { 1, 5, 10, 20, 50 };
        
        foreach (var concurrency in concurrencyLevels)
        {
            var testData = GenerateTestDataBatch(concurrency);
            
            var stopwatch = Stopwatch.StartNew();
            
            var semaphore = new SemaphoreSlim(concurrency, concurrency);
            var tasks = testData.Select(async data =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await labelService.ClassifyAsync(data);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            
            var results = await Task.WhenAll(tasks);
            stopwatch.Stop();
            
            var throughput = (double)concurrency / stopwatch.ElapsedMilliseconds * 1000;
            _logger.LogInformation($"Concurrency: {concurrency}, Time: {stopwatch.ElapsedMilliseconds}ms, Throughput: {throughput:F2} ops/sec");
        }
    }
    
    /// <summary>
    /// Test memory usage patterns
    /// </summary>
    private async Task RunMemoryUsageTest()
    {
        _logger.LogInformation("\n=== Memory Usage Performance Test ===");
        
        var labelService = _serviceProvider.GetRequiredService<ISensitivityLabelService>();
        
        // Force garbage collection before test
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var initialMemory = GC.GetTotalMemory(false);
        _logger.LogInformation($"Initial Memory: {initialMemory:N0} bytes");
        
        // Process large batch
        var largeTestData = GenerateTestDataBatch(1000);
        var stopwatch = Stopwatch.StartNew();
        
        var tasks = largeTestData.Select(data => labelService.ClassifyAsync(data));
        var results = await Task.WhenAll(tasks);
        
        stopwatch.Stop();
        
        var currentMemory = GC.GetTotalMemory(false);
        var memoryUsed = currentMemory - initialMemory;
        
        _logger.LogInformation($"Memory after processing: {currentMemory:N0} bytes");
        _logger.LogInformation($"Memory used: {memoryUsed:N0} bytes");
        _logger.LogInformation($"Memory per operation: {memoryUsed / 1000:N0} bytes");
        _logger.LogInformation($"Processing time: {stopwatch.ElapsedMilliseconds}ms");
        
        // Force cleanup
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var finalMemory = GC.GetTotalMemory(false);
        var memoryFreed = currentMemory - finalMemory;
        
        _logger.LogInformation($"Memory after cleanup: {finalMemory:N0} bytes");
        _logger.LogInformation($"Memory freed: {memoryFreed:N0} bytes");
    }
    
    /// <summary>
    /// Test label validation performance
    /// </summary>
    private async Task RunLabelValidationPerformanceTest()
    {
        _logger.LogInformation("\n=== Label Validation Performance Test ===");
        
        var labelValidator = _serviceProvider.GetRequiredService<ILabelValidator>();
        var labelRepository = _serviceProvider.GetRequiredService<ILabelRepository>();
        
        var allLabels = await labelRepository.GetAllAsync();
        var testLabels = allLabels.ToList();
        
        var stopwatch = Stopwatch.StartNew();
        
        var validationTasks = testLabels.Select(label => labelValidator.ValidateAsync(label));
        var validationResults = await Task.WhenAll(validationTasks);
        
        stopwatch.Stop();
        
        var successCount = validationResults.Count(r => r);
        var avgTimePerValidation = (double)stopwatch.ElapsedMilliseconds / testLabels.Count;
        
        _logger.LogInformation($"Labels validated: {testLabels.Count}");
        _logger.LogInformation($"Successful validations: {successCount}");
        _logger.LogInformation($"Total time: {stopwatch.ElapsedMilliseconds}ms");
        _logger.LogInformation($"Average time per validation: {avgTimePerValidation:F2}ms");
    }
    
    /// <summary>
    /// Generate test content of specified size
    /// </summary>
    private string GenerateTestContent(int size)
    {
        var content = new System.Text.StringBuilder(size);
        var baseText = "This is sample confidential business data containing sensitive information. ";
        
        while (content.Length < size)
        {
            content.Append(baseText);
        }
        
        return content.ToString(0, Math.Min(size, content.Length));
    }
    
    /// <summary>
    /// Generate batch of test data
    /// </summary>
    private List<GroundingData> GenerateTestDataBatch(int batchSize)
    {
        var testData = new List<GroundingData>();
        var contentTemplates = new[]
        {
            "Employee salary information: {0}",
            "Financial report data: {0}",
            "Customer personal information: {0}",
            "Internal business strategy: {0}",
            "Technical specifications: {0}",
            "Marketing campaign details: {0}",
            "Legal contract terms: {0}",
            "Research and development: {0}"
        };
        
        for (int i = 0; i < batchSize; i++)
        {
            var template = contentTemplates[i % contentTemplates.Length];
            var content = string.Format(template, $"Record {i + 1}");
            
            testData.Add(new GroundingData
            {
                Id = $"batch-test-{i + 1}",
                Content = content,
                Source = "Performance Test",
                DataType = "text/plain",
                Metadata = new Dictionary<string, object>
                {
                    { "batchId", i + 1 },
                    { "testRun", DateTime.UtcNow.Ticks }
                }
            });
        }
        
        return testData;
    }
}