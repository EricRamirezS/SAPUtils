using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Models;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Models.UserTables;
using IUserTable = SAPUtils.__Internal.Attributes.UserTables.IUserTable;

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
        public void InitializeUserTables(Type[] tables) {
            foreach (Type table in tables) {
                Logger.Trace("Processing table: {0}", table.Name);
                if (!ValidateTable(table)) continue;

                if (!ValidateFields(table)) continue;

                IUserTable userTable = (IUserTable)table.GetCustomAttributes(typeof(UserTableAttribute), true).First();

                Logger.Trace("Creating user table: {0}", userTable.Name);
                CreateUserTable(userTable);

                foreach (PropertyInfo propertyInfo in table.GetProperties().Where(p => !IsIgnoredField(p))) {
                    if (propertyInfo.Name == "Code" || propertyInfo.Name == "Name") continue;
                    if (IsIgnoredField(propertyInfo)) continue;
                    Logger.Trace("Processing field: {0}.{1}", userTable.Name, propertyInfo.Name);
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
                            dtUserTableField.Name + "Date",
                            dtUserTableField.DateDescription,
                            dtUserTableField.FieldType,
                            dtUserTableField.SubType,
                            dtUserTableField.Mandatory,
                            dtUserTableField.Size,
                            null,
                            dtUserTableField.ValidValues,
                            dtUserTableField.LinkedSystemObject,
                            dtUserTableField.LinkedTable,
                            dtUserTableField.LinkedUdo);
                        Thread.Sleep(100);
                        CreateUserTableField(
                            userTable.Name,
                            dtUserTableField.Name + "Time",
                            dtUserTableField.TimeDescription,
                            dtUserTableField.FieldType,
                            BoFldSubTypes.st_Time,
                            dtUserTableField.Mandatory,
                            dtUserTableField.Size,
                            null,
                            dtUserTableField.ValidValues,
                            dtUserTableField.LinkedSystemObject,
                            dtUserTableField.LinkedTable,
                            dtUserTableField.LinkedUdo);
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
                            userTableField.LinkedUdo);
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
        public void AddUserField(string tableName, IUserField fieldInfo) {
            CreateUserTableField(tableName,
                fieldInfo.Name,
                fieldInfo.Description,
                fieldInfo.FieldType,
                fieldInfo.SubType,
                fieldInfo.Mandatory,
                fieldInfo.Size,
                fieldInfo.DefaultValue.ToString(),
                fieldInfo.ValidValues,
                fieldInfo.LinkedSystemObject,
                fieldInfo.LinkedTable,
                fieldInfo.LinkedUdo);
        }

        private void CreateUserTable(IUserTable userTable) {
            IUserTablesMD userTableMd = null;

            try {
                userTableMd = Company.GetBusinessObject(BoObjectTypes.oUserTables) as UserTablesMD;

                Logger.Trace("Verifying if table {0} already exist", userTable.Name);
                if (userTableMd != null && userTableMd.GetByKey(userTable.Name)) {
                    Logger.Info("Table {0} already exist", userTable.Name);
                    return;
                }

                if (userTableMd == null) return;

                userTableMd.TableName = userTable.Name;
                userTableMd.TableDescription = userTable.Description;
                userTableMd.TableType = userTable.TableType;

                if (userTableMd.Add() == 0) {
                    Logger.Info("Table {0} created", userTable.Name);
                    return;
                }

                string error = Company.GetLastErrorDescription();
                throw new Exception($"No es posible agregar la tabla de usuario {userTable.Name}. Error {error}");
            }
            finally {
                if (userTableMd != null) Marshal.ReleaseComObject(userTableMd);
                GC.Collect();
            }
        }

        private void CreateUserTableField(string tableName,
            string fieldName,
            string fieldDescription,
            BoFieldTypes fieldType,
            BoFldSubTypes fieldSubType,
            bool mandatory,
            int? size,
            string defaultValue,
            IList<IUserFieldValidValue> validValues,
            UDFLinkedSystemObjectTypesEnum? linkedSystemObject = null,
            string linkedTable = null,
            string linkedUdo = null) {
            UserFieldsMD userFieldsMd = null;

            try {
                userFieldsMd = Company.GetBusinessObject(BoObjectTypes.oUserFields) as UserFieldsMD;

                if (ExistTableField(tableName, fieldName)) {
                    Logger.Debug("Field {0}.{1} already exist", tableName, fieldName);
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

                if (string.IsNullOrEmpty(defaultValue) == false) {
                    userFieldsMd.DefaultValue = defaultValue;
                }

                if (validValues != null && validValues.Count > 0) {
                    foreach (IUserFieldValidValue validValue in validValues) {
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
                    Logger.Info("Field {0}.{1} created", tableName, fieldName);
                }
                else {
                    string error = Company.GetLastErrorDescription();
                    throw new Exception(
                        $"No es posible agregar el campo {fieldName} a la tabla {tableName}. Error {error}");
                }
            }
            finally {
                if (userFieldsMd != null) Marshal.ReleaseComObject(userFieldsMd);
                GC.Collect();
            }
        }

        private bool ExistTableField(string tableName, string fieldName) {
            Recordset recordset = null;
            Logger.Trace("Verifying if field {0}.{1} already exist", tableName, fieldName);
            try {
                recordset = Company.GetBusinessObject(BoObjectTypes.BoRecordset) as Recordset;
                if (recordset != null) {
                    recordset.DoQuery(string.Format(IsHana
                            ? @"SELECT '0' AS ""IGNORE"" FROM CUFD WHERE ""TableID"" = '@{0}' AND ""AliasID"" = '{1}'"
                            : "SELECT NULL FROM CUFD WHERE TableId = '@{0}' AND AliasId = '{1}'",
                        tableName, fieldName));
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
            Logger.Trace("Verifying table {0} implements UserTableObjectModel", table.Name);
            if (!UserTableObjectModelType.IsAssignableFrom(table)) {
                Logger.Error("{0} is not assignable From IUserTableObjectModel", table.Name);
                return false;
            }

            Logger.Trace("Veryfing table {0} has Attribute UserTableAttribute", table.Name);
            if (table.GetCustomAttributes(typeof(UserTableAttribute), true).FirstOrDefault() is UserTableAttribute) return true;

            Logger.Error("UserTable Attribute not found in {0}", table.Name);
            return false;

        }


        private bool ValidateFields(Type table) {
            bool valid = true;
            foreach (PropertyInfo propertyInfo in table.GetProperties()) {
                if (propertyInfo.Name == "Code" || propertyInfo.Name == "Name") continue;

                if (AuditableField.IsAuditableField(table, propertyInfo))
                    continue;

                if (IsIgnoredField(propertyInfo)) {
                    Logger.Trace("{0} property `{1}` markes as Ignored", table.Name, propertyInfo.Name);
                    continue;
                }

                if (!(propertyInfo.GetCustomAttributes(typeof(IUserTableField), true)
                        .FirstOrDefault() is IUserTableField userTableField)) {
                    Logger.Error("IUserTableField not found in {0}.{1}", table.Name, propertyInfo.Name);
                    valid = false;
                }
                else if (NormalizeType(userTableField.Type) != NormalizeType(propertyInfo.PropertyType)) {
                    Logger.Error(
                        "IUserTableField {0}.{1} is not valid. " +
                        "Expected property type: {2}, but got: {3}",
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