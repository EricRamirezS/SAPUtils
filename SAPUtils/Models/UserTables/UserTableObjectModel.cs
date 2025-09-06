using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Models;
using SAPUtils.__Internal.Query;
using SAPUtils.__Internal.Utils;
using SAPUtils.Attributes.UserTables;
using SAPUtils.I18N;
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
        /// This cache uses a <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>
        /// to ensure thread-safety when accessing or modifying the stored instances.
        /// </remarks>
        /// <seealso cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>
        private static readonly ConcurrentDictionary<(Type, string), UserTableObjectModel> Cache =
            new ConcurrentDictionary<(Type, string), UserTableObjectModel>();

        private static readonly ConcurrentDictionary<Type, Dictionary<PropertyInfo, Func<object, object>>>
            GettersCache = new ConcurrentDictionary<Type, Dictionary<PropertyInfo, Func<object, object>>>();

        private static readonly ConcurrentDictionary<Type, Dictionary<PropertyInfo, Action<object, object>>>
            SettersCache = new ConcurrentDictionary<Type, Dictionary<PropertyInfo, Action<object, object>>>();

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
            UserTableObjectModel clone = (UserTableObjectModel)MemberwiseClone();
            clone.Code = null;
            clone.OriginalCode = null;
            clone.OriginalActive = null;
            clone.OriginalCreatedAt = default;
            clone.OriginalCreatedBy = 0;
            return clone;
        }

        internal static Dictionary<PropertyInfo, Func<object, object>> GetGetters(Type type) {
            return GettersCache.GetOrAdd(type, BuildGetters);
        }

        internal static Dictionary<PropertyInfo, Action<object, object>> GetSetters(Type type) {
            return SettersCache.GetOrAdd(type, BuildSetters);
        }

        [Localizable(false)]
        private static Dictionary<PropertyInfo, Func<object, object>> BuildGetters(Type type) {
            Dictionary<PropertyInfo, Func<object, object>> dict = new Dictionary<PropertyInfo, Func<object, object>>();

            foreach ((PropertyInfo prop, IUserTableField _) in UserTableMetadataCache.GetUserFields(type)) {
                ParameterExpression instanceParam = Expression.Parameter(typeof(object), "instance");
                UnaryExpression typedInstance = Expression.Convert(instanceParam, type);
                MemberExpression propertyAccess = Expression.Property(typedInstance, prop);
                UnaryExpression convert = Expression.Convert(propertyAccess, typeof(object));

                Func<object, object> lambda = Expression.Lambda<Func<object, object>>(convert, instanceParam).Compile();
                dict[prop] = lambda;
            }

            return dict;
        }

        [Localizable(false)]
        private static Dictionary<PropertyInfo, Action<object, object>> BuildSetters(Type type) {
            Dictionary<PropertyInfo, Action<object, object>> dict =
                new Dictionary<PropertyInfo, Action<object, object>>();

            foreach ((PropertyInfo prop, IUserTableField _) in UserTableMetadataCache.GetUserFields(type)) {
                if (!prop.CanWrite) continue;

                ParameterExpression instanceParam = Expression.Parameter(typeof(object), "instance");
                ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");

                UnaryExpression typedInstance = Expression.Convert(instanceParam, type);
                UnaryExpression convertedValue = Expression.Convert(valueParam, prop.PropertyType);
                BinaryExpression assign = Expression.Assign(Expression.Property(typedInstance, prop), convertedValue);

                Action<object, object> lambda =
                    Expression.Lambda<Action<object, object>>(assign, instanceParam, valueParam).Compile();
                dict[prop] = lambda;
            }

            return dict;
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
        [Localizable(false)]
        public static List<T> GetAll<T>(IWhere where = null) where T : IUserTableObjectModel, new() {
            List<T> data = new List<T>();
            Type type = typeof(T);
            ILogger log = Logger.Instance;
            where = where ?? Where.Builder().Build();
            log.Debug(Texts.UserTableObjectModel_GetAll_Fetching_All__0_, type.FullName);

            IUserTable userTable = UserTableMetadataCache.GetUserTableAttribute(typeof(T));
            if (userTable == null) {
                log.Error(Texts.UserTableObjectModel_GetAll_UserTableAttribute_not_found_in__0_, type.FullName);
                return data;
            }

            log.Debug(Texts.UserTableObjectModel_GetAll_Fetching_all__0__from_table__1_, type.FullName, userTable.Name);

            string tableName = userTable.Name;
            Recordset rs = null;
            try {
                rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                tableName = SapAddon.Instance().IsHana ? $"\"@{tableName}\"" : $"[@{tableName}]";
                string whereString = new SqlWhereBuilder(where).Build();
                string query = $"SELECT T0.* FROM {tableName} T0 {whereString}";
                log.Trace(Texts.UserTableObjectModel_GetAll_Executing_query___0_, query);
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

        [Localizable(false)]
        private static void PopulateFields<T>(Fields fields, Type type, string tableName, ref T entity)
            where T : IUserTableObjectModel {
            ILogger log = Logger.Instance;
            Dictionary<PropertyInfo, Action<object, object>> setters = GetSetters(entity.GetType());
            foreach ((PropertyInfo propertyInfo, IUserTableField userTableField) in
                     UserTableMetadataCache.GetUserFields(type)) {
                string fieldName = string.IsNullOrWhiteSpace(userTableField.Name)
                    ? propertyInfo.Name
                    : userTableField.Name;
                log.Trace(Texts.UserTableObjectModel_PopulateFields_Processing_field___0___1_, tableName, fieldName);
                if (userTableField is DateTimeFieldAttribute dtUserTableField) {
                    Field date = fields.Item($"U_{fieldName}Date");
                    Field time = fields.Item($"U_{fieldName}Time");
                    if (date.IsNull() == BoYesNoEnum.tYES && time.IsNull() == BoYesNoEnum.tYES) continue;

                    DateTime d = (DateTime)date.Value;
                    DateTime t = dtUserTableField.ParseTimeValue(time.Value);
                    DateTime dt = new DateTime(d.Year, d.Month, d.Day, t.Hour, t.Minute, t.Second, d.Kind);
                    setters[propertyInfo](entity, dt);
                }
                else {
                    Field field = fields.Item($"U_{fieldName}");
                    log.Trace(Texts.UserTableObjectModel_PopulateFields_, tableName, fieldName, field.Value);
                    if (field.IsNull() == BoYesNoEnum.tNO) {
                        setters[propertyInfo](entity, userTableField.ParseValue(field.Value));
                    }
                }
            }

            if (!(entity is UserTableObjectModel itemModel)) return;

            itemModel.OriginalCode = entity.Code;
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
            log.Debug(Texts.UserTableObjectModel_Get_Fetching__0__with_Code___1_, type.FullName, code);
            item = default;
            IUserTable userTable = UserTableMetadataCache.GetUserTableAttribute(typeof(T));
            if (userTable == null) {
                log.Error(Texts.UserTableObjectModel_Get_UserTableAttribute_not_found_in__0_, type.FullName);
                return false;
            }

            log.Debug(Texts.UserTableObjectModel_Get_Fetching__0__from_table__1__with_Code___2_, type.FullName, userTable.Name, code);

            string tableName = userTable.Name;
            UserTable table = SapAddon.Instance().Company.UserTables.Item(tableName);
            if (!table.GetByKey(code)) {
                log.Info(Texts.UserTableObjectModel_Get__0__from_table__1__not_found_with_Code___2_, type.FullName, tableName, code);
                return false;
            }

            log.Debug(Texts.UserTableObjectModel_Get__0__from_table__1__found_in__0___populating_properties___, type.FullName, tableName, code);
            item = new T {
                Code = table.Code,
                Name = table.Name,
            };

            PopulateFields(table.UserFields.Fields, type, tableName, ref item);

            log.Trace(Texts.UserTableObjectModel_Get__0__successfully_populated_for_Code___1_, type.FullName, code);
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
        protected static T GetCached<T>(string code) where T : UserTableObjectModel, new() {
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
}