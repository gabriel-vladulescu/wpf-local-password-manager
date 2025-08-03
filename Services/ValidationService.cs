using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using AccountManager.Models;

namespace AccountManager.Services
{
    /// <summary>
    /// Validation result containing error messages and validation status
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public string FirstError => Errors.FirstOrDefault();

        public ValidationResult()
        {
            IsValid = true;
        }

        public ValidationResult(string error)
        {
            IsValid = false;
            Errors.Add(error);
        }

        public ValidationResult(IEnumerable<string> errors)
        {
            var errorList = errors?.ToList() ?? new List<string>();
            IsValid = !errorList.Any();
            Errors = errorList;
        }

        public void AddError(string error)
        {
            if (!string.IsNullOrEmpty(error))
            {
                Errors.Add(error);
                IsValid = false;
            }
        }

        public void AddErrors(IEnumerable<string> errors)
        {
            if (errors != null)
            {
                foreach (var error in errors)
                {
                    AddError(error);
                }
            }
        }
    }

    /// <summary>
    /// Centralized validation service for all application data
    /// </summary>
    public class ValidationService
    {
        private static readonly Lazy<ValidationService> _instance = new(() => new ValidationService());
        public static ValidationService Instance => _instance.Value;

        private readonly Regex _emailRegex;
        private readonly Regex _urlRegex;
        private readonly Regex _strongPasswordRegex;

        private ValidationService()
        {
            // Email validation regex (RFC 5322 compliant)
            _emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // URL validation regex
            _urlRegex = new Regex(@"^(https?://)?([\da-z\.-]+)\.([a-z\.]{2,6})([/\w \.-]*)*/?$", 
                RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Strong password regex (at least 8 chars, 1 upper, 1 lower, 1 number, 1 special)
            _strongPasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$", 
                RegexOptions.Compiled);
        }

        #region Account Validation

        /// <summary>
        /// Validate an account object
        /// </summary>
        public ValidationResult ValidateAccount(Account account)
        {
            var result = new ValidationResult();

            if (account == null)
            {
                result.AddError("Account is required");
                return result;
            }

            // Validate account name
            var nameValidation = ValidateAccountName(account.Name);
            result.AddErrors(nameValidation.Errors);

            // Validate email if provided
            if (!string.IsNullOrWhiteSpace(account.Email))
            {
                var emailValidation = ValidateEmail(account.Email);
                result.AddErrors(emailValidation.Errors);
            }

            // Validate website if provided
            if (!string.IsNullOrWhiteSpace(account.Website))
            {
                var websiteValidation = ValidateWebsite(account.Website);
                result.AddErrors(websiteValidation.Errors);
            }

            // Validate username if provided
            if (!string.IsNullOrWhiteSpace(account.Username))
            {
                var usernameValidation = ValidateUsername(account.Username);
                result.AddErrors(usernameValidation.Errors);
            }

            // Validate password if provided
            if (!string.IsNullOrWhiteSpace(account.Password))
            {
                var passwordValidation = ValidatePassword(account.Password);
                result.AddErrors(passwordValidation.Errors);
            }

            return result;
        }

        /// <summary>
        /// Validate account name
        /// </summary>
        public ValidationResult ValidateAccountName(string name)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddError("Account name is required");
                return result;
            }

            var trimmedName = name.Trim();
            
            if (trimmedName.Length < 2)
            {
                result.AddError("Account name must be at least 2 characters long");
            }
            else if (trimmedName.Length > 100)
            {
                result.AddError("Account name cannot exceed 100 characters");
            }

            // Check for invalid characters
            if (ContainsInvalidCharacters(trimmedName))
            {
                result.AddError("Account name contains invalid characters");
            }

            return result;
        }

        /// <summary>
        /// Validate username
        /// </summary>
        public ValidationResult ValidateUsername(string username)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(username))
            {
                return result; // Username is optional
            }

            var trimmedUsername = username.Trim();
            
            if (trimmedUsername.Length > 50)
            {
                result.AddError("Username cannot exceed 50 characters");
            }

            return result;
        }

        /// <summary>
        /// Validate password with configurable strength requirements
        /// </summary>
        public ValidationResult ValidatePassword(string password, bool requireStrong = false)
        {
            var result = new ValidationResult();

            if (string.IsNullOrEmpty(password))
            {
                return result; // Password is optional for validation
            }

            if (password.Length < 4)
            {
                result.AddError("Password must be at least 4 characters long");
            }
            else if (password.Length > 128)
            {
                result.AddError("Password cannot exceed 128 characters");
            }

            if (requireStrong && !_strongPasswordRegex.IsMatch(password))
            {
                result.AddError("Password must contain at least 8 characters, including uppercase, lowercase, number, and special character");
            }

            return result;
        }

        /// <summary>
        /// Validate email address
        /// </summary>
        public ValidationResult ValidateEmail(string email)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(email))
            {
                return result; // Email is optional
            }

            var trimmedEmail = email.Trim();

            if (trimmedEmail.Length > 254)
            {
                result.AddError("Email address is too long");
                return result;
            }

            // Use both regex and MailAddress for validation
            if (!_emailRegex.IsMatch(trimmedEmail))
            {
                result.AddError("Please enter a valid email address");
                return result;
            }

            try
            {
                var mailAddress = new MailAddress(trimmedEmail);
                if (mailAddress.Address != trimmedEmail)
                {
                    result.AddError("Please enter a valid email address");
                }
            }
            catch
            {
                result.AddError("Please enter a valid email address");
            }

            return result;
        }

        /// <summary>
        /// Validate website URL
        /// </summary>
        public ValidationResult ValidateWebsite(string website)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(website))
            {
                return result; // Website is optional
            }

            var trimmedWebsite = website.Trim();

            if (trimmedWebsite.Length > 2048)
            {
                result.AddError("Website URL is too long");
                return result;
            }

            // Add protocol if missing
            var urlToValidate = trimmedWebsite;
            if (!urlToValidate.StartsWith("http://") && !urlToValidate.StartsWith("https://"))
            {
                urlToValidate = "https://" + urlToValidate;
            }

            if (!Uri.TryCreate(urlToValidate, UriKind.Absolute, out var uriResult) ||
                (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps))
            {
                result.AddError("Please enter a valid website URL");
            }

            return result;
        }

        #endregion

        #region Group Validation

        /// <summary>
        /// Validate an account group
        /// </summary>
        public ValidationResult ValidateGroup(AccountGroup group)
        {
            var result = new ValidationResult();

            if (group == null)
            {
                result.AddError("Group is required");
                return result;
            }

            var nameValidation = ValidateGroupName(group.Name);
            result.AddErrors(nameValidation.Errors);

            return result;
        }

        /// <summary>
        /// Validate group name
        /// </summary>
        public ValidationResult ValidateGroupName(string name)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.AddError("Group name is required");
                return result;
            }

            var trimmedName = name.Trim();
            
            if (trimmedName.Length < 2)
            {
                result.AddError("Group name must be at least 2 characters long");
            }
            else if (trimmedName.Length > 50)
            {
                result.AddError("Group name cannot exceed 50 characters");
            }

            // Check for invalid characters
            if (ContainsInvalidCharacters(trimmedName))
            {
                result.AddError("Group name contains invalid characters");
            }

            return result;
        }

        /// <summary>
        /// Validate group name uniqueness
        /// </summary>
        public ValidationResult ValidateGroupNameUniqueness(string name, IEnumerable<AccountGroup> existingGroups, AccountGroup excludeGroup = null)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(name) || existingGroups == null)
            {
                return result;
            }

            var trimmedName = name.Trim();
            var isDuplicate = existingGroups
                .Where(g => g != excludeGroup)
                .Any(g => string.Equals(g.Name?.Trim(), trimmedName, StringComparison.OrdinalIgnoreCase));

            if (isDuplicate)
            {
                result.AddError("A group with this name already exists");
            }

            return result;
        }

        #endregion

        #region General Validation Helpers

        /// <summary>
        /// Check if a string contains invalid characters for names
        /// </summary>
        private bool ContainsInvalidCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            // Define invalid characters for names (file system unfriendly characters)
            var invalidChars = new char[] { '<', '>', ':', '"', '|', '?', '*', '\\', '/' };
            return input.IndexOfAny(invalidChars) >= 0;
        }

        /// <summary>
        /// Validate a required string field
        /// </summary>
        public ValidationResult ValidateRequiredString(string value, string fieldName, int minLength = 1, int maxLength = int.MaxValue)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(value))
            {
                result.AddError($"{fieldName} is required");
                return result;
            }

            var trimmedValue = value.Trim();
            
            if (trimmedValue.Length < minLength)
            {
                result.AddError($"{fieldName} must be at least {minLength} character(s) long");
            }
            else if (trimmedValue.Length > maxLength)
            {
                result.AddError($"{fieldName} cannot exceed {maxLength} characters");
            }

            return result;
        }

        /// <summary>
        /// Validate an optional string field
        /// </summary>
        public ValidationResult ValidateOptionalString(string value, string fieldName, int maxLength = int.MaxValue)
        {
            var result = new ValidationResult();

            if (string.IsNullOrWhiteSpace(value))
            {
                return result; // Optional field, no validation needed
            }

            var trimmedValue = value.Trim();
            
            if (trimmedValue.Length > maxLength)
            {
                result.AddError($"{fieldName} cannot exceed {maxLength} characters");
            }

            return result;
        }

        /// <summary>
        /// Check password strength and return score (0-4)
        /// </summary>
        public int GetPasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;

            int score = 0;

            // Length check
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;

            // Character type checks
            if (password.Any(char.IsLower)) score++;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c))) score++;

            // Cap at 4
            return Math.Min(score, 4);
        }

        /// <summary>
        /// Get password strength description
        /// </summary>
        public string GetPasswordStrengthDescription(int strength)
        {
            return strength switch
            {
                0 => "Very Weak",
                1 => "Weak",
                2 => "Fair",
                3 => "Good",
                4 => "Strong",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Validate multiple fields and combine results
        /// </summary>
        public ValidationResult ValidateMultiple(params ValidationResult[] validations)
        {
            var result = new ValidationResult();

            foreach (var validation in validations)
            {
                if (validation != null && !validation.IsValid)
                {
                    result.AddErrors(validation.Errors);
                }
            }

            return result;
        }

        #endregion

        #region Custom Validation Rules

        /// <summary>
        /// Register and validate against custom rules
        /// </summary>
        public ValidationResult ValidateWithCustomRules<T>(T item, params Func<T, ValidationResult>[] customRules)
        {
            var result = new ValidationResult();

            foreach (var rule in customRules)
            {
                var ruleResult = rule(item);
                if (!ruleResult.IsValid)
                {
                    result.AddErrors(ruleResult.Errors);
                }
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// Extension methods for validation
    /// </summary>
    public static class ValidationExtensions
    {
        /// <summary>
        /// Convert ValidationResult to a single error message
        /// </summary>
        public static string ToErrorMessage(this ValidationResult result)
        {
            if (result == null || result.IsValid)
                return string.Empty;

            return string.Join(Environment.NewLine, result.Errors);
        }

        /// <summary>
        /// Convert ValidationResult to a formatted error message
        /// </summary>
        public static string ToFormattedErrorMessage(this ValidationResult result, string prefix = "• ")
        {
            if (result == null || result.IsValid)
                return string.Empty;

            return string.Join(Environment.NewLine, result.Errors.Select(e => prefix + e));
        }
    }
}