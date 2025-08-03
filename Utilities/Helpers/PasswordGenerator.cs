using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AccountManager.Utilities.Helpers
{
    /// <summary>
    /// Password generation options
    /// </summary>
    public class PasswordGenerationOptions
    {
        public int Length { get; set; } = 16;
        public bool IncludeUppercase { get; set; } = true;
        public bool IncludeLowercase { get; set; } = true;
        public bool IncludeNumbers { get; set; } = true;
        public bool IncludeSpecialCharacters { get; set; } = true;
        public bool ExcludeSimilarCharacters { get; set; } = true;
        public bool ExcludeAmbiguousCharacters { get; set; } = true;
        public string CustomCharacterSet { get; set; } = "";
        public bool RequireAtLeastOneFromEachCategory { get; set; } = true;

        /// <summary>
        /// Predefined options for different security levels
        /// </summary>
        public static class Presets
        {
            public static PasswordGenerationOptions Simple => new()
            {
                Length = 8,
                IncludeUppercase = true,
                IncludeLowercase = true,
                IncludeNumbers = true,
                IncludeSpecialCharacters = false,
                ExcludeSimilarCharacters = true,
                ExcludeAmbiguousCharacters = true
            };

            public static PasswordGenerationOptions Standard => new()
            {
                Length = 12,
                IncludeUppercase = true,
                IncludeLowercase = true,
                IncludeNumbers = true,
                IncludeSpecialCharacters = true,
                ExcludeSimilarCharacters = true,
                ExcludeAmbiguousCharacters = true
            };

            public static PasswordGenerationOptions Strong => new()
            {
                Length = 16,
                IncludeUppercase = true,
                IncludeLowercase = true,
                IncludeNumbers = true,
                IncludeSpecialCharacters = true,
                ExcludeSimilarCharacters = true,
                ExcludeAmbiguousCharacters = false
            };

            public static PasswordGenerationOptions Maximum => new()
            {
                Length = 32,
                IncludeUppercase = true,
                IncludeLowercase = true,
                IncludeNumbers = true,
                IncludeSpecialCharacters = true,
                ExcludeSimilarCharacters = false,
                ExcludeAmbiguousCharacters = false
            };

            public static PasswordGenerationOptions NumericPin => new()
            {
                Length = 6,
                IncludeUppercase = false,
                IncludeLowercase = false,
                IncludeNumbers = true,
                IncludeSpecialCharacters = false,
                ExcludeSimilarCharacters = false,
                ExcludeAmbiguousCharacters = false
            };
        }
    }

    /// <summary>
    /// Secure password generator using cryptographically strong random number generation
    /// </summary>
    public static class PasswordGenerator
    {
        #region Character Sets

        private const string UppercaseLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string LowercaseLetters = "abcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "0123456789";
        private const string SpecialCharacters = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        
        // Characters that look similar and might cause confusion
        private const string SimilarCharacters = "il1Lo0O";
        
        // Characters that might be ambiguous in certain fonts or contexts
        private const string AmbiguousCharacters = "{}[]()/\\'\"`~,;.<>";

        #endregion

        #region Public Methods

        /// <summary>
        /// Generate a password using default options
        /// </summary>
        /// <param name="length">Password length (default: 16)</param>
        /// <returns>Generated password</returns>
        public static string Generate(int length = 16)
        {
            return Generate(new PasswordGenerationOptions { Length = length });
        }

        /// <summary>
        /// Generate a password using specified options
        /// </summary>
        /// <param name="options">Password generation options</param>
        /// <returns>Generated password</returns>
        public static string Generate(PasswordGenerationOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Length < 1)
                throw new ArgumentException("Password length must be at least 1", nameof(options));

            if (options.Length > 1000)
                throw new ArgumentException("Password length cannot exceed 1000 characters", nameof(options));

            var characterSet = BuildCharacterSet(options);
            
            if (string.IsNullOrEmpty(characterSet))
                throw new InvalidOperationException("No valid characters available for password generation");

            return GeneratePasswordFromCharacterSet(characterSet, options);
        }

        /// <summary>
        /// Generate multiple passwords
        /// </summary>
        /// <param name="count">Number of passwords to generate</param>
        /// <param name="options">Password generation options</param>
        /// <returns>Array of generated passwords</returns>
        public static string[] GenerateMultiple(int count, PasswordGenerationOptions options = null)
        {
            if (count < 1)
                throw new ArgumentException("Count must be at least 1", nameof(count));

            if (count > 100)
                throw new ArgumentException("Cannot generate more than 100 passwords at once", nameof(count));

            options ??= new PasswordGenerationOptions();
            var passwords = new string[count];

            for (int i = 0; i < count; i++)
            {
                passwords[i] = Generate(options);
            }

            return passwords;
        }

        /// <summary>
        /// Generate a pronounceable password (less secure but easier to remember)
        /// </summary>
        /// <param name="syllableCount">Number of syllables (default: 4)</param>
        /// <param name="includeNumbers">Include numbers between syllables</param>
        /// <param name="includeSpecialChars">Include special characters</param>
        /// <returns>Generated pronounceable password</returns>
        public static string GeneratePronounceablePassword(int syllableCount = 4, bool includeNumbers = true, bool includeSpecialChars = false)
        {
            if (syllableCount < 1)
                throw new ArgumentException("Syllable count must be at least 1", nameof(syllableCount));

            var consonants = "bcdfghjklmnpqrstvwxyz";
            var vowels = "aeiou";
            var numbers = "0123456789";
            var specials = "!@#$%&*";

            var password = new StringBuilder();

            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = 0; i < syllableCount; i++)
                {
                    // Add consonant-vowel-consonant pattern
                    password.Append(GetRandomCharacter(consonants, rng));
                    password.Append(GetRandomCharacter(vowels, rng));
                    
                    if (i < syllableCount - 1) // Don't add consonant at the end
                    {
                        password.Append(GetRandomCharacter(consonants, rng));
                        
                        // Occasionally add numbers or special characters between syllables
                        if (includeNumbers && GetRandomNumber(rng, 0, 3) == 0)
                        {
                            password.Append(GetRandomCharacter(numbers, rng));
                        }
                        
                        if (includeSpecialChars && GetRandomNumber(rng, 0, 4) == 0)
                        {
                            password.Append(GetRandomCharacter(specials, rng));
                        }
                    }
                }

                // Capitalize first letter
                if (password.Length > 0)
                {
                    password[0] = char.ToUpper(password[0]);
                }
            }

            return password.ToString();
        }

        /// <summary>
        /// Generate a passphrase using random words
        /// </summary>
        /// <param name="wordCount">Number of words (default: 4)</param>
        /// <param name="separator">Word separator (default: "-")</param>
        /// <param name="includeNumbers">Include numbers</param>
        /// <returns>Generated passphrase</returns>
        public static string GeneratePassphrase(int wordCount = 4, string separator = "-", bool includeNumbers = true)
        {
            if (wordCount < 2)
                throw new ArgumentException("Word count must be at least 2", nameof(wordCount));

            // Simple word list for demonstration - in a real application, you'd use a larger dictionary
            var words = new[]
            {
                "apple", "brave", "chair", "dance", "eagle", "flame", "grace", "heart", "image", "jolly",
                "key", "light", "magic", "night", "ocean", "peace", "quick", "river", "storm", "trust",
                "unity", "voice", "water", "youth", "zebra", "amber", "bread", "cloud", "dream", "earth",
                "frost", "green", "happy", "ivory", "jazz", "kite", "lemon", "moon", "novel", "olive",
                "piano", "quest", "robin", "sunny", "tiger", "urban", "vivid", "windy", "xerus", "yield"
            };

            var selectedWords = new string[wordCount];
            
            using (var rng = RandomNumberGenerator.Create())
            {
                for (int i = 0; i < wordCount; i++)
                {
                    var wordIndex = GetRandomNumber(rng, 0, words.Length);
                    selectedWords[i] = words[wordIndex];
                }

                // Optionally add numbers
                if (includeNumbers)
                {
                    var numberPosition = GetRandomNumber(rng, 0, wordCount);
                    var randomNumber = GetRandomNumber(rng, 10, 999);
                    selectedWords[numberPosition] += randomNumber.ToString();
                }
            }

            return string.Join(separator, selectedWords);
        }

        /// <summary>
        /// Estimate password strength (0-4 scale)
        /// </summary>
        /// <param name="password">Password to analyze</param>
        /// <returns>Strength score (0 = very weak, 4 = very strong)</returns>
        public static int EstimateStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;

            int score = 0;

            // Length scoring
            if (password.Length >= 8) score++;
            if (password.Length >= 12) score++;

            // Character variety scoring
            if (password.Any(char.IsLower)) score++;
            if (password.Any(char.IsUpper)) score++;
            if (password.Any(char.IsDigit)) score++;
            if (password.Any(c => SpecialCharacters.Contains(c))) score++;

            // Pattern detection (reduce score for common patterns)
            if (HasCommonPatterns(password))
                score = Math.Max(0, score - 1);

            return Math.Min(4, score);
        }

        #endregion

        #region Private Methods

        private static string BuildCharacterSet(PasswordGenerationOptions options)
        {
            var characterSet = new StringBuilder();

            if (!string.IsNullOrEmpty(options.CustomCharacterSet))
            {
                characterSet.Append(options.CustomCharacterSet);
            }
            else
            {
                if (options.IncludeUppercase)
                    characterSet.Append(UppercaseLetters);

                if (options.IncludeLowercase)
                    characterSet.Append(LowercaseLetters);

                if (options.IncludeNumbers)
                    characterSet.Append(Numbers);

                if (options.IncludeSpecialCharacters)
                    characterSet.Append(SpecialCharacters);
            }

            var result = characterSet.ToString();

            // Remove similar characters if requested
            if (options.ExcludeSimilarCharacters)
            {
                result = RemoveCharacters(result, SimilarCharacters);
            }

            // Remove ambiguous characters if requested
            if (options.ExcludeAmbiguousCharacters)
            {
                result = RemoveCharacters(result, AmbiguousCharacters);
            }

            return result;
        }

        private static string GeneratePasswordFromCharacterSet(string characterSet, PasswordGenerationOptions options)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var password = new StringBuilder(options.Length);

                // If we need at least one from each category, ensure that first
                if (options.RequireAtLeastOneFromEachCategory && string.IsNullOrEmpty(options.CustomCharacterSet))
                {
                    EnsureCharacterVariety(password, options, rng);
                }

                // Fill the rest of the password
                while (password.Length < options.Length)
                {
                    var randomChar = GetRandomCharacter(characterSet, rng);
                    password.Append(randomChar);
                }

                // Shuffle the password to avoid predictable patterns
                return ShuffleString(password.ToString(), rng);
            }
        }

        private static void EnsureCharacterVariety(StringBuilder password, PasswordGenerationOptions options, RandomNumberGenerator rng)
        {
            if (options.IncludeUppercase && password.Length < options.Length)
            {
                var chars = options.ExcludeSimilarCharacters ? RemoveCharacters(UppercaseLetters, SimilarCharacters) : UppercaseLetters;
                if (options.ExcludeAmbiguousCharacters) chars = RemoveCharacters(chars, AmbiguousCharacters);
                if (!string.IsNullOrEmpty(chars))
                    password.Append(GetRandomCharacter(chars, rng));
            }

            if (options.IncludeLowercase && password.Length < options.Length)
            {
                var chars = options.ExcludeSimilarCharacters ? RemoveCharacters(LowercaseLetters, SimilarCharacters) : LowercaseLetters;
                if (options.ExcludeAmbiguousCharacters) chars = RemoveCharacters(chars, AmbiguousCharacters);
                if (!string.IsNullOrEmpty(chars))
                    password.Append(GetRandomCharacter(chars, rng));
            }

            if (options.IncludeNumbers && password.Length < options.Length)
            {
                var chars = options.ExcludeSimilarCharacters ? RemoveCharacters(Numbers, SimilarCharacters) : Numbers;
                if (!string.IsNullOrEmpty(chars))
                    password.Append(GetRandomCharacter(chars, rng));
            }

            if (options.IncludeSpecialCharacters && password.Length < options.Length)
            {
                var chars = options.ExcludeAmbiguousCharacters ? RemoveCharacters(SpecialCharacters, AmbiguousCharacters) : SpecialCharacters;
                if (!string.IsNullOrEmpty(chars))
                    password.Append(GetRandomCharacter(chars, rng));
            }
        }

        private static string RemoveCharacters(string source, string charactersToRemove)
        {
            return new string(source.Where(c => !charactersToRemove.Contains(c)).ToArray());
        }

        private static char GetRandomCharacter(string characters, RandomNumberGenerator rng)
        {
            if (string.IsNullOrEmpty(characters))
                throw new ArgumentException("Character set cannot be empty", nameof(characters));

            var index = GetRandomNumber(rng, 0, characters.Length);
            return characters[index];
        }

        private static int GetRandomNumber(RandomNumberGenerator rng, int minValue, int maxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentException("minValue must be less than maxValue");

            var range = (uint)(maxValue - minValue);
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var randomValue = BitConverter.ToUInt32(bytes, 0);
            return (int)(randomValue % range) + minValue;
        }

        private static string ShuffleString(string input, RandomNumberGenerator rng)
        {
            var array = input.ToCharArray();
            
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = GetRandomNumber(rng, 0, i + 1);
                (array[i], array[j]) = (array[j], array[i]);
            }
            
            return new string(array);
        }

        private static bool HasCommonPatterns(string password)
        {
            // Check for common patterns that weaken passwords
            var lowerPassword = password.ToLower();
            
            // Sequential characters
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] + 1 == password[i + 1] && password[i + 1] + 1 == password[i + 2])
                    return true;
            }

            // Repeated characters
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] == password[i + 1] && password[i + 1] == password[i + 2])
                    return true;
            }

            // Common patterns
            var commonPatterns = new[] { "123", "abc", "qwe", "asd", "zxc", "password", "admin" };
            return commonPatterns.Any(pattern => lowerPassword.Contains(pattern));
        }

        #endregion
    }
}