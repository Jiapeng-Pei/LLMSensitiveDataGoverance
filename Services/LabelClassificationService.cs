using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LLMSensitiveDataGoverance.Core.Models;
using LLMSensitiveDataGoverance.Core.Interfaces;
using LLMSensitiveDataGoverance.Core.Utilities;

namespace LLMSensitiveDataGoverance.Core.Services
{
    /// <summary>
    /// Service for classifying content and determining appropriate sensitivity labels
    /// </summary>
    public class LabelClassificationService
    {
        private readonly ILabelRepository _labelRepository;
        private readonly ContentAnalyzer _contentAnalyzer;
        private readonly Dictionary<string, LabelPriority> _keywordPriorityMap;

        /// <summary>
        /// Initializes a new instance of the LabelClassificationService
        /// </summary>
        /// <param name="labelRepository">Repository for label data access</param>
        /// <param name="contentAnalyzer">Utility for content analysis</param>
        public LabelClassificationService(ILabelRepository labelRepository, ContentAnalyzer contentAnalyzer)
        {
            _labelRepository = labelRepository ?? throw new ArgumentNullException(nameof(labelRepository));
            _contentAnalyzer = contentAnalyzer ?? throw new ArgumentNullException(nameof(contentAnalyzer));
            _keywordPriorityMap = InitializeKeywordPriorityMap();
        }

        /// <summary>
        /// Classifies content and returns classification result
        /// </summary>
        /// <param name="content">Content to classify</param>
        /// <param name="metadata">Additional metadata for classification</param>
        /// <returns>Classification result with suggested label</returns>
        public async Task<ClassificationResult> ClassifyContentAsync(string content, Dictionary<string, object> metadata = null)
        {
            if (string.IsNullOrEmpty(content))
                return new ClassificationResult { SuggestedLabelId = "public-default", Confidence = 0.9 };

            try
            {
                // Analyze content for sensitive keywords and patterns
                var analysisResult = await _contentAnalyzer.AnalyzeAsync(content, metadata);
                
                // Determine priority based on analysis
                var suggestedPriority = DeterminePriority(analysisResult);
                
                // Get appropriate label for the priority
                var labels = await _labelRepository.GetByPriorityAsync(suggestedPriority);
                var selectedLabel = labels.FirstOrDefault(l => l.IsActive);

                if (selectedLabel == null)
                {
                    // Fall back to internal label
                    selectedLabel = await _labelRepository.GetByNameAsync("Internal");
                }

                return new ClassificationResult
                {
                    SuggestedLabelId = selectedLabel?.Id ?? "internal-default",
                    Confidence = analysisResult.ConfidenceScore,
                    DetectedPatterns = analysisResult.DetectedPatterns,
                    RecommendedProtections = GetRecommendedProtections(analysisResult)
                };
            }
            catch (Exception ex)
            {
                // Log error and return safe default
                return new ClassificationResult
                {
                    SuggestedLabelId = "internal-default",
                    Confidence = 0.5,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Determines priority based on content analysis result
        /// </summary>
        private LabelPriority DeterminePriority(ContentAnalysisResult analysisResult)
        {
            if (analysisResult.DetectedPatterns.Any(p => p.RiskLevel == "Critical"))
                return LabelPriority.Restricted;

            if (analysisResult.DetectedPatterns.Any(p => p.RiskLevel == "High"))
                return LabelPriority.HighlyConfidential;

            if (analysisResult.DetectedPatterns.Any(p => p.RiskLevel == "Medium"))
                return LabelPriority.Confidential;

            if (analysisResult.DetectedPatterns.Any(p => p.RiskLevel == "Low"))
                return LabelPriority.Internal;

            return LabelPriority.Public;
        }

        /// <summary>
        /// Gets recommended protection settings based on analysis
        /// </summary>
        private ProtectionSettings GetRecommendedProtections(ContentAnalysisResult analysisResult)
        {
            var highRiskPatterns = analysisResult.DetectedPatterns.Where(p => p.RiskLevel is "Critical" or "High").ToList();
            var mediumRiskPatterns = analysisResult.DetectedPatterns.Where(p => p.RiskLevel == "Medium").ToList();

            return new ProtectionSettings
            {
                RequireEncryption = highRiskPatterns.Any(),
                PreventExtraction = highRiskPatterns.Any(p => p.PatternType == "PersonalData" || p.PatternType == "Financial"),
                PreventCopyPaste = highRiskPatterns.Any() || mediumRiskPatterns.Count > 2,
                PreventGrounding = analysisResult.DetectedPatterns.Any(p => p.RiskLevel == "Critical"),
                AllowedUsers = new List<string>(),
                AllowedGroups = new List<string>()
            };
        }

        /// <summary>
        /// Initializes keyword to priority mapping
        /// </summary>
        private Dictionary<string, LabelPriority> InitializeKeywordPriorityMap()
        {
            return new Dictionary<string, LabelPriority>(StringComparer.OrdinalIgnoreCase)
            {
                // Critical/Restricted keywords
                { "social security", LabelPriority.Restricted },
                { "ssn", LabelPriority.Restricted },
                { "credit card", LabelPriority.Restricted },
                { "passport", LabelPriority.Restricted },
                { "classified", LabelPriority.Restricted },
                { "top secret", LabelPriority.Restricted },

                // High/Highly Confidential keywords
                { "confidential", LabelPriority.HighlyConfidential },
                { "proprietary", LabelPriority.HighlyConfidential },
                { "trade secret", LabelPriority.HighlyConfidential },
                { "financial", LabelPriority.HighlyConfidential },
                { "salary", LabelPriority.HighlyConfidential },

                // Medium/Confidential keywords
                { "internal", LabelPriority.Confidential },
                { "employee", LabelPriority.Confidential },
                { "project", LabelPriority.Confidential },
                { "strategy", LabelPriority.Confidential },

                // Low/Internal keywords
                { "meeting", LabelPriority.Internal },
                { "team", LabelPriority.Internal },
                { "company", LabelPriority.Internal }
            };
        }
    }

    /// <summary>
    /// Result of content classification
    /// </summary>
    public class ClassificationResult
    {
        public string SuggestedLabelId { get; set; }
        public double Confidence { get; set; }
        public List<DetectedPattern> DetectedPatterns { get; set; } = new List<DetectedPattern>();
        public ProtectionSettings RecommendedProtections { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Result of content analysis
    /// </summary>
    public class ContentAnalysisResult
    {
        public double ConfidenceScore { get; set; }
        public List<DetectedPattern> DetectedPatterns { get; set; } = new List<DetectedPattern>();
        public Dictionary<string, object> AnalysisMetadata { get; set; } = new Dictionary<string, object>();
    }

    /// <summary>
    /// Detected pattern in content
    /// </summary>
    public class DetectedPattern
    {
        public string PatternType { get; set; }
        public string RiskLevel { get; set; }
        public string Description { get; set; }
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public double Confidence { get; set; }
    }
}