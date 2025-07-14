using System;
using System.Collections.Generic;
using LLMSensitiveDataGoverance.Core.Models;

namespace LLMSensitiveDataGoverance.Core.Utilities
{
    /// <summary>
    /// Comparer for sensitivity label priorities
    /// </summary>
    public class LabelPriorityComparer : IComparer<LabelPriority>
    {
        /// <summary>
        /// Compares two label priorities
        /// </summary>
        /// <param name="x">First priority</param>
        /// <param name="y">Second priority</param>
        /// <returns>Comparison result</returns>
        public int Compare(LabelPriority x, LabelPriority y)
        {
            return ((int)x).CompareTo((int)y);
        }

        /// <summary>
        /// Determines if the first priority is higher than the second
        /// </summary>
        /// <param name="priority1">First priority</param>
        /// <param name="priority2">Second priority</param>
        /// <returns>True if first priority is higher</returns>
        public bool IsHigherPriority(LabelPriority priority1, LabelPriority priority2)
        {
            return Compare(priority1, priority2) > 0;
        }

        /// <summary>
        /// Gets the maximum priority from a collection
        /// </summary>
        /// <param name="priorities">Collection of priorities</param>
        /// <returns>Maximum priority</returns>
        public LabelPriority GetMaxPriority(IEnumerable<LabelPriority> priorities)
        {
            var maxPriority = LabelPriority.Public;
            
            foreach (var priority in priorities)
            {
                if (IsHigherPriority(priority, maxPriority))
                    maxPriority = priority;
            }

            return maxPriority;
        }
    }
}