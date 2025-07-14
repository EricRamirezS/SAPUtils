using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using SAPbobsCOM;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Internal.Attributes.UserTables;
using SAPUtils.Internal.Models;
using SAPUtils.Models.UserTables;

namespace SAPUtils {
    public partial class SapAddon {
        internal bool IsHana => Company.DbServerType == BoDataServerTypes.dst_HANADB;

        public void InitializeUserData(Type[] tables) {
            foreach (Type table in tables) {
                Logger.Trace("Processing table: {0}", table.Name);
                if (!ValidateTable(table)) continue;

                if (!ValidateFields(table)) continue;

                UserTableAttribute userTable =
                    (UserTableAttribute)table.GetCustomAttributes(typeof(UserTableAttribute), true).First();

                Logger.Trace("Creating user table: {0}", userTable.Name);
                CreateUserTable(userTable.Name, userTable.Description);

                foreach (PropertyInfo propertyInfo in table.GetProperties()) {
                    if (propertyInfo.Name == "Code" || propertyInfo.Name == "Name") continue;
                    Logger.Trace("Processing field: {0}.{1}", userTable.Name, propertyInfo.Name);
                    IUserTableField userTableField;
                    if (!AuditableField.IsAuditableField(table, propertyInfo)) {

                        userTableField = (IUserTableField)propertyInfo.GetCustomAttributes(typeof(IUserTableField), true).First();

                        if (string.IsNullOrWhiteSpace(userTableField.Name)) {
                            userTableField.Name = propertyInfo.Name;
                        }

                        if (string.IsNullOrWhiteSpace(userTableField.Description)
                            && typeof(DateTimeUserTableFieldAttribute) != userTableField.GetType()) {
                            userTableField.Description = propertyInfo.Name;
                        }
                    }
                    else {
                        userTableField = AuditableField.GetUserTableField(table, propertyInfo);
                    }
                    if (userTableField is DateTimeUserTableFieldAttribute dtUserTableField) {
                        CreateUserTableField(
                            userTable.Name,
                            dtUserTableField.Name + "Date",
                            dtUserTableField.DateDescription,
                            dtUserTableField.FieldType,
                            dtUserTableField.SubType,
                            dtUserTableField.Required,
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
                            dtUserTableField.Required,
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
                            userTableField.Required,
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


        private void CreateUserTable(string tableName, string tableDescription) {
            IUserTablesMD userTableMd = null;

            try {
                userTableMd = Company.GetBusinessObject(BoObjectTypes.oUserTables) as UserTablesMD;

                Logger.Trace("Verifying if table {0} already exist", tableName);
                if (userTableMd != null && userTableMd.GetByKey(tableName)) {
                    Logger.Info("Table {0} already exist", tableName);
                    return;
                }

                if (userTableMd == null) return;

                userTableMd.TableName = tableName;
                userTableMd.TableDescription = tableDescription;

                if (userTableMd.Add() == 0) {
                    Logger.Info("Table {0} created", tableName);
                    return;
                }

                string error = Company.GetLastErrorDescription();
                throw new Exception($"No es posible agregar la tabla de usuario {tableName}. Error {error}");
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
                    foreach (UserFieldValidValue validValue in validValues) {
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
                        $"No es posible agregar el campo {fieldName} a la tabla de usuario {tableName}. Error {error}");
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
                recordset.DoQuery(string.Format(IsHana
                        ? @"SELECT '0' AS ""IGNORE"" FROM CUFD WHERE ""TableID"" = '@{0}' AND ""AliasID"" = '{1}'"
                        : "SELECT NULL FROM CUFD WHERE TableId = '@{0}' AND AliasId = '{1}'",
                    tableName, fieldName));
                return recordset.RecordCount > 0;
            }
            finally {
                if (recordset != null) Marshal.ReleaseComObject(recordset);
                GC.Collect();
            }
        }

        private static readonly Type userTableObjectModelType = typeof(IUserTableObjectModel);

        private bool ValidateTable(Type table) {
            Logger.Trace("Verifying table {0} implements UserTableObjectModel", table.Name);
            if (!userTableObjectModelType.IsAssignableFrom(table)) {
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

                if (!(propertyInfo.GetCustomAttributes(typeof(IUserTableField), true)
                        .FirstOrDefault() is IUserTableField userTableField)) {
                    Logger.Error("UserTableFieldAttribute not found in {0}.{1}", table.Name, propertyInfo.Name);
                    valid = false;
                }
                else if (userTableField.Type != propertyInfo.PropertyType) {
                    Logger.Error(
                        "UserTableFieldAttribute {0}.{1} is not valid. " +
                        "Expected property type: {2}, but got: {3}",
                        table.Name, propertyInfo.Name, userTableField.Type, propertyInfo.PropertyType);
                    valid = false;
                }
            }

            return valid;
        }
    }
}