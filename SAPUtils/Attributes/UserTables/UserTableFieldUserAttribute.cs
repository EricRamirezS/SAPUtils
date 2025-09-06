using System;
using System.Collections.Generic;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Models;
using SAPUtils.Exceptions;
using SAPUtils.I18N;
using IUserTable = SAPUtils.__Internal.Attributes.UserTables.IUserTable;
using IValidValue = SAPbouiCOM.IValidValue;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global

// ReSharper disable UnusedType.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace SAPUtils.Attributes.UserTables {
    /// <inheritdoc cref="IUserTableField" />
    public abstract class UserTableFieldAttributeBase : Attribute, IUserTableField {
        private string _description;

        private string _linkedTable;
        private string _name;
        private string[] _validValuePairs;

        /// <summary>
        /// Specifies the linked table type for a user table field.
        /// This value represents the .NET type of the table that serves as the foreign key reference
        /// or linked entity in the SAP Business One User Table structure.
        /// </summary>
        /// <remarks>
        /// When setting this property, it defines a relationship between the current user table field
        /// and another table, represented by the provided type. This is typically used to establish
        /// database-like relationships in user-defined tables.
        /// </remarks>
        /// <value>
        /// The type of the linked table, which corresponds to the target user table.
        /// </value>
        /// <seealso cref="UserTableFieldAttributeBase" />
        public Type LinkedTableType { get; set; }

        /// <inheritdoc cref="IUserTableField.Name" />
        public virtual string Name
        {
            get => _name;
            set
            {
                if (value != null && value.Length > 50)
                    throw new NameTooLongException(string.Format(Texts.UserTableFieldAttributeBase_Name_Name_cannot_exceed_50_characters__Provided_length___0__, value.Length));
                _name = value;
            }
        }

        /// <inheritdoc cref="IUserTableField.Description" />
        public virtual string Description
        {
            get => _description;
            set
            {
                if (value != null && value.Length > 80)
                    throw new DescriptionTooLongException(string.Format(Texts.UserTableFieldAttributeBase_Description_Description_cannot_exceed_80_characters__Provided_length___0__, value.Length));
                _description = value;
            }
        }

        /// <inheritdoc />
        public abstract BoFieldTypes FieldType { get; }

        /// <inheritdoc />
        public virtual BoFldSubTypes SubType => BoFldSubTypes.st_None;

        /// <inheritdoc />
        public virtual bool Mandatory { get; set; }

        /// <inheritdoc />
        public virtual int Size { get; set; } = 1;

        /// <inheritdoc />
        public abstract object DefaultValue { get; set; }

        /// <inheritdoc />
        public abstract object ParseValue(object value);
        /// <inheritdoc />
        public abstract string ToSapData(object value);

        /// <inheritdoc />
        public abstract bool ValidateField(object value);

        /// <inheritdoc />
        public virtual string ToColumnData(object value) => ToSapData(value);

        /// <inheritdoc />
        public abstract Type Type { get; }

        /// <inheritdoc />
        public virtual IList<IValidValue> ValidValues { get; internal set; }

        /// <inheritdoc />
        public virtual string[] ValidValuePairs
        {
            get => _validValuePairs;
            set
            {
                _validValuePairs = value;
                ValidValues = UserFieldValidValue.ParseValidValuePairs(value);
            }
        }

        /// <inheritdoc />
        public UDFLinkedSystemObjectTypesEnum LinkedSystemObject { get; set; }

        /// <inheritdoc />
        public string LinkedUdo { get; set; }

        /// <inheritdoc />
        public string LinkedTable
        {
            get
            {
                if (_linkedTable != null) return _linkedTable;
                if (LinkedTableType == null) return null;
                IUserTable attr = UserTableMetadataCache.GetUserTableAttribute(LinkedTableType);
                return attr?.Name;
            }
            set => _linkedTable = value;
        }
    }
}