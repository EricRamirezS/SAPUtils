using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Models;
using SAPUtils.Attributes.UserTables;
using SAPUtils.I18N;
using SAPUtils.Models.UserTables;
using IUserTable = SAPUtils.__Internal.Attributes.UserTables.IUserTable;
using IValidValue = SAPbouiCOM.IValidValue;

namespace SAPUtils {
    /// <summary>
    /// Represents the main SAP AddOn class used for initializing and managing the connection
    /// and interactions with SAP Business One's COM objects.
    /// </summary>
    public partial class SapAddon {

        private static readonly Type UserTableObjectModelType = typeof(IUserTableObjectModel);
        internal bool IsHana => Company.DbServerType == BoDataServerTypes.dst_HANADB;

        /// <summary>
        /// Initializes user-defined table data within SAP Business One by processing the provided table types,
        /// creating user tables and their associated fields as defined by attributes.
        /// </summary>
        /// <param name="tables">An array of <see cref="Type"/> objects representing user-defined table classes to be initialized.</param>
        /// <remarks>
        /// Types must implement <see cref="IUserTableObjectModel"/> interface and be decorated with <see cref="UserTableAttribute"/>.
        /// </remarks>
        [SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
        public void InitializeUserTables(Type[] tables) {
            foreach (Type table in tables) {
                Logger.Trace(Texts.SapAddon_InitializeUserTables_Processing_table___0_, table.Name);
                if (!ValidateTable(table)) continue;

                if (!ValidateFields(table)) continue;

                IUserTable userTable = (IUserTable)table.GetCustomAttributes(typeof(UserTableAttribute), true).First();

                Logger.Trace(Texts.SapAddon_InitializeUserTables_Creating_user_table___0_, userTable.Name);
                CreateUserTable(userTable);

                foreach (PropertyInfo propertyInfo in table.GetProperties().Where(p => !IsIgnoredField(p))) {
                    if (propertyInfo.Name == "Code" || propertyInfo.Name == "Name") continue;
                    if (IsIgnoredField(propertyInfo)) continue;
                    Logger.Trace(Texts.SapAddon_InitializeUserTables_Processing_field___0___1_, userTable.Name, propertyInfo.Name);
                    IUserTableField userTableField;
                    if (!AuditableField.IsAuditableField(table, propertyInfo)) {

                        userTableField = (IUserTableField)propertyInfo.GetCustomAttributes(typeof(IUserTableField), true).First();

                        if (string.IsNullOrWhiteSpace(userTableField.Name)) {
                            userTableField.Name = propertyInfo.Name;
                        }

                        if (string.IsNullOrWhiteSpace(userTableField.Description)
                            && typeof(DateTimeFieldAttribute) != userTableField.GetType()) {
                            userTableField.Description = propertyInfo.Name;
                        }
                    }
                    else {
                        userTableField = AuditableField.GetUserTableField(table, propertyInfo);
                    }
                    if (userTableField is DateTimeFieldAttribute dtUserTableField) {
                        CreateUserTableField(
                            userTable.Name,
                            $"{dtUserTableField.Name}Date",
                            dtUserTableField.DateDescription,
                            dtUserTableField.FieldType,
                            dtUserTableField.SubType,
                            dtUserTableField.Mandatory,
                            dtUserTableField.Size,
                            dtUserTableField.DefaultValue != null
                                ? dtUserTableField.DateToSapData(dtUserTableField.ParseDateValue(dtUserTableField.DefaultValue))
                                : null,
                            dtUserTableField.ValidValues,
                            dtUserTableField.LinkedSystemObject,
                            dtUserTableField.LinkedTable,
                            dtUserTableField.LinkedUdo,
                            true);
                        Thread.Sleep(100);
                        CreateUserTableField(
                            userTable.Name,
                            $"{dtUserTableField.Name}Time",
                            dtUserTableField.TimeDescription,
                            dtUserTableField.FieldType,
                            BoFldSubTypes.st_Time,
                            dtUserTableField.Mandatory,
                            dtUserTableField.Size,
                            dtUserTableField.DefaultValue != null
                                ? dtUserTableField.TimeToSapData(dtUserTableField.ParseTimeValue(dtUserTableField.DefaultValue))
                                : null,
                            dtUserTableField.ValidValues,
                            dtUserTableField.LinkedSystemObject,
                            dtUserTableField.LinkedTable,
                            dtUserTableField.LinkedUdo,
                            true);
                    }
                    else {
                        CreateUserTableField(
                            userTable.Name,
                            userTableField.Name,
                            userTableField.Description,
                            userTableField.FieldType,
                            userTableField.SubType,
                            userTableField.Mandatory,
                            userTableField.Size,
                            userTableField.DefaultValue == null
                                ? null
                                : userTableField.ToSapData(userTableField.DefaultValue),
                            userTableField.ValidValues,
                            userTableField.LinkedSystemObject,
                            userTableField.LinkedTable,
                            userTableField.LinkedUdo,
                            true);
                    }
                }
            }
        }

        /// <summary>
        /// Adds a user-defined field to the specified SAP Business One table by using the provided field information.
        /// </summary>
        /// <param name="tableName">The name of the table to which the user-defined field will be added.</param>
        /// <param name="fieldInfo">An instance of <see cref="IUserField"/> that specifies the field's name, description, type, subtype, and other related metadata.</param>
        /// <remarks>
        /// The field will be created with the configurations specified in the <paramref name="fieldInfo"/> parameter, including size, default value, valid values,
        /// and any linked objects or user-defined tables (UDO).
        /// Ensure that the specified table name exists within the SAP Business One database schema.
        /// </remarks>
        /// <seealso cref="IUserField"/>
        public void AddSapTableUserField(string tableName, IUserField fieldInfo) {
            if (fieldInfo is DateTimeFieldAttribute) {
                throw new NotSupportedException(Texts.SapAddon_AddSapTableUserField_Cannot_create_DateTimeFieldAttribute_Field_in_System_Table__use_Date_and_Time_instead_);
            }
            CreateUserTableField(tableName,
                fieldInfo.Name,
                fieldInfo.Description,
                fieldInfo.FieldType,
                fieldInfo.SubType,
                fieldInfo.Mandatory,
                fieldInfo.Size,
                fieldInfo.DefaultValue == null
                    ? null
                    : fieldInfo.ToSapData(fieldInfo.DefaultValue),
                fieldInfo.ValidValues,
                fieldInfo.LinkedSystemObject,
                fieldInfo.LinkedTable,
                fieldInfo.LinkedUdo,
                false);
        }

        private void CreateUserTable(IUserTable userTable) {
            IUserTablesMD userTableMd = null;

            try {
                userTableMd = Company.GetBusinessObject(BoObjectTypes.oUserTables) as UserTablesMD;

                Logger.Trace(Texts.SapAddon_CreateUserTable_Verifying_if_table__0__already_exist, userTable.Name);
                if (userTableMd != null && userTableMd.GetByKey(userTable.Name)) {
                    Logger.Debug(Texts.SapAddon_CreateUserTable_Table__0__already_exist, userTable.Name);
                    return;
                }

                if (userTableMd == null) return;

                userTableMd.TableName = userTable.Name;
                userTableMd.TableDescription = userTable.Description;
                userTableMd.TableType = userTable.TableType;

                if (userTableMd.Add() == 0) {
                    Logger.Info(Texts.SapAddon_CreateUserTable_Table__0__created, userTable.Name);
                    return;
                }

                string error = Company.GetLastErrorDescription();
                throw new Exception(string.Format(Texts.SapAddon_CreateUserTable_Unable_to_add_user_table__0___Error__1_, userTable.Name, error));
            }
            finally {
                if (userTableMd != null) Marshal.ReleaseComObject(userTableMd);
                GC.Collect();
            }
        }

        [Localizable(false)]
        private void CreateUserTableField(string tableName,
            string fieldName,
            string fieldDescription,
            BoFieldTypes fieldType,
            BoFldSubTypes fieldSubType,
            bool mandatory,
            int? size,
            string defaultValue,
            IList<IValidValue> validValues,
            UDFLinkedSystemObjectTypesEnum? linkedSystemObject = null,
            string linkedTable = null,
            string linkedUdo = null,
            bool userTable = true) {
            UserFieldsMD userFieldsMd = null;

            try {
                userFieldsMd = Company.GetBusinessObject(BoObjectTypes.oUserFields) as UserFieldsMD;

                if (ExistTableField(tableName, fieldName, userTable)) {
                    Logger.Debug(Texts.SapAddon_CreateUserTableField_Field__0___1__already_exist, tableName, fieldName);
                    return;
                }

                if (userFieldsMd == null) return;
                userFieldsMd.TableName = tableName;
                userFieldsMd.Name = fieldName;
                userFieldsMd.Description = fieldDescription;
                userFieldsMd.Type = fieldType;
                userFieldsMd.SubType = fieldSubType;
                userFieldsMd.Mandatory = mandatory ? BoYesNoEnum.tYES : BoYesNoEnum.tNO;

                if (size.HasValue) {
                    userFieldsMd.EditSize = size.Value;
                }

                if (defaultValue != null) {
                    userFieldsMd.DefaultValue = defaultValue;
                }

                if (validValues != null && validValues.Count > 0) {
                    foreach (IValidValue validValue in validValues) {
                        userFieldsMd.ValidValues.Value = validValue.Value;
                        userFieldsMd.ValidValues.Description = validValue.Description;
                        userFieldsMd.ValidValues.Add();
                    }
                }

                if (linkedSystemObject.HasValue) {
                    userFieldsMd.LinkedSystemObject = linkedSystemObject.Value;
                }
                if (linkedTable != null) userFieldsMd.LinkedTable = linkedTable;
                if (linkedUdo != null) userFieldsMd.LinkedUDO = linkedUdo;

                if (userFieldsMd.Add() == 0) {
                    Logger.Info(Texts.SapAddon_CreateUserTableField_Field__0___1__created, tableName, fieldName);
                }
                else {
                    string error = Company.GetLastErrorDescription();
                    throw new Exception(
                        string.Format(Texts.SapAddon_CreateUserTableField_Unable_to_add_field__0__to_table__1___Error__2_, fieldName, tableName, error));
                }
            }
            finally {
                if (userFieldsMd != null) Marshal.ReleaseComObject(userFieldsMd);
                GC.Collect();
            }
        }

        private bool ExistTableField(string tableName, string fieldName, bool userTable) {
            Recordset recordset = null;
            Logger.Trace(Texts.SapAddon_ExistTableField_Verifying_if_field__0___1__already_exist, tableName, fieldName);
            try {
                recordset = Company.GetBusinessObject(BoObjectTypes.BoRecordset) as Recordset;
                if (recordset != null) {
                    recordset.DoQuery(IsHana
                        // ReSharper disable LocalizableElement
                        ? $@"SELECT '0' AS ""IGNORE"" FROM CUFD WHERE ""TableID"" = '{(userTable ? "@" : "")}{tableName}' AND ""AliasID"" = '{fieldName}'"
                        : $"SELECT NULL FROM CUFD WHERE TableId = '{(userTable ? "@" : "")}{tableName}' AND AliasId = '{fieldName}'");
                    // ReSharper restore LocalizableElement
                    return recordset.RecordCount > 0;
                }
            }
            finally {
                if (recordset != null) Marshal.ReleaseComObject(recordset);
                GC.Collect();
            }
            return false;
        }

        private bool ValidateTable(Type table) {
            Logger.Trace(Texts.SapAddon_ValidateTable_Verifying_table__0__implements_UserTableObjectModel, table.Name);
            if (!UserTableObjectModelType.IsAssignableFrom(table)) {
                Logger.Error(Texts.SapAddon_ValidateTable__0__is_not_assignable_From_IUserTableObjectModel, table.Name);
                return false;
            }

            Logger.Trace(Texts.SapAddon_ValidateTable_Veryfing_table__0__has_Attribute_UserTableAttribute, table.Name);
            if (table.GetCustomAttributes(typeof(UserTableAttribute), true).FirstOrDefault() is UserTableAttribute) return true;

            Logger.Error(Texts.SapAddon_ValidateTable_UserTable_Attribute_not_found_in_class__0_, table.Name);
            return false;

        }


        private bool ValidateFields(Type table) {
            bool valid = true;
            foreach (PropertyInfo propertyInfo in table.GetProperties()) {
                if (propertyInfo.Name == "Code" || propertyInfo.Name == "Name") continue;

                if (AuditableField.IsAuditableField(table, propertyInfo))
                    continue;

                if (IsIgnoredField(propertyInfo)) {
                    Logger.Trace(Texts.SapAddon_ValidateFields__0__Property__1__Marked_as_Ignored, table.Name, propertyInfo.Name);
                    continue;
                }

                if (!(propertyInfo.GetCustomAttributes(typeof(IUserTableField), true)
                        .FirstOrDefault() is IUserTableField userTableField)) {
                    Logger.Error(Texts.SapAddon_ValidateFields_IUserTableField_not_found_in__0___1_, table.Name, propertyInfo.Name);
                    valid = false;
                }
                else if (NormalizeType(userTableField.Type) != NormalizeType(propertyInfo.PropertyType)) {
                    Logger.Error(
                        Texts.SapAddon_ValidateFields_IUserTableField__0___1__is_not_valid__Expected_property_type___2___but_got___3_,
                        table.Name, propertyInfo.Name, userTableField.Type, propertyInfo.PropertyType);
                    valid = false;
                }
                else {
                    userTableField.Name = string.IsNullOrWhiteSpace(userTableField.Name)
                        ? propertyInfo.Name
                        : userTableField.Name;
                    userTableField.Description = string.IsNullOrWhiteSpace(userTableField.Description)
                        ? userTableField.Name
                        : userTableField.Description;
                }
            }

            return valid;

            Type NormalizeType(Type type) =>
                Nullable.GetUnderlyingType(type) ?? type;

        }
        private static bool IsIgnoredField(PropertyInfo propertyInfo) {
            return propertyInfo.IsDefined(typeof(IgnoreFieldAttribute), true);
        }
    }
}