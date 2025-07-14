using System;
using System.Collections.Generic;
using SAPbobsCOM;
using SAPUtils.Internal.Attributes.UserTables;
using SAPUtils.Internal.Models;
using SAPUtils.Models.UserTables;

namespace SAPUtils.Attributes.UserTables {
    /// <inheritdoc cref="IUserTableField" />
    public abstract class UserTableFieldAttributeBase : Attribute, IUserTableField {
        /// <inheritdoc />
        public virtual string Name { get; set; }
        /// <inheritdoc />
        public virtual string Description { get; set; }
        /// <inheritdoc />
        public abstract BoFieldTypes FieldType { get; }
        /// <inheritdoc />
        public virtual BoFldSubTypes SubType => BoFldSubTypes.st_None;
        /// <inheritdoc />
        public virtual bool Required { get; set; }

        /// <inheritdoc />
        public virtual int Size { get; set; } = 1;

        /// <inheritdoc />
        public abstract object DefaultValue { get; set; }
        /// <inheritdoc />
        public abstract object ParseValue(object value);
        /// <inheritdoc />
        public abstract string ToSapData(object value);
        /// <inheritdoc />
        public abstract Type Type { get; }

        /// <inheritdoc />
        public IList<IUserFieldValidValue> ValidValues { get; internal set; }

        private string[] _validValuePairs;

        /// <inheritdoc />
        public string[] ValidValuePairs
        {
            get => _validValuePairs;
            set
            {
                _validValuePairs = value;
                ValidValues = UserFieldValidValue.ParseValidValuePairs(value);
            }
        }

        /// <inheritdoc />
        public UDFLinkedSystemObjectTypesEnum? LinkedSystemObject { get; set; }
        /// <inheritdoc />
        public string LinkedUdo { get; set; }

        private string _linkedTable;

        /// <inheritdoc />
        public string LinkedTable
        {
            get
            {
                if (_linkedTable != null) return _linkedTable;
                if (LinkedTableType == null) return null;
                UserTableAttribute attr = UserTableMetadataCache.GetUserTableAttribute(LinkedTableType);
                return attr?.Name;
            }
            set => _linkedTable = value;
        }

        /// <inheritdoc />
        public Type LinkedTableType { get; set; }
    }
}