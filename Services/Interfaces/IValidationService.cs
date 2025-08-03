using System;
using System.Collections.Generic;
using AccountManager.Models;

namespace AccountManager.Services.Interfaces
{
    public interface IValidationService
    {
        ValidationResult ValidateAccount(Account account);
        ValidationResult ValidateAccountName(string name);
        ValidationResult ValidateUsername(string username);
        ValidationResult ValidatePassword(string password, bool requireStrong = false);
        ValidationResult ValidateEmail(string email);
        ValidationResult ValidateWebsite(string website);
        ValidationResult ValidateGroup(AccountGroup group);
        ValidationResult ValidateGroupName(string name);
        ValidationResult ValidateGroupNameUniqueness(string name, IEnumerable<AccountGroup> existingGroups, AccountGroup excludeGroup = null);
        ValidationResult ValidateRequiredString(string value, string fieldName, int minLength = 1, int maxLength = int.MaxValue);
        ValidationResult ValidateOptionalString(string value, string fieldName, int maxLength = int.MaxValue);
        ValidationResult ValidateMultiple(params ValidationResult[] validations);
        ValidationResult ValidateWithCustomRules<T>(T item, params Func<T, ValidationResult>[] customRules);
        int GetPasswordStrength(string password);
        string GetPasswordStrengthDescription(int strength);
    }
}