using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Services;

namespace LLMSensitiveDataGoverance.Core.Utilities
{
    /// <summary>
    /// Utility for analyzing content to detect sensitive patterns
    /// </summary>
    public class ContentAnalyzer
    {
        private readonly Dictionary<string, Regex> _sensitivePatterns;
        private readonly Dictionary<string, string> _patternRiskLevels;

        /// <summary>
        /// Initializes a new instance of the ContentAnalyzer
        /// </summary>
        public ContentAnalyzer()
        {
            _sensitivePatterns = InitializeSensitivePatterns();
            _patternRiskLevels = InitializePatternRiskLevels();
        }

        /// <summary>
        /// Analyzes content for sensitive patterns
        /// </summary>
        /// <param name="content">Content to analyze</param>
        /// <param name="metadata">Additional metadata</param>
        /// <returns>Analysis result</returns>
        public async Task<ContentAnalysisResult> AnalyzeAsync(string content, Dictionary<string, object> metadata = null)
        {
            if (string.IsNullOrEmpty(content))
                return new ContentAnalysisResult { ConfidenceScore = 0.9 };

            var result = new ContentAnalysisResult();
            var detectedPatterns = new List<DetectedPattern>();

            // Analyze for each pattern type
            foreach (var pattern in _sensitivePatterns)
            {
                var matches = pattern.Value.Matches(content);
                foreach (Match match in matches)
                {
                    var detectedPattern = new DetectedPattern
                    {
                        PatternType = pattern.Key,
                        RiskLevel = _patternRiskLevels[pattern.Key],
                        Description = GetPatternDescription(pattern.Key),
                        StartIndex = match.Index,
                        Length = match.Length,
                        Confidence = CalculatePatternConfidence(pattern.Key, match.Value)
                    };

                    detectedPatterns.Add(detectedPattern);
                }
            }

            result.DetectedPatterns = detectedPatterns;
            result.ConfidenceScore = CalculateOverallConfidence(detectedPatterns);
            result.AnalysisMetadata = CreateAnalysisMetadata(content, metadata);

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Initializes sensitive pattern regex dictionary
        /// </summary>
        private Dictionary<string, Regex> InitializeSensitivePatterns()
        {
            return new Dictionary<string, Regex>
            {
                // Personal identification patterns
                { "SSN", new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.IgnoreCase) },
                { "CreditCard", new Regex(@"\b\d{4}[-\s]?\d{4}[-\s]?\d{4}[-\s]?\d{4}\b", RegexOptions.IgnoreCase) },
                { "Email", new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}\b", RegexOptions.IgnoreCase) },
                { "Phone", new Regex(@"\b\d{3}-\d{3}-\d{4}\b", RegexOptions.IgnoreCase) },
                
                // Financial patterns
                { "Financial", new Regex(@"\b(salary|wage|income|revenue|profit|loss|financial|budget|cost)\b", RegexOptions.IgnoreCase) },
                { "BankAccount", new Regex(@"\b\d{8,12}\b", RegexOptions.IgnoreCase) },
                
                // Confidential keywords
                { "Confidential", new Regex(@"\b(confidential|secret|private|proprietary|classified)\b", RegexOptions.IgnoreCase) },
                { "Internal", new Regex(@"\b(internal|company|corporate|organization)\b", RegexOptions.IgnoreCase) },
                
                // Personal data
                { "PersonalData", new Regex(@"\b(name|address|birthday|age|gender|nationality)\b", RegexOptions.IgnoreCase) },
                
                // Technical/IP patterns
                { "TechnicalIP", new Regex(@"\b(patent|trademark|copyright|algorithm|source code|technical)\b", RegexOptions.IgnoreCase) }
            };
        }

        /// <summary>
        /// Initializes pattern risk level mapping
        /// </summary>
        private Dictionary<string, string> InitializePatternRiskLevels()
        {
            return new Dictionary<string, string>
            {
                { "SSN", "Critical" },
                { "CreditCard", "Critical" },
                { "BankAccount", "Critical" },
                { "Financial", "High" },
                { "Confidential", "High" },
                { "PersonalData", "Medium" },
                { "TechnicalIP", "Medium" },
                { "Internal", "Low" },
                { "Email", "Low" },
                { "Phone", "Medium" }
            };
        }

        /// <summary>
        /// Gets description for pattern type
        /// </summary>
        private string GetPatternDescription(string patternType)
        {
            return patternType switch
            {
                "SSN" => "Social Security Number detected",
                "CreditCard" => "Credit card number detected",
                "BankAccount" => "Bank account number detected",
                "Financial" => "Financial information detected",
                "Confidential" => "Confidential keyword detected",
                "PersonalData" => "Personal data detected",
                "TechnicalIP" => "Technical intellectual property detected",
                "Internal" => "Internal company information detected",
                "Email" => "Email address detected",
                "Phone" => "Phone number detected",
                _ => "Sensitive pattern detected"
            };
        }

        /// <summary>
        /// Calculates confidence score for a specific pattern
        /// </summary>
        private double CalculatePatternConfidence(string patternType, string matchedValue)
        {
            return patternType switch
            {
                "SSN" => 0.95,
                "CreditCard" => 0.90,
                "BankAccount" => 0.85,
                "Financial" => 0.75,
                "Confidential" => 0.80,
                "PersonalData" => 0.70,
                "TechnicalIP" => 0.75,
                "Internal" => 0.65,
                "Email" => 0.90,
                "Phone" => 0.85,
                _ => 0.60
            };
        }

        /// <summary>
        /// Calculates overall confidence score
        /// </summary>
        private double CalculateOverallConfidence(List<DetectedPattern> patterns)
        {
            if (!patterns.Any())
                return 0.90; // High confidence for public content

            var criticalPatterns = patterns.Where(p => p.RiskLevel == "Critical").ToList();
            var highPatterns = patterns.Where(p => p.RiskLevel == "High").ToList();
            var mediumPatterns = patterns.Where(p => p.RiskLevel == "Medium").ToList();

            if (criticalPatterns.Any())
                return 0.95;
            if (highPatterns.Any())
                return 0.85;
            if (mediumPatterns.Any())
                return 0.75;

            return 0.65;
        }

        /// <summary>
        /// Creates analysis metadata
        /// </summary>
        private Dictionary<string, object> CreateAnalysisMetadata(string content, Dictionary<string, object> inputMetadata)
        {
            var metadata = new Dictionary<string, object>
            {
                { "ContentLength", content.Length },
                { "WordCount", content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length },
                { "AnalysisTimestamp", DateTime.UtcNow }
            };

            if (inputMetadata != null)
            {
                foreach (var item in inputMetadata)
                {
                    metadata[$"Input_{item.Key}"] = item.Value;
                }
            }

            return metadata;
        }
    }
}