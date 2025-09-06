using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.Attributes.UserTables;
using SAPUtils.I18N;
using SAPUtils.Models.UserTables;

namespace SAPUtils.__Internal.Models {
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal static class UserTableMetadataCache {
        private static readonly ConcurrentDictionary<Type, List<(PropertyInfo Property, IUserTableField Field)>>
            PropertyFieldCache =
                new ConcurrentDictionary<Type, List<(PropertyInfo Property, IUserTableField Field)>>();

        private static readonly ConcurrentDictionary<Type, UserTableAttribute> UserTableAttributeCache =
            new ConcurrentDictionary<Type, UserTableAttribute>();

        private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> PropertyInfoCache =
            new ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>>();

        private static readonly ConcurrentDictionary<Type, MethodInfo> GetAllMethodCache =
            new ConcurrentDictionary<Type, MethodInfo>();

        public static MethodInfo GetAllMethodInfo<T>() {
            return GetAllMethodInfo(typeof(T));
        }

        public static MethodInfo GetAllMethodInfo(Type type) {
            if (type == null) throw new ArgumentNullException(nameof(type));

            return GetAllMethodCache.GetOrAdd(type, t => {
                MethodInfo method = typeof(UserTableObjectModel)
                    .GetMethod("GetAll", BindingFlags.Public | BindingFlags.Static);

                return method == null ? throw new InvalidOperationException(Texts.UserTableMetadataCache_GetAllMethodInfo_UserTableObjectModel_GetAll_Method_not_found) : method.MakeGenericMethod(t);
            });
        }

        public static PropertyInfo GetUserFieldPropertyInfo(Type type, string fieldName) {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            Dictionary<string, PropertyInfo> properties = PropertyInfoCache.GetOrAdd(type, t =>
                t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase));

            properties.TryGetValue(fieldName, out PropertyInfo prop);
            return prop;
        }

        public static (PropertyInfo Property, IUserTableField Field) GetUserField(Type type, string name) {
            return PropertyFieldCache.GetOrAdd(type, t => {
                List<(PropertyInfo Property, IUserTableField Field)> props = t
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.Name != "Code" && p.Name != "Name")
                    .Where(p => !p.IsDefined(typeof(IgnoreFieldAttribute), true))
                    .Select(p => {
                        IUserTableField field = AuditableField.IsAuditableField(t, p)
                            ? AuditableField.GetUserTableField(t, p)
                            : p.GetCustomAttributes(typeof(IUserTableField), true)
                                .Cast<IUserTableField>()
                                .FirstOrDefault();

                        if (field == null) return default;

                        field.Name = string.IsNullOrEmpty(field.Name) ? p.Name : field.Name;
                        field.Description = string.IsNullOrEmpty(field.Description) ? field.Name : field.Description;
                        return (Property: p, Field: field);
                    })
                    .Where(p => p != default)
                    .ToList();

                return props;
            }).FirstOrDefault(p => p.Property.Name == name);
        }

        public static List<(PropertyInfo Property, IUserTableField Field)> GetUserFields(Type type) {
            return PropertyFieldCache.GetOrAdd(type, t => {
                List<(PropertyInfo Property, IUserTableField Field)> props = t
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.Name != "Code" && p.Name != "Name")
                    .Where(p => !p.IsDefined(typeof(IgnoreFieldAttribute), true))
                    .Select(p => {
                        IUserTableField field = AuditableField.IsAuditableField(t, p)
                            ? AuditableField.GetUserTableField(t, p)
                            : p.GetCustomAttributes(typeof(IUserTableField), true)
                                .Cast<IUserTableField>()
                                .FirstOrDefault();

                        return field != null ? (Property: p, Field: field) : default;
                    })
                    .Where(p => p != default)
                    .ToList();

                return props;
            });
        }

        public static IUserTable GetUserTableAttribute(Type type) {
            return UserTableAttributeCache.GetOrAdd(type, t =>
                t.GetCustomAttributes(typeof(UserTableAttribute), true)
                    .Cast<UserTableAttribute>()
                    .FirstOrDefault()
            );
        }

        public static IUserTable GetUserTableAttribute(string tableName) {
            return UserTableAttributeCache.FirstOrDefault(e => e.Value.Name == tableName).Value;
        }

        public static Type GetTableType(string tableName) {
            return UserTableAttributeCache.FirstOrDefault(e => e.Value.Name == tableName).Key;
        }
    }
}