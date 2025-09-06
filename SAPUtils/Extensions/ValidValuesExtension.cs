using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using SAPbouiCOM;
using SAPUtils.Database;
using SAPUtils.I18N;
using SAPUtils.Models.UserTables;
using static SAPUtils.Utils.SapClass;

namespace SAPUtils.Extensions {
    /// <summary>
    /// Provides extension methods for managing the <see cref="SAPbouiCOM.ValidValues"/> collection.
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class ValidValuesExtension {
        /// <summary>
        /// Adds a range of valid values to the specified <see cref="SAPbouiCOM.ValidValues"/> collection.
        /// </summary>
        /// <param name="vv">
        /// The <see cref="SAPbouiCOM.ValidValues"/> to which the values will be added.
        /// </param>
        /// <param name="data">
        /// A collection of <see cref="SAPUtils.Models.UserTables.IUserFieldValidValue"/> representing the values and descriptions to be added.
        /// </param>
        /// <param name="clear">
        /// A boolean indicating whether to clear the existing values in the <see cref="SAPbouiCOM.ValidValues"/> before adding the new ones.
        /// </param>
        /// <param name="addEmpty">
        /// A boolean indicating whether to add an empty value to the collection as the first entry.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="vv"/> or <paramref name="data"/> is null.
        /// </exception>
        /// <see cref="SAPbouiCOM.ValidValues"/>
        /// <see cref="SAPUtils.Models.UserTables.IUserFieldValidValue"/>
        public static void AddRange(this ValidValues vv, IEnumerable<IValidValue> data, bool clear = false, bool addEmpty = false) {
            if (vv == null) throw new ArgumentNullException(nameof(vv));
            if (data == null) throw new ArgumentNullException(nameof(data));

            if (clear) vv.Clear();

            if (addEmpty) vv.Add("", "");

            foreach (IValidValue line in data)
                try {
                    vv.Add(line.Value, line.Description);
                }
                catch (Exception ex) {
                    log.Error(string.Format(Texts.ValidValuesExtension_AddRange_Could_not_add__0___1__to_ValidValues, line.Value, line.Description), ex);
                }
        }

        /// <summary>
        /// Clears all values from the specified <see cref="SAPbouiCOM.ValidValues"/> collection.
        /// </summary>
        /// <param name="vv">
        /// The <see cref="SAPbouiCOM.ValidValues"/> collection to be cleared.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="vv"/> parameter is null.
        /// </exception>
        /// <see cref="SAPbouiCOM.ValidValues"/>
        public static void Clear(this ValidValues vv) {
            if (vv == null) throw new ArgumentNullException(nameof(vv));

            for (int i = vv.Count - 1; i >= 0; i--) {
                vv.Remove(i, BoSearchKey.psk_Index);
            }
        }

        /// <summary>
        /// Loads valid values into the specified <see cref="SAPbouiCOM.ValidValues"/> collection from the specified user-defined table in SAP Business One.
        /// </summary>
        /// <param name="vv">
        /// The <see cref="SAPbouiCOM.ValidValues"/> collection to populate with valid values.
        /// </param>
        /// <param name="userTableName">
        /// The name of the user-defined table from which valid values should be loaded.
        /// </param>
        /// <param name="addEmpty">
        /// A boolean indicating whether to add an empty value to the collection as the first entry. The default is false.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="vv"/> parameter is null.
        /// </exception>
        /// <see cref="SAPbouiCOM.ValidValues"/>
        /// <see cref="SAPUtils.Models.UserTables.IUserFieldValidValue"/>
        public static void LoadFromUserTable(this ValidValues vv, string userTableName, bool addEmpty = false) {
            if (vv == null) throw new ArgumentNullException(nameof(vv));
            using (IRepository repository = Repository.Get()) {
                IList<IUserFieldValidValue> data = repository.GetValidValuesFromUserTable(userTableName);
                vv.AddRange(data, clear: true, addEmpty: addEmpty);
            }
        }
    }
}