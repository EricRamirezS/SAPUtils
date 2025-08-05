using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Models;
using SAPUtils.__Internal.Query;
using SAPUtils.__Internal.Utils;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Database;
using SAPUtils.Events;
using SAPUtils.Exceptions;
using SAPUtils.Query;
using SAPUtils.Utils;
using IUserTable = SAPUtils.__Internal.Attributes.UserTables.IUserTable;

// ReSharper disable SuspiciousTypeConversion.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable UnusedMember.Global

namespace SAPUtils.Models.UserTables {

    /// <inheritdoc />
    public abstract class UserTableObjectModel : IUserTableObjectModel {

        /// <summary>
        /// Represents a thread-safe cache for storing and retrieving instances of <see cref="UserTableObjectModel"/>
        /// objects based on their type and a unique code identifier.
        /// </summary>
        /// <remarks>
        /// This cache utilizes a <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>
        /// to ensure thread-safety when accessing or modifying the stored instances.
        /// </remarks>
        /// <seealso cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>
        private static readonly ConcurrentDictionary<(Type, string), UserTableObjectModel> Cache =
            new ConcurrentDictionary<(Type, string), UserTableObjectModel>();

        internal string OriginalCode { get; set; }
        internal bool? OriginalActive { get; set; }
        internal DateTime OriginalCreatedAt { get; set; }
        internal int OriginalCreatedBy { get; set; }

        /// <inheritdoc />
        public abstract string Code { get; set; }

        /// <inheritdoc />
        public abstract string Name { get; set; }

        /// <inheritdoc />
        [IgnoreField]
        public virtual string DisplayName => Name;

        /// <inheritdoc />
        public abstract bool Add();
        /// <inheritdoc />
        public abstract bool Delete();
        /// <inheritdoc />
        public abstract bool Update(bool restore = false);
        /// <inheritdoc />
        public abstract bool Save();

        /// <inheritdoc />
        public abstract string GetNextAvailableCode();

        /// <inheritdoc />
        public object Clone() {
            return MemberwiseClone();
        }

        /// <summary>
        /// Retrieves all records from the specified user table and maps them to instances of the given type.
        /// </summary>
        /// <param name="where"></param>
        /// <typeparam name="T">
        /// A type that implements <see cref="IUserTableObjectModel"/> and is decorated with <see cref="UserTableAttribute"/>.
        /// </typeparam>
        /// <returns>
        /// A list of objects of type <typeparamref name="T"/> representing the records found in the user table.
        /// </returns>
        public static List<T> GetAll<T>(IWhere where = null) where T : IUserTableObjectModel, new() {
            List<T> data = new List<T>();
            Type type = typeof(T);
            ILogger log = Logger.Instance;
            where = where ?? Where.Builder().Build();
            log.Debug("Fetching All {0}", type.FullName);

            IUserTable userTable = UserTableMetadataCache.GetUserTableAttribute(typeof(T));
            if (userTable == null) {
                log.Error("UserTableAttribute not found in {0}", type.FullName);
                return data;
            }

            log.Debug("Fetching all {0} from table {1}", type.FullName, userTable.Name);

            string tableName = userTable.Name;
            Recordset rs = null;
            try {
                rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                tableName = SapAddon.Instance().IsHana ? $"\"@{tableName}\"" : $"[@{tableName}]";
                string whereString = new SqlWhereBuilder(where).Build();
                string query = $"SELECT T0.* FROM {tableName} T0 {whereString}";
                log.Trace("Executing query: {0}", query);
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
            PrimaryKeyStrategy pks = userTable.PrimaryKeyStrategy;
            if (pks == PrimaryKeyStrategy.Serie) {
                data = data
                    .OrderByDescending(e => e is ISoftDeletable deletable && deletable.Active)
                    .ThenBy(e => int.TryParse(e.Code, out int num) ? num : int.MaxValue)
                    .ToList();
            }
            else {
                data = data
                    .OrderByDescending(e => e is ISoftDeletable deletable && deletable.Active)
                    .ThenBy(e => e.Code)
                    .ToList();
            }
            return data;
        }

        private static void PopulateFields<T>(Fields fields, Type type, string tableName, ref T item) where T : IUserTableObjectModel {
            ILogger log = Logger.Instance;
            foreach ((PropertyInfo propertyInfo, IUserTableField userTableField) in UserTableMetadataCache.GetUserFields(type)) {
                string fieldName = string.IsNullOrWhiteSpace(userTableField.Name)
                    ? propertyInfo.Name
                    : userTableField.Name;
                log.Trace("Processing field: {0}.{1}", tableName, fieldName);
                if (userTableField is DateTimeFieldAttribute dtUserTableField) {
                    Field date = fields.Item($"U_{fieldName}Date");
                    Field time = fields.Item($"U_{fieldName}Time");
                    if (date.IsNull() == BoYesNoEnum.tYES && time.IsNull() == BoYesNoEnum.tYES) continue;

                    DateTime d = (DateTime)date.Value;
                    DateTime t = dtUserTableField.ParseTimeValue(time.Value);
                    DateTime dt = new DateTime(d.Year, d.Month, d.Day, t.Hour, t.Minute, t.Second, d.Kind);
                    propertyInfo.SetValue(item, dt);
                }
                else {
                    Field field = fields.Item($"U_{fieldName}");
                    log.Trace("Processing field: {0}.{1} = {2}", tableName, fieldName, field.Value);
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
            ILogger log = Logger.Instance;
            log.Debug("Fetching {0} with Code: {1}", type.FullName, code);
            item = default;
            IUserTable userTable = UserTableMetadataCache.GetUserTableAttribute(typeof(T));
            if (userTable == null) {
                log.Error("UserTableAttribute not found in {0}", type.FullName);
                return false;
            }

            log.Debug("Fetching {0} from table {1} with Code: {2}", type.FullName, userTable.Name, code);

            string tableName = userTable.Name;
            UserTable table = SapAddon.Instance().Company.UserTables.Item(tableName);
            if (!table.GetByKey(code)) {
                log.Info("{0} from table {1} not found with Code: {2}", type.FullName, tableName, code);
                return false;
            }

            log.Debug("{0} from table {1} found in {0}, populating properties...", type.FullName, tableName, code);
            item = new T {
                Code = table.Code,
                Name = table.Name,
            };

            PopulateFields(table.UserFields.Fields, type, tableName, ref item);

            log.Trace("{0} successfully populated for Code: {1}", type.FullName, code);
            return true;
        }

        /// <summary>
        /// Retrieves a cached instance of the specified <typeparamref name="T"/> type using the provided code.
        /// If the object is not found in the cache, retrieves it using the <see cref="Get{T}(string, out T)"/> method and caches the result.
        /// </summary>
        /// <typeparam name="T">The type derived from <see cref="UserTableObjectModel"/> to fetch from the cache or database.</typeparam>
        /// <param name="code">The unique identifier (code) of the object to retrieve. Cannot be null or whitespace.</param>
        /// <returns>
        /// The cached or freshly retrieved instance of <typeparamref name="T"/> if found; otherwise, <c>null</c>.
        /// </returns>
        /// <seealso cref="Get{T}(string, out T)"/>
        static protected T GetCached<T>(string code) where T : UserTableObjectModel, new() {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            (Type, string code) key = (typeof(T), code);

            if (Cache.TryGetValue(key, out UserTableObjectModel existing) && existing is T typed)
                return typed;

            if (!Get(code, out T result))
                return null;

            Cache[key] = result;
            return result;
        }

        /// <summary>
        /// Clears the entire cache of user table objects. This method removes all cached entries
        /// regardless of their type or code, ensuring a fresh state for future operations.
        /// </summary>
        public static void ClearCache() {
            Cache.Clear();
        }

        /// <summary>
        /// Clears the cache entries for all instances of the specified type <typeparamref name="T"/> from the internal cache.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the user table object model to be cleared from the cache. This type must be a subclass of <see cref="UserTableObjectModel"/>.
        /// </typeparam>
        /// <remarks>
        /// This method iterates through the cache and removes all entries that match the specified type <typeparamref name="T"/>.
        /// </remarks>
        /// <seealso cref="UserTableObjectModel"/>
        public static void ClearCache<T>() where T : UserTableObjectModel {
            Type type = typeof(T);
            foreach ((Type, string) key in Cache.Keys.Where(k => k.Item1 == type).ToList()) {
                Cache.TryRemove(key, out _);
            }
        }

        /// <summary>
        /// Clears all cached instances of user table object models.
        /// </summary>
        public static void ClearCache<T>(string code) where T : UserTableObjectModel {
            (Type, string code) key = (typeof(T), code);
            Cache.TryRemove(key, out _);
        }

        /// <summary>
        /// Retrieves a business object of the specified type and key from the SAP Business One company database.
        /// </summary>
        /// <typeparam name="T">The type of the business object to retrieve, which must be a class.</typeparam>
        /// <param name="objectType">The type of the business object, specified as a value of the <see cref="SAPbobsCOM.BoObjectTypes"/> enumeration.</param>
        /// <param name="key">The unique key used to identify the business object in the database.</param>
        /// <returns>
        /// The retrieved business object of type <typeparamref name="T"/> if found; otherwise, <c>null</c>.
        /// </returns>
        /// <seealso cref="SAPUtils.SapAddon"/>
        public static T GetBusinessObjectByKey<T>(BoObjectTypes objectType, object key) where T : class {
            Company company = SapAddon.Instance().Company;
            dynamic obj = company.GetBusinessObject(objectType);

            //All SAP BO Business Objects should have GetByKey method
            bool found = obj.GetByKey(key);
            return found ? obj as T : null;
        }

        /// <summary>
        /// Retrieves the previous record of the specified type from a sequence of user table object models.
        /// </summary>
        /// <typeparam name="T">The type of the user table object model to retrieve, which must implement <see cref="IUserTableObjectModel"/> and have a parameterless constructor.</typeparam>
        /// <returns>
        /// The previous record of the specified type, or <c>null</c> if no record exists.
        /// </returns>
        /// <seealso cref="IUserTableObjectModel"/>
        public abstract T GetPreviousRecord<T>() where T : class, IUserTableObjectModel, new();
        /// <summary>
        /// Retrieves the next record of the specified type from a sequence of user table object models.
        /// </summary>
        /// <typeparam name="T">The type of the user table object model to retrieve, which must implement <see cref="IUserTableObjectModel"/> and have a parameterless constructor.</typeparam>
        /// <returns>
        /// The next record of the specified type, or <c>null</c> if no record exists.
        /// </returns>
        /// <seealso cref="IUserTableObjectModel"/>
        public abstract T GetNextRecord<T>() where T : class, IUserTableObjectModel, new();
        /// <summary>
        /// Retrieves the first record of the specified type from a sequence of user table object models.
        /// </summary>
        /// <typeparam name="T">The type of the user table object model to retrieve, which must implement <see cref="IUserTableObjectModel"/> and have a parameterless constructor.</typeparam>
        /// <returns>
        /// The first record of the specified type, or <c>null</c> if no record exists.
        /// </returns>
        /// <seealso cref="IUserTableObjectModel"/>
        public abstract T GetFirstRecord<T>() where T : class, IUserTableObjectModel, new();
        /// <summary>
        /// Retrieves the last record of the specified type from a sequence of user table object models.
        /// </summary>
        /// <typeparam name="T">The type of the user table object model to retrieve, which must implement <see cref="IUserTableObjectModel"/> and have a parameterless constructor.</typeparam>
        /// <returns>
        /// The last record of the specified type, or <c>null</c> if no record exists.
        /// </returns>
        /// <seealso cref="IUserTableObjectModel"/>
        public abstract T GetLastRecord<T>() where T : class, IUserTableObjectModel, new();
    }

    /// <summary>
    /// An attribute used to mark a property to be ignored by specific operations,
    /// such as mapping, reflection, or metadata generation.
    /// </summary>
    /// <remarks>
    /// This attribute is commonly utilized in scenarios where properties should not
    /// be included in certain processes, for example, serialization or field mapping.
    /// </remarks>
    /// <seealso cref="SAPUtils.Models.UserTables.UserTableObjectModel" />
    /// <seealso cref="SAPUtils.__Internal.Models.UserTableMetadataCache" />
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreFieldAttribute : Attribute { }

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
        public static List<T> GetAll(IWhere where = null) {
            return GetAll<T>(where);
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
        public override bool Update(bool restore = false) {
            Log.Debug("Attempting to update {0} in table {1} with Code: {2}", GetType().Name, _userTableAttribute.Name, Code);
            try {
                Log.Trace("Retrieving SAP User Table `{0}`...", _userTableAttribute.Name);
                UserTable table = SapAddon.Instance().Company.UserTables.Item(_userTableAttribute.Name);
                RestoreOriginalCode();

                if (!table.GetByKey(Code)) {
                    throw new ItemDoNotExistException(GetType().Name, _userTableAttribute.Name, Code);
                }

                if (!restore || !(this is ISoftDeletable deletable)) return Save();
                bool? temp = OriginalActive;
                OriginalActive = deletable.Active;
                if (Save()) {
                    return true;
                }
                OriginalActive = temp;
                return false;
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
        public override bool Delete() {
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
                    if (OriginalActive.HasValue) {
                        bool temp = OriginalActive.Value;
                        softDeletable.Active = false;
                        OriginalActive = false;
                        if (Save()) {
                            result = 0;
                        }
                        else {
                            OriginalActive = !temp;
                        }
                    }
                    else {
                        softDeletable.Active = false;
                        if (Save()) {
                            result = 0;
                        }
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
                foreach ((PropertyInfo propertyInfo, IUserTableField userTableField) in UserTableMetadataCache.GetUserFields(GetType())) {
                    if (AuditableField.IsAuditableField(GetType(), propertyInfo)) continue;
                    Log.Trace("Validating {0}'s field {1} = {2}", GetType().Name, propertyInfo.Name, propertyInfo.GetValue(this));
                    if (userTableField.ValidateField(propertyInfo.GetValue(this))) continue;
                    Log.Debug("Invalid value for field {0}: {1}", propertyInfo.Name, propertyInfo.GetValue(this));
                    InvalidFieldEvent.Invoke(propertyInfo, userTableField);
                    return false;
                }
                UserTable table = SapAddon.Instance().Company.UserTables.Item(_userTableAttribute.Name);
                if (Code == null && OriginalCode == null) {
                    PrimaryKeyStrategy primaryKeyStrategy = _userTableAttribute.PrimaryKeyStrategy;
                    GenerateCode(primaryKeyStrategy, table);
                }
                Log.Debug("Saving {0} into table {1} with Code: {2}", GetType().Name, _userTableAttribute.Name, Code);
                bool exist = table.GetByKey(Code);

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

                    if (this is ISoftDeletable deletable && OriginalActive.HasValue) {
                        deletable.Active = OriginalActive.Value;
                    }
                }
                else {
                    Log.Trace("Inserting new {0} into table {1} with Code: {2}", GetType().Name, _userTableAttribute.Name, Code);
                    table.Code = Code;

                    if (this is IAuditableDate dateAudit) {
                        dateAudit.CreatedAt = DateTime.Now;
                        dateAudit.UpdatedAt = DateTime.Now;
                        OriginalCreatedAt = dateAudit.CreatedAt;
                    }

                    if (this is IAuditableUser userAudit) {
                        userAudit.CreatedBy = SapAddon.Instance().Company.UserSignature;
                        userAudit.UpdatedBy = SapAddon.Instance().Company.UserSignature;
                        OriginalCreatedBy = userAudit.CreatedBy;
                    }

                    if (this is ISoftDeletable deletableField) {
                        deletableField.Active = true;
                        OriginalActive = deletableField.Active;
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
                        case DateTimeFieldAttribute _:
                        {
                            DateTime? dateTime = (DateTime?)value;
                            table.UserFields.Fields.Item($"U_{fieldName}Date").Value = dateTime;
                            table.UserFields.Fields.Item($"U_{fieldName}Time").Value = dateTime;
                            break;
                        }
                        case DateFieldAttribute _:
                        case TimeFieldAttribute _:
                        {
                            DateTime dateTime = (DateTime?)value ?? DateTime.MinValue;
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
                    OriginalCode = Code;
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

        /// <inheritdoc />
        public override T1 GetPreviousRecord<T1>() {
            string previousCode = GetPreviousCode();
            if (previousCode == null) return null;
            return Get(previousCode, out T1 t) ? t : null;
        }
        /// <inheritdoc />
        public override T1 GetNextRecord<T1>() {
            string nextCode = GetNextCode();
            if (nextCode == null) return null;
            return Get(nextCode, out T1 t) ? t : null;
        }

        /// <inheritdoc />
        public override T1 GetFirstRecord<T1>() {
            string previousCode = GetFirstCode();
            if (previousCode == null) return null;
            return Get(previousCode, out T1 t) ? t : null;
        }
        /// <inheritdoc />
        public override T1 GetLastRecord<T1>() {
            string nextCode = GetLastCode();
            if (nextCode == null) return null;
            return Get(nextCode, out T1 t) ? t : null;
        }

        /// <summary>
        /// Retrieves the previous record based on the current context within the user table.
        /// </summary>
        /// <typeparam name="T">
        /// The type representing the structure of the user table, which must inherit from <see cref="UserTableObjectModel{T}"/>.
        /// </typeparam>
        /// <returns>
        /// Returns an instance of the previous record if found; otherwise, returns <c>null</c>.
        /// </returns>
        /// <remarks>
        /// The retrieval is based on the logical sequence of codes within the table. If no record exists, <c>null</c> is returned.
        /// </remarks>
        /// <seealso cref="UserTableObjectModel{T}"/>
        public T GetPreviousRecord() {
            return GetPreviousRecord<T>();
        }

        /// <summary>
        /// Retrieves the next record based on the current context within the user table.
        /// </summary>
        /// <typeparam name="T">
        /// The type representing the structure of the user table, which must inherit from <see cref="UserTableObjectModel{T}"/>.
        /// </typeparam>
        /// <returns>
        /// Returns an instance of the next record if found; otherwise, returns <c>null</c>.
        /// </returns>
        /// <remarks>
        /// The retrieval is based on the logical sequence of codes within the table. If no record exists, <c>null</c> is returned.
        /// </remarks>
        /// <seealso cref="UserTableObjectModel{T}"/>
        public T GetNextRecord() {
            return GetNextRecord<T>();
        }

        /// <summary>
        /// Retrieves the first record based on the current context within the user table.
        /// </summary>
        /// <typeparam name="T">
        /// The type representing the structure of the user table, which must inherit from <see cref="UserTableObjectModel{T}"/>.
        /// </typeparam>
        /// <returns>
        /// Returns an instance of the first record if found; otherwise, returns <c>null</c>.
        /// </returns>
        /// <remarks>
        /// The retrieval is based on the logical sequence of codes within the table. If no record exists, <c>null</c> is returned.
        /// </remarks>
        /// <seealso cref="UserTableObjectModel{T}"/>
        public T GetFirstRecord() {
            return GetFirstRecord<T>();
        }

        /// <summary>
        /// Retrieves the last record based on the current context within the user table.
        /// </summary>
        /// <typeparam name="T">
        /// The type representing the structure of the user table, which must inherit from <see cref="UserTableObjectModel{T}"/>.
        /// </typeparam>
        /// <returns>
        /// Returns an instance of the last record if found; otherwise, returns <c>null</c>.
        /// </returns>
        /// <remarks>
        /// The retrieval is based on the logical sequence of codes within the table. If no record exists, <c>null</c> is returned.
        /// </remarks>
        /// <seealso cref="UserTableObjectModel{T}"/>
        public T GetLastRecord() {
            return GetLastRecord<T>();
        }

        private void GenerateCode(PrimaryKeyStrategy primaryKeyStrategy, SAPbobsCOM.IUserTable table) {
            switch (primaryKeyStrategy) {
                case PrimaryKeyStrategy.Manual when string.IsNullOrEmpty(Code):
                    throw new CodeNotSetException(_userTableAttribute.Name);
                case PrimaryKeyStrategy.Manual when table.GetByKey(Code):
                    throw new ItemAlreadyExistException(_userTableAttribute.Name, Code);
                case PrimaryKeyStrategy.Manual:
                    break;
                case PrimaryKeyStrategy.Guid:
                    Code = Guid.NewGuid().ToString();
                    break;
                case PrimaryKeyStrategy.Serie:
                    using (Repository repository = (Repository)Repository.Get())
                        Code = repository.GetNextCodeUserTable(_userTableAttribute.Name).ToString();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(PrimaryKeyStrategy));
            }
            if (Name == null) {
                Name = Code;
            }
        }

        /// <inheritdoc />
        public override string GetNextAvailableCode() {
            switch (_userTableAttribute.PrimaryKeyStrategy) {
                case PrimaryKeyStrategy.Manual:
                    return "";
                case PrimaryKeyStrategy.Guid:
                    return Guid.NewGuid().ToString();
                case PrimaryKeyStrategy.Serie:
                    using (Repository repository = (Repository)Repository.Get())
                        return repository.GetNextCodeUserTable(_userTableAttribute.Name).ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(PrimaryKeyStrategy));
            }
        }

        private void RestoreOriginalCode() {
            if (Code == OriginalCode) return;
            Log.Debug("Reverting {0} Code to: {1}", GetType().Name, OriginalCode);
            Code = OriginalCode;
        }


        private string GetNextCode() {
            return GetAdjacentCode(next: true);
        }

        private string GetPreviousCode() {
            return GetAdjacentCode(next: false);
        }

        private string GetAdjacentCode(bool next) {
            string orderDir = next ? "ASC" : "DESC";
            string comparisonOp = next ? ">" : "<";
            string userTable = $"@{_userTableAttribute.Name}";
            Recordset rs;
            bool isHana = SapAddon.Instance().IsHana;

            // 1. Obtener longitud máxima (solo si es Serie)
            int maxLength = 0;
            if (_userTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie) {
                string lenQuery = isHana
                    ? $"SELECT MAX(LENGTH({Quote("Code")})) AS \"MaxLen\" FROM {Quote(userTable)}"
                    : $"SELECT MAX(LEN({Quote("Code")})) AS MaxLen FROM {Quote(userTable)}";

                rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(lenQuery);
                maxLength = rs.Fields.Item(0).Value != null ? (int)rs.Fields.Item(0).Value : 0;
            }

            // 2. Preparar expresiones
            string paddedCodeExpr = _userTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie
                ? isHana
                    ? $"LPAD({Quote("Code")}, {maxLength}, '0')"
                    : $"RIGHT(REPLICATE('0', {maxLength}) + {Quote("Code")}, {maxLength})"
                : Quote("Code");

            if (Code != null) {


                string paddedCurrent = Code == null
                    ? null
                    : _userTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie
                        ? Code.PadLeft(maxLength, '0')
                        : Code;

                string where = paddedCurrent == null
                    ? ""
                    : $"WHERE {paddedCodeExpr} {comparisonOp} '{paddedCurrent.Replace("'", "''")}'";

                // 3. Consulta principal
                string mainQuery = isHana
                    ? $@"
                    SELECT {Quote("Code")}
                    FROM {Quote(userTable)}
                    {where}
                    ORDER BY {paddedCodeExpr} {orderDir}
                    LIMIT 1"
                    : $@"
                    SELECT TOP 1 {Quote("Code")}
                    FROM {Quote(userTable)}
                    {where}
                    ORDER BY {paddedCodeExpr} {orderDir}";

                rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(mainQuery);
                if (!rs.EoF)
                    return rs.Fields.Item("Code").Value.ToString();
            }
            rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            // 4. Si no se encontró, consulta fallback
            string fallbackQuery = isHana
                ? $@"
                    SELECT {Quote("Code")}
                    FROM {Quote(userTable)}
                    ORDER BY {paddedCodeExpr} {orderDir}
                    LIMIT 1"
                : $@"
                SELECT TOP 1 {Quote("Code")}
                FROM {Quote(userTable)}
                ORDER BY {paddedCodeExpr} {orderDir}";

            rs.DoQuery(fallbackQuery);
            return rs.EoF ? null : rs.Fields.Item("Code").Value.ToString();

            string Quote(string s) => isHana ? $"\"{s}\"" : $"[{s}]";
        }

        private string GetFirstCode() {
            return GetEdgeCode(first: true);
        }

        private string GetLastCode() {
            return GetEdgeCode(first: false);
        }


        private string GetEdgeCode(bool first) {
            string orderDir = first ? "ASC" : "DESC";

            string userTable = $"@{_userTableAttribute.Name}";
            Recordset rs;
            bool isHana = SapAddon.Instance().IsHana;

            // Obtener longitud máxima si es Serie
            int maxLength = 0;
            if (_userTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie) {
                string lenQuery = isHana
                    ? $"SELECT MAX(LENGTH({Quote("Code")})) AS \"MaxLen\" FROM {Quote(userTable)}"
                    : $"SELECT MAX(LEN({Quote("Code")})) AS MaxLen FROM {Quote(userTable)}";

                rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(lenQuery);
                maxLength = rs.Fields.Item(0).Value != null ? (int)rs.Fields.Item(0).Value : 0;
            }

            // Expresión de ordenamiento
            string paddedCodeExpr = _userTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie
                ? isHana
                    ? $"LPAD({Quote("Code")}, {maxLength}, '0')"
                    : $"RIGHT(REPLICATE('0', {maxLength}) + {Quote("Code")}, {maxLength})"
                : Quote("Code");

            // Consulta SQL
            string query = isHana
                ? $@"
                    SELECT {Quote("Code")}
                    FROM {Quote(userTable)}
                    ORDER BY {paddedCodeExpr} {orderDir}
                    LIMIT 1"
                : $@"
                    SELECT TOP 1 {Quote("Code")}
                    FROM {Quote(userTable)}
                    ORDER BY {paddedCodeExpr} {orderDir}";

            rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
            rs.DoQuery(query);

            return rs.EoF ? null : rs.Fields.Item("Code").Value.ToString();

            string Quote(string s) => isHana ? $"\"{s}\"" : $"[{s}]";
        }
    }

}