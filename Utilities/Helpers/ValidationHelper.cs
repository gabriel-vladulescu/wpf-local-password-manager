using System;
using System.Collections.Generic;
using System.Linq;
using AccountManager.Services;

namespace AccountManager.Utilities.Helpers
{
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates multiple fields and returns the first error
        /// </summary>
        public static string ValidateFields(params (string value, string fieldName, bool required)[] fields)
        {
            foreach (var (value, fieldName, required) in fields)
            {
                if (required && string.IsNullOrWhiteSpace(value))
                {
                    return $"{fieldName} is required";
                }
            }
            return null;
        }

        /// <summary>
        /// Validates a list of validation results and returns combined errors
        /// </summary>
        public static string CombineValidationErrors(params ValidationResult[] results)
        {
            var errors = results?
                .Where(r => r != null && !r.IsValid)
                .SelectMany(r => r.Errors)
                .ToList();

            return errors?.Any() == true ? string.Join(Environment.NewLine, errors) : null;
        }

        /// <summary>
        /// Creates a validation result from a boolean condition
        /// </summary>
        public static ValidationResult CreateValidationResult(bool isValid, string errorMessage = null)
        {
            var result = new ValidationResult();
            if (!isValid && !string.IsNullOrEmpty(errorMessage))
            {
                result.AddError(errorMessage);
            }
            return result;
        }

        /// <summary>
        /// Validates that a string meets length requirements
        /// </summary>
        public static ValidationResult ValidateLength(string value, string fieldName, int minLength = 0, int maxLength = int.MaxValue)
        {
            var result = new ValidationResult();
            
            if (string.IsNullOrEmpty(value))
            {
                if (minLength > 0)
                {
                    result.AddError($"{fieldName} is required");
                }
                return result;
            }

            if (value.Length < minLength)
            {
                result.AddError($"{fieldName} must be at least {minLength} characters long");
            }
            else if (value.Length > maxLength)
            {
                result.AddError($"{fieldName} cannot exceed {maxLength} characters");
            }

            return result;
        }

        /// <summary>
        /// Validates that a value is within a numeric range
        /// </summary>
        public static ValidationResult ValidateRange<T>(T value, string fieldName, T minValue, T maxValue) where T : IComparable<T>
        {
            var result = new ValidationResult();
            
            if (value.CompareTo(minValue) < 0)
            {
                result.AddError($"{fieldName} must be at least {minValue}");
            }
            else if (value.CompareTo(maxValue) > 0)
            {
                result.AddError($"{fieldName} cannot exceed {maxValue}");
            }

            return result;
        }
    }
}