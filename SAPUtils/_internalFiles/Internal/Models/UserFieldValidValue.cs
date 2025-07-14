using System;
using System.Collections.Generic;

namespace SAPUtils.Models.UserTables {

    internal sealed class UserFieldValidValue : IUserFieldValidValue {
        public UserFieldValidValue(string value, string description) {
            Value = value;
            Description = description;
        }

        /// <summary>
        /// Gets or sets the internal value of the user field.
        /// This is typically the value stored in the database or used for logic comparisons.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Gets or sets the human-readable description associated with the value.
        /// This is usually displayed in the UI to represent the meaning of the <see cref="Value"/>.
        /// </summary>
        public string Description { get; set; }
        
        internal static List<IUserFieldValidValue> ParseValidValuePairs(string[] pairs)
        {
            List<IUserFieldValidValue> validValues = new List<IUserFieldValidValue>();

            if (pairs == null || pairs.Length == 0)
                return null;

            if (pairs.Length % 2 != 0)
                throw new ArgumentException("Valid values must be provided in (value, description) pairs.");

            for (int i = 0; i < pairs.Length; i += 2) {
                validValues.Add(new UserFieldValidValue(
                    value: pairs[i],
                    description: pairs[i + 1]
                ));
            }
            return validValues;
        }
    }
}