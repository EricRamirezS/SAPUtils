using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Models;
using SAPUtils.__Internal.Utils;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Database;
using SAPUtils.Events;
using SAPUtils.Exceptions;
using SAPUtils.Query;
using SAPUtils.Utils;
using IUserTable = SAPUtils.__Internal.Attributes.UserTables.IUserTable;

namespace SAPUtils.Models.UserTables {
    /// <summary>
    /// Represents a generic base class for interacting with SAP B1 User Tables, providing CRUD operations and mapping logic.
    /// </summary>
    /// <typeparam name="T">
    /// The derived type that extends <see cref="UserTableObjectModel{T}"/>. This type must be decorated with 
    /// <see cref="Attributes.UserTables.UserTableAttribute"/> and define the structure of the user table.
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
        private static readonly Type CachedType = typeof(T);

        static UserTableObjectModel() {
            PropertyGetters = GetGetters(typeof(T));
            PropertySetters = GetSetters(typeof(T));
            UserTableAttribute = UserTableMetadataCache.GetUserTableAttribute(typeof(T));
            CachedUserFields = UserTableMetadataCache.GetUserFields(typeof(T));

            _fieldMeta = new List<FieldMetadata>();

            List<(PropertyInfo Property, IUserTableField Field)> userFields =
                UserTableMetadataCache.GetUserFields(typeof(T));

            foreach ((PropertyInfo prop, IUserTableField field) in userFields) {
                SapFieldKind kind;
                switch (field) {
                    case DateTimeFieldAttribute _:
                        kind = SapFieldKind.DateTime;
                        break;
                    case DateFieldAttribute _:
                        kind = SapFieldKind.Date;
                        break;
                    case TimeFieldAttribute _:
                        kind = SapFieldKind.Time;
                        break;
                    default:
                        kind = SapFieldKind.Normal;
                        break;
                }

                _fieldMeta.Add(new FieldMetadata {
                    Prop = prop,
                    Field = field,
                    SapName = string.IsNullOrWhiteSpace(field.Name) ? prop.Name : field.Name,
                    Kind = kind,
                    Getter = CreateGetter(prop),
                    Setter = CreateSetter(prop)
                });
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="UserTableObjectModel{T}"/>, validating the presence of <see cref="Attributes.UserTables.UserTableAttribute"/>.
        /// </summary>
        /// <exception cref="UserTableAttributeNotFound">
        /// Thrown when the <typeparamref name="T"/> type does not define a valid <see cref="Attributes.UserTables.UserTableAttribute"/>.
        /// </exception>
        protected UserTableObjectModel() {
            Log.Trace("Initializing UserTableObjectModel...");
            try {
                if (UserTableAttribute == null)
                    throw new UserTableAttributeNotFound(typeof(T).Name);
            }
            catch (Exception ex) {
                Log.Error(ex);
                throw new UserTableAttributeNotFound(typeof(T).Name);
            }

            Log.Debug("UserTableAttribute initialized: {0}", UserTableAttribute.Name);
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
            Log.Debug("Attempting to add {0} into table {1} with Code: {2}", GetType().Name, UserTableAttribute.Name,
                Code);
            try {
                SAPbobsCOM.IUserTable table = SapAddon.Instance().Company.UserTables.Item(UserTableAttribute.Name);
                PrimaryKeyStrategy primaryKeyStrategy = UserTableAttribute.PrimaryKeyStrategy;
                GenerateCode(primaryKeyStrategy, table);

                if (Save()) return true;

                if (UserTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual) return false;
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
            Log.Debug("Attempting to update {0} in table {1} with Code: {2}", GetType().Name, UserTableAttribute.Name,
                Code);
            try {
                Log.Trace("Retrieving SAP User Table `{0}`...", UserTableAttribute.Name);
                UserTable table = SapAddon.Instance().Company.UserTables.Item(UserTableAttribute.Name);
                RestoreOriginalCode();

                if (!table.GetByKey(Code)) {
                    throw new ItemDoNotExistException(GetType().Name, UserTableAttribute.Name, Code);
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
            Log.Debug("Attempting to delete {0} from table {1} with Code: {2}", GetType().Name, UserTableAttribute.Name,
                Code);
            try {
                UserTable table = SapAddon.Instance().Company.UserTables.Item(UserTableAttribute.Name);
                RestoreOriginalCode();

                if (!table.GetByKey(Code)) {
                    Log.Info("{0} from table {1} with Code {2} does not exist.", GetType().Name,
                        UserTableAttribute.Name, Code);
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
                    Log.Info("{0} from table {1} with Code {3} deleted successfully.", GetType().Name,
                        UserTableAttribute.Name, Code);
                    return true;
                }

                SapAddon.Instance().Company.GetLastError(out int errCode, out string errMsg);
                Log.Error(
                    "Failed to delete {0} from table {1} with Code {2}. SAP Error Code: {3}. SAP Error Message: {4}",
                    GetType().Name,
                    UserTableAttribute.Name,
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
                Company company = SapAddon.Instance().Company;
                DateTime nowDt = DateTime.Now;
                int userSign = company.UserSignature;

                foreach (FieldMetadata meta in _fieldMeta) {
                    if (AuditableField.IsAuditableField(CachedType, meta.Prop)) continue;
                    object value = meta.Getter((T)this);
                    Log.Trace(() => $"Validating {CachedType.Name}'s field {meta.Prop.Name} = {value}");
                    if (meta.Field.ValidateField(value)) continue;
                    Log.Debug(() => $"Invalid value for field {meta.Prop.Name}: {value}");
                    InvalidFieldEvent.Invoke(meta.Prop, meta.Field);
                    return false;
                }

                UserTable table = company.UserTables.Item(UserTableAttribute.Name);
                Fields fields = table.UserFields.Fields;

                if (Code == null && OriginalCode == null) GenerateCode(UserTableAttribute.PrimaryKeyStrategy, table);
                else if (OriginalCode != null) Code = OriginalCode;

                Log.Debug("Saving {0} into table {1} with Code: {2}", CachedType.Name, UserTableAttribute.Name, Code);
                bool exist = table.GetByKey(Code);

                if (exist)
                    Log.Trace("Updating existing {0} in table {1} with Code: {2}", CachedType.Name,
                        UserTableAttribute.Name, Code);
                else {
                    table.Code = Code;
                    Log.Trace("Inserting new {0} into table {1} with Code: {2}", CachedType.Name,
                        UserTableAttribute.Name, Code);
                }

                ApplyAuditAndSoftDelete(exist, nowDt, userSign);

                table.Name = Name;

                foreach (FieldMetadata meta in _fieldMeta) {
                    object value = meta.Getter((T)this);

                    if (!exist && value == null && meta.Field.DefaultValue != null) {
                        value = meta.Field.DefaultValue;
                        meta.Setter((T)this, value);
                    }

                    switch (meta.Kind) {
                        case SapFieldKind.DateTime:
                            fields.Item($"U_{meta.SapName}Date").Value = value;
                            fields.Item($"U_{meta.SapName}Time").Value = value;
                            break;
                        case SapFieldKind.Date:
                        case SapFieldKind.Time:
                            fields.Item($"U_{meta.SapName}").Value = value ?? DateTime.MinValue;
                            break;
                        case SapFieldKind.Normal:
                        default:
                            try {
                                fields.Item($"U_{meta.SapName}").Value = meta.Field.ToSapData(value);
                            }
                            catch (Exception ex) {
                                Log.Warning("Failed to assign value to field {0}: {1}", meta.SapName, ex.Message);
                                throw;
                            }

                            break;
                    }

                    Log.Trace(() => $"Processed field {meta.Prop.Name} = {value}");
                }

                int result = exist ? table.Update() : table.Add();

                if (result == 0) {
                    OriginalCode = Code;
                    Log.Info("{0} from table {1} with Code {2} saved successfully.", CachedType.Name,
                        UserTableAttribute.Name, Code);
                    Task.Run(() => ClearCache<T>(OriginalCode));
                    return true;
                }

                company.GetLastError(out int errCode, out string errMsg);
                Log.Error(
                    "Failed to save {0} in table {1} with Code {2}. SAP Error Code: {3}. SAP Error Message: {4}",
                    CachedType.Name, UserTableAttribute.Name, Code, errCode, errMsg);
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
                    throw new CodeNotSetException(UserTableAttribute.Name);
                case PrimaryKeyStrategy.Manual when table.GetByKey(Code):
                    throw new ItemAlreadyExistException(UserTableAttribute.Name, Code);
                case PrimaryKeyStrategy.Manual:
                    break;
                case PrimaryKeyStrategy.Guid:
                    Code = Guid.NewGuid().ToString();
                    break;
                case PrimaryKeyStrategy.Serie:
                    using (Repository repository = (Repository)Repository.Get())
                        Code = repository.GetNextCodeUserTable(UserTableAttribute.Name).ToString();
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
            switch (UserTableAttribute.PrimaryKeyStrategy) {
                case PrimaryKeyStrategy.Manual:
                    return "";
                case PrimaryKeyStrategy.Guid:
                    return Guid.NewGuid().ToString();
                case PrimaryKeyStrategy.Serie:
                    using (Repository repository = (Repository)Repository.Get())
                        return repository.GetNextCodeUserTable(UserTableAttribute.Name).ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(PrimaryKeyStrategy));
            }
        }

        /// <summary>
        /// Generates a new code based on the primary key strategy defined in the user table attribute.
        /// </summary>
        /// <returns>
        /// A new code as a string based on the selected primary key strategy. It could be an empty string (for manual strategy),
        /// a GUID (for Guid strategy), or the next code in a sequence (for Serie strategy).
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the primary key strategy is not recognized or supported.
        /// </exception>
        /// <seealso cref="PrimaryKeyStrategy" />
        public static string GetNewCode() {
            switch (UserTableAttribute.PrimaryKeyStrategy) {
                case PrimaryKeyStrategy.Manual:
                    return "";
                case PrimaryKeyStrategy.Guid:
                    return Guid.NewGuid().ToString();
                case PrimaryKeyStrategy.Serie:
                    using (Repository repository = (Repository)Repository.Get())
                        return repository.GetNextCodeUserTable(UserTableAttribute.Name).ToString();
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
            string userTable = $"@{UserTableAttribute.Name}";
            Recordset rs;
            bool isHana = SapAddon.Instance().IsHana;

            // 1. Obtener longitud máxima (solo si es Serie)
            int maxLength = 0;
            if (UserTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie) {
                string lenQuery = isHana
                    ? $"SELECT MAX(LENGTH({Quote("Code")})) AS \"MaxLen\" FROM {Quote(userTable)}"
                    : $"SELECT MAX(LEN({Quote("Code")})) AS MaxLen FROM {Quote(userTable)}";

                rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(lenQuery);
                maxLength = rs.Fields.Item(0).Value != null ? (int)rs.Fields.Item(0).Value : 0;
            }

            // 2. Preparar expresiones
            string paddedCodeExpr = UserTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie
                ? isHana
                    ? $"LPAD({Quote("Code")}, {maxLength}, '0')"
                    : $"RIGHT(REPLICATE('0', {maxLength}) + {Quote("Code")}, {maxLength})"
                : Quote("Code");

            if (Code != null) {
                string paddedCurrent = Code == null
                    ? null
                    : UserTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie
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

            string userTable = $"@{UserTableAttribute.Name}";
            Recordset rs;
            bool isHana = SapAddon.Instance().IsHana;

            // Obtener longitud máxima si es Serie
            int maxLength = 0;
            if (UserTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie) {
                string lenQuery = isHana
                    ? $"SELECT MAX(LENGTH({Quote("Code")})) AS \"MaxLen\" FROM {Quote(userTable)}"
                    : $"SELECT MAX(LEN({Quote("Code")})) AS MaxLen FROM {Quote(userTable)}";

                rs = (Recordset)SapAddon.Instance().Company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery(lenQuery);
                maxLength = rs.Fields.Item(0).Value != null ? (int)rs.Fields.Item(0).Value : 0;
            }

            // Expresión de ordenamiento
            string paddedCodeExpr = UserTableAttribute.PrimaryKeyStrategy == PrimaryKeyStrategy.Serie
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

        private void ApplyAuditAndSoftDelete(bool exist, DateTime nowDt, int userSign) {
            if (this is IAuditableDate dateAudit) {
                dateAudit.UpdatedAt = nowDt;
                dateAudit.CreatedAt = !exist ? nowDt : OriginalCreatedAt;
            }

            if (this is IAuditableUser userAudit) {
                userAudit.UpdatedBy = userSign;
                userAudit.CreatedBy = exist ? OriginalCreatedBy : userSign;
            }

            if (exist) {
                if (!(this is ISoftDeletable deletable) || !OriginalActive.HasValue) return;
                deletable.Active = OriginalActive.Value;
            }
            else {
                if (!(this is ISoftDeletable deletableField)) return;
                deletableField.Active = true;
                OriginalActive = deletableField.Active;
            }
        }

        private static Func<T, object> CreateGetter(PropertyInfo prop) {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T), "instance");
            MemberExpression propertyAccess =
                Expression.Property(Expression.Convert(instanceParam, prop.DeclaringType), prop);
            UnaryExpression convertToObject = Expression.Convert(propertyAccess, typeof(object));
            return Expression.Lambda<Func<T, object>>(convertToObject, instanceParam).Compile();
        }

        private static Action<T, object> CreateSetter(PropertyInfo prop) {
            ParameterExpression instanceParam = Expression.Parameter(typeof(T), "instance");
            ParameterExpression valueParam = Expression.Parameter(typeof(object), "value");
            UnaryExpression convertInstance = Expression.Convert(instanceParam, prop.DeclaringType);
            UnaryExpression convertValue = Expression.Convert(valueParam, prop.PropertyType);
            MemberExpression propertyAccess = Expression.Property(convertInstance, prop);
            BinaryExpression assign = Expression.Assign(propertyAccess, convertValue);
            return Expression.Lambda<Action<T, object>>(assign, instanceParam, valueParam).Compile();
        }

        private class FieldMetadata {
            public IUserTableField Field;
            public Func<T, object> Getter;
            public SapFieldKind Kind;
            public PropertyInfo Prop;
            public string SapName;
            public Action<T, object> Setter;
        }

        private enum SapFieldKind {
            Normal,
            DateTime,
            Date,
            Time
        }

        // ReSharper disable StaticMemberInGenericType
        private static readonly IUserTable UserTableAttribute;
        private static readonly List<(PropertyInfo Property, IUserTableField Field)> CachedUserFields;
        private static readonly Dictionary<PropertyInfo, Func<object, object>> PropertyGetters;
        private static readonly Dictionary<PropertyInfo, Action<object, object>> PropertySetters;

        private static readonly List<FieldMetadata> _fieldMeta;
        // ReSharper restore StaticMemberInGenericType
    }
}