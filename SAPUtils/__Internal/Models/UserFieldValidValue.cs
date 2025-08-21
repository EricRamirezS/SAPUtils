using System;
using System.Collections.Generic;
using SAPbouiCOM;
using SAPUtils.Models.UserTables;
using ValidValue = SAPUtils.Models.UserTables.ValidValue;

namespace SAPUtils.__Internal.Models {

    internal sealed class UserFieldValidValue : ValidValue, IUserFieldValidValue {
        public UserFieldValidValue(string value, string description) : base(value, description) { }
        internal static List<IValidValue> ParseValidValuePairs(string[] pairs) {
            List<IValidValue> validValues = new List<IValidValue>();

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