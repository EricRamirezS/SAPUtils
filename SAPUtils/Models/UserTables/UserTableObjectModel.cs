using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Models;
using SAPUtils.__Internal.Repository;
using SAPUtils.__Internal.Utils;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Exceptions;
using SAPUtils.Utils;
using IUserTable = SAPUtils.__Internal.Attributes.UserTables.IUserTable;

// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global

namespace SAPUtils.Models.UserTables {

    /// <inheritdoc />
    public abstract class UserTableObjectModel : IUserTableObjectModel {

        internal string OriginalCode { get; private set; }
        internal bool? OriginalActive { get; private set; }
        internal DateTime OriginalCreatedAt { get; private set; }
        internal int OriginalCreatedBy { get; private set; }

        private static ILogger Log => Logger.Instance;

        /// <inheritdoc />
        public abstract string Code { get; set; }

        /// <inheritdoc />
        public abstract string Name { get; set; }

        /// <inheritdoc />
        public abstract bool Add();
        /// <inheritdoc />
        public abstract bool Delete(bool restore = false);
        /// <inheritdoc />
        public abstract bool Update();
        /// <inheritdoc />
        public abstract bool Save();

        /// <summary>
        /// Retrieves all records from the specified user table and maps them to instances of the given type.
        /// </summary>
        /// <typeparam name="T">
        /// A type that implements <see cref="IUserTableObjectModel"/> and is decorated with <see cref="UserTableAttribute"/>.
        /// </typeparam>
        /// <returns>
        /// A list of objects of type <typeparamref name="T"/> representing the records found in the user table.
        /// </returns>
        public static List<T> GetAll<T>() where T : IUserTableObjectModel, new() {
            List<T> data = new List<T>();
            Type type = typeof(T);
            Log.Debug("Fetching All {0} with Code: {1}", type.FullName);

            IUserTable userTable = UserTableMetadataCache.GetUserTableAttribute(typeof(T));
            if (userTable == null) {
                Log.Error("UserTableAttribute not found in {0}", type.FullName);
                return data;
            }

            Log.Debug("Fetching all {0} from table {1}", type.FullName, userTable.Name);

            string tableName = userTable.Name;
            Recordset rs = null;
            try {
                rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                string query = $"SELECT * FROM \"@{tableName}\"";
                rs.DoQuery(query);

                while (!rs.EoF) {
                    T item = new T {
                        Code = rs.Fields.Item("Code").Value.ToString(),
                        Name = rs.Fields.Item("Name").Value.ToString(),
                    };
                    PopulateFields(rs.Fields, type, tableName, ref item);
                    data.Add(item);

                    rs.MoveNext();
                }
            }
            finally {
                if (rs != null && Marshal.IsComObject(rs)) Marshal.ReleaseComObject(rs);
            }
            data = data
                .OrderByDescending(e => e is ISoftDeletable deletable && deletable.Active)
                .ThenBy(e => e.Code)
                .ToList();
            return data;
        }
        private static void PopulateFields<T>(Fields fields, Type type, string tableName, ref T item) where T : IUserTableObjectModel, new() {
            foreach ((PropertyInfo propertyInfo, IUserTableField userTableField) in UserTableMetadataCache.GetUserFields(type)) {
                string fieldName = string.IsNullOrWhiteSpace(userTableField.Name)
                    ? propertyInfo.Name
                    : userTableField.Name;
                Log.Trace("Processing field: {0}.{1}", tableName, fieldName);
                if (userTableField is DateTimeUserTableFieldAttribute dtUserTableField) {
                    Field date = fields.Item($"U_{fieldName}Date");
                    Field time = fields.Item($"U_{fieldName}Time");
                    if (date.IsNull() != BoYesNoEnum.tNO || time.IsNull() != BoYesNoEnum.tNO) continue;

                    DateTime d = (DateTime)date.Value;
                    DateTime t = (DateTime)time.Value;
                    DateTime dt = new DateTime(d.Year, d.Month, d.Day, t.Hour, t.Minute, t.Second, d.Kind);
                    propertyInfo.SetValue(item, dtUserTableField.ParseValue(dt));
                }
                else {
                    Field field = fields.Item($"U_{fieldName}");
                    Log.Trace("Processing field: {0}.{1} = {2}", tableName, fieldName, field.Value);
                    if (field.IsNull() == BoYesNoEnum.tNO) {
                        propertyInfo.SetValue(item, userTableField.ParseValue(field.Value));
                    }
                }
            }
            if (!(item is UserTableObjectModel itemModel)) return;

            itemModel.OriginalCode = item.Code;
            if (itemModel is ISoftDeletable softDeletable) {
                itemModel.OriginalActive = softDeletable.Active;
            }

            if (itemModel is IAuditableDate dateAudit) {
                itemModel.OriginalCreatedAt = dateAudit.CreatedAt;
            }

            if (itemModel is IAuditableUser userAudit) {
                itemModel.OriginalCreatedBy = userAudit.CreatedBy;
            }
        }

        /// <summary>
        /// Retrieves a record from the specified user table by its code and maps it to an instance of the given type.
        /// </summary>
        /// <typeparam name="T">
        /// A type that implements <see cref="IUserTableObjectModel"/> and is decorated with <see cref="UserTableAttribute"/>.
        /// </typeparam>
        /// <param name="code">The code of the record to retrieve.</param>
        /// <param name="item">
        /// When this method returns, contains the retrieved item if found; otherwise, the default value of <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the record was found and mapped successfully; otherwise, <c>false</c>.
        /// </returns>
        public static bool Get<T>(string code, out T item) where T : IUserTableObjectModel, new() {
            Type type = typeof(T);
            Log.Debug("Fetching {0} with Code: {1}", type.FullName, code);
            item = default;
            IUserTable userTable = UserTableMetadataCache.GetUserTableAttribute(typeof(T));
            if (userTable == null) {
                Log.Error("UserTableAttribute not found in {0}", type.FullName);
                return false;
            }

            Log.Debug("Fetching {0} from table {1} with Code: {2}", type.FullName, userTable.Name, code);

            string tableName = userTable.Name;
            UserTable table = SapAddon.Instance().Company.UserTables.Item(tableName);
            if (!table.GetByKey(code)) {
                Log.Info("{0} from table {1} not found with Code: {2}", type.FullName, tableName, code);
                return false;
            }

            Log.Debug("{0} from table {1} found in {0}, populating properties...", type.FullName, tableName, code);
            item = new T {
                Code = table.Code,
                Name = table.Name,
            };

            PopulateFields(table.UserFields.Fields, type, tableName, ref item);

            Log.Trace("{0} successfully populated for Code: {1}", type.FullName, code);
            return true;
        }
    }

    /// <summary>
    /// Represents a generic base class for interacting with SAP B1 User Tables, providing CRUD operations and mapping logic.
    /// </summary>
    /// <typeparam name="T">
    /// The derived type that extends <see cref="UserTableObjectModel{T}"/>. This type must be decorated with 
    /// <see cref="UserTableAttribute"/> and define the structure of the user table.
    /// </typeparam>
    /// <remarks>
    /// This class provides built-in support for:
    /// <list type="bullet">
    /// <item><description>Retrieving and saving user table data</description></item>
    /// <item><description>Auto-generating codes using various <see cref="PrimaryKeyStrategy"/> strategies</description></item>
    /// <item><description>Soft deletion and auditing when applicable</description></item>
    /// </list>
    /// </remarks>
    public class UserTableObjectModel<T> : UserTableObjectModel where T : UserTableObjectModel<T>, new() {

        private readonly IUserTable _userTableAttribute;

        /// <summary>
        /// Initializes a new instance of <see cref="UserTableObjectModel{T}"/>, validating the presence of <see cref="UserTableAttribute"/>.
        /// </summary>
        /// <exception cref="UserTableAttributeNotFound">
        /// Thrown when the <typeparamref name="T"/> type does not define a valid <see cref="UserTableAttribute"/>.
        /// </exception>
        protected UserTableObjectModel() {
            Log.Trace("Initializing UserTableObjectModel...");
            try {
                _userTableAttribute = UserTableMetadataCache.GetUserTableAttribute(typeof(T));
            }
            catch (Exception ex) {
                Log.Error(ex);
                throw new UserTableAttributeNotFound(typeof(T).Name);
            }

            Log.Debug("UserTableAttribute initialized: {0}", _userTableAttribute.Name);
        }

        /// <inheritdoc />
        public override string Code { get; set; }

        /// <inheritdoc />
        public override string Name { get; set; }

        private static ILogger Log => Logger.Instance;
        /// <summary>
        /// Retrieves all records of type <typeparamref name="T"/> from the corresponding user table.
        /// </summary>
        /// <returns>
        /// A list of all records as instances of <typeparamref name="T"/>.
        /// </returns>
        public static List<T> GetAll() {
            return GetAll<T>();
        }

        /// <summary>
        /// Retrieves a record of type <typeparamref name="T"/> using the specified code.
        /// </summary>
        /// <param name="code">The code of the record to retrieve.</param>
        /// <param name="item">
        /// When this method returns, contains the retrieved item if found; otherwise, the default value of <typeparamref name="T"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the item was found; otherwise, <c>false</c>.
        /// </returns>
        public static bool Get(string code, out T item) {
            return UserTableObjectModel.Get(code, out item);
        }


        /// <inheritdoc />
        public override bool Add() {
            Log.Debug("Attempting to add {0} into table {1} with Code: {2}", GetType().Name, _userTableAttribute.Name, Code);
            try {
                SAPbobsCOM.IUserTable table = SapAddon.Instance().Company.UserTables.Item(_userTableAttribute.Name);
                PrimaryKeyStrategy primaryKeyStrategy = _userTableAttribute.PrimaryKeyStrategy;
                GenerateCode(primaryKeyStrategy, table);

                if (Save()) return true;

                if (_userTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual) return false;
                Code = null;
                return false;
            }
            catch (ItemAlreadyExistException ex) {
                Log.Error(ex);
            }
            catch (Exception ex) {
                Log.Critical(ex);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Update() {
            Log.Debug("Attempting to update {0} in table {1} with Code: {2}", GetType().Name, _userTableAttribute.Name, Code);
            try {
                Log.Trace("Retrieving SAP User Table `{0}`...", _userTableAttribute.Name);
                UserTable table = SapAddon.Instance().Company.UserTables.Item(_userTableAttribute.Name);
                RestoreOriginalCode();

                if (table.GetByKey(Code)) return Save();

                throw new ItemDoNotExistException(GetType().Name, _userTableAttribute.Name, Code);
            }
            catch (ItemDoNotExistException ex) {
                Log.Error(ex);
            }
            catch (Exception ex) {
                Log.Critical(ex);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Delete(bool restore = false) {
            Log.Debug("Attempting to delete {0} from table {1} with Code: {2}", GetType().Name, _userTableAttribute.Name, Code);
            try {
                UserTable table = SapAddon.Instance().Company.UserTables.Item(_userTableAttribute.Name);
                RestoreOriginalCode();

                if (!table.GetByKey(Code)) {
                    Log.Info("{0} from table {1} with Code {2} does not exist.", GetType().Name, _userTableAttribute.Name, Code);
                    return false;
                }

                int result = -1;
                if (this is ISoftDeletable softDeletable) {
                    if (!restore) {
                        softDeletable.Active = false;
                    }
                    if (Save()) {
                        result = 0;
                    }
                }
                else {
                    result = table.Remove();
                }

                if (result == 0) {
                    Log.Info("{0} from table {1} with Code {3} deleted successfully.", GetType().Name, _userTableAttribute.Name, Code);
                    return true;
                }

                SapAddon.Instance().Company.GetLastError(out int errCode, out string errMsg);
                Log.Error(
                    "Failed to delete {0} from table {1} with Code {2}. SAP Error Code: {3}. SAP Error Message: {4}",
                    GetType().Name,
                    _userTableAttribute.Name,
                    Code,
                    errCode,
                    errMsg);
                return false;

            }
            catch (Exception ex) {
                Log.Critical(ex);
            }

            return false;
        }

        /// <inheritdoc />
        public override bool Save() {
            try {
                UserTable table = SapAddon.Instance().Company.UserTables.Item(_userTableAttribute.Name);
                if (Code == null && OriginalCode == null) {
                    PrimaryKeyStrategy primaryKeyStrategy = _userTableAttribute.PrimaryKeyStrategy;
                    GenerateCode(primaryKeyStrategy, table);
                }
                Log.Debug("Saving {0} into table {1} with Code: {2}", GetType().Name, _userTableAttribute.Name, Code);
                bool exist = table.GetByKey(Code);
                if (this is ISoftDeletable deletable && OriginalActive.HasValue) {
                    deletable.Active = OriginalActive.Value;
                }

                if (exist) {
                    Log.Trace("Updating existing {0} in table {1} with Code: {2}", GetType().Name, _userTableAttribute.Name, Code);
                    if (this is IAuditableDate dateAudit) {
                        dateAudit.CreatedAt = OriginalCreatedAt;
                        dateAudit.UpdatedAt = DateTime.Now;
                    }

                    if (this is IAuditableUser userAudit) {
                        userAudit.CreatedBy = OriginalCreatedBy;
                        userAudit.UpdatedBy = SapAddon.Instance().Company.UserSignature;
                    }
                }
                else {
                    Log.Trace("Inserting new {0} into table {1} with Code: {2}", GetType().Name, _userTableAttribute.Name, Code);
                    table.Code = Code;

                    if (this is IAuditableDate dateAudit) {
                        dateAudit.CreatedAt = DateTime.Now;
                        dateAudit.UpdatedAt = DateTime.Now;
                    }

                    if (this is IAuditableUser userAudit) {
                        userAudit.CreatedBy = SapAddon.Instance().Company.UserSignature;
                        userAudit.UpdatedBy = SapAddon.Instance().Company.UserSignature;
                    }
                }

                table.Name = Name;

                foreach ((PropertyInfo propertyInfo, IUserTableField userTableField) in UserTableMetadataCache.GetUserFields(GetType())) {
                    Log.Trace("Processing {0}'s field {1} = {2}", GetType().Name, propertyInfo.Name, propertyInfo.GetValue(this));
                    string fieldName = string.IsNullOrWhiteSpace(userTableField.Name)
                        ? propertyInfo.Name
                        : userTableField.Name;

                    object value = propertyInfo.GetValue(this);
                    if (exist == false && value == null && userTableField.DefaultValue != null) {
                        propertyInfo.SetValue(this, userTableField.DefaultValue);
                        value = propertyInfo.GetValue(this);
                    }

                    switch (userTableField) {
                        case DateTimeUserTableFieldAttribute _:
                        {
                            DateTime? dateTime = (DateTime?)value;
                            table.UserFields.Fields.Item($"U_{fieldName}Date").Value = dateTime;
                            table.UserFields.Fields.Item($"U_{fieldName}Time").Value = dateTime;
                            break;
                        }
                        case DateUserTableFieldAttribute _:
                        case TimeUserTableFieldAttribute _:
                        {
                            DateTime? dateTime = (DateTime?)value;
                            table.UserFields.Fields.Item($"U_{fieldName}").Value = dateTime;
                            break;
                        }
                        default:
                            try {
                                table.UserFields.Fields.Item($"U_{fieldName}").Value =
                                    userTableField.ToSapData(value);
                            }
                            catch (Exception ex) {
                                Log.Warning("Failed to assign value to field {0}: {1}", fieldName, ex.Message);
                                throw;
                            }
                            break;
                    }
                }

                int result = exist ? table.Update() : table.Add();

                if (result == 0) {
                    Log.Info("{0} from table {1} with Code {2} saved successfully.", GetType().Name, _userTableAttribute.Name, Code);
                    return true;
                }

                SapAddon.Instance().Company.GetLastError(out int errCode, out string errMsg);
                Log.Error(
                    "Failed to save {0} in table {1} with Code {2}. SAP Error Code: {3}. SAP Error Message: {4}",
                    GetType().Name, _userTableAttribute.Name, Code, errCode, errMsg);
                return false;
            }
            catch (Exception ex) {
                Log.Critical(ex);
            }

            return false;
        }

        private void GenerateCode(PrimaryKeyStrategy primaryKeyStrategy, SAPbobsCOM.IUserTable table) {
            switch (primaryKeyStrategy) {
                case PrimaryKeyStrategy.Manual when Code == null:
                    throw new CodeNotSetException(_userTableAttribute.Name);
                case PrimaryKeyStrategy.Manual when table.GetByKey(Code):
                    throw new ItemAlreadyExistException(_userTableAttribute.Name, Code);
                case PrimaryKeyStrategy.Guid:
                    Code = Guid.NewGuid().ToString();
                    break;
                case PrimaryKeyStrategy.Serie:
                    using (IRepository repository = Repository.Get())
                        Code = repository.GetNextCodeUserTable(_userTableAttribute.Name).ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(PrimaryKeyStrategy));
            }
            if (Name == null) {
                Name = Code;
            }
        }

        private void RestoreOriginalCode() {
            if (Code == OriginalCode) return;
            Log.Debug("Reverting {0} Code to: {1}", GetType().Name, OriginalCode);
            Code = OriginalCode;
        }
    }

}