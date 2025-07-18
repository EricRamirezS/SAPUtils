using System;
using System.Globalization;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.Utils;

namespace SAPUtils.Attributes.UserTables {
    /// <summary>
    /// Represents an attribute used to define a user-defined field (UDF) in SAP Business One that specifically stores double-precision floating-point (`double?`) values.
    /// <br/>
    /// Fields attributed with this class are stored as `db_Float`, enabling robust handling of numeric values with decimal accuracy.
    /// <br/>
    /// <b>Key Features:</b>
    /// - Provides type safety for storing double-precision numeric values.
    /// - Includes default parsing logic for converting various numeric and string inputs into a `double?`.
    /// - Overrides serialization to ensure values are stored in SAP-compatible formats.
    /// </summary>
    /// <remarks>
    /// This attribute, as part of the user-defined field (UDF) infrastructure for SAP Business One, ensures that system precision configuration is adhered to when values are stored and retrieved from tables.
    /// It is primarily designed to interact with underlying SAP Business One database structures.
    /// </remarks>
    /// <remarks>
    /// Implements <see cref="IUserTableField{Double}"/> for type-safe handling of `double?` data and offers a pre-configured default value handling mechanism.
    /// </remarks>
    /// <seealso cref="QuantityFieldAttribute"/>
    public abstract class DoubleUserTableFieldAttribute : UserTableFieldAttributeBase, IUserTableField<double?> {
        /// <summary>
        /// Represents the strongly-typed default value of the field for implementations of
        /// <see cref="SAPUtils.Attributes.UserTables.DoubleUserTableFieldAttribute" />.
        /// </summary>
        /// <remarks>
        /// This value is used internally to store the default value as a nullable double (`double?`)
        /// for the field associated with a user table.
        /// </remarks>
        /// <seealso cref="SAPUtils.Attributes.UserTables.DoubleUserTableFieldAttribute"/>
        /// <seealso cref="SAPUtils.Attributes.UserTables.UserTableFieldAttributeBase"/>
        /// <seealso cref="SAPUtils.__Internal.Attributes.UserTables.IUserTableField{T}"/>
        private double? _stronglyTypedDefaultValue;


        /// <inheritdoc />
        public override int Size { get; set; } = 11;

        /// <inheritdoc />
        public override object DefaultValue
        {
            get => _stronglyTypedDefaultValue;
            set => _stronglyTypedDefaultValue = (double?)ParseValue(value);
        }

        /// <inheritdoc />
        public override object ParseValue(object value) {
            return Parsers.ParseDouble(value);
        }

        /// <inheritdoc />
        public override string ToSapData(object value) {
            return value == null ? "0" : ((double)value).ToString(CultureInfo.InvariantCulture);
        }

        /// <inheritdoc />
        public override Type Type => typeof(double?);

        /// <inheritdoc />
        double? IUserTableField<double?>.DefaultValue => _stronglyTypedDefaultValue;

        /// <inheritdoc />
        double? IUserTableField<double?>.ParseValue(object value) => (double?)ParseValue(value);
    }
}