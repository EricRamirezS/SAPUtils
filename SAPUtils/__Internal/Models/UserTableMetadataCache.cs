using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Models.UserTables;

namespace SAPUtils.__Internal.Models {
    internal static class UserTableMetadataCache {
        private static readonly ConcurrentDictionary<Type, List<(PropertyInfo Property, IUserTableField Field)>> PropertyFieldCache =
            new ConcurrentDictionary<Type, List<(PropertyInfo Property, IUserTableField Field)>>();

        private static readonly ConcurrentDictionary<Type, UserTableAttribute> UserTableAttributeCache =
            new ConcurrentDictionary<Type, UserTableAttribute>();

        public static (PropertyInfo Property, IUserTableField Field) GetUserField(Type type, string name) {
            return PropertyFieldCache.GetOrAdd(type, t =>
            {
                List<(PropertyInfo Property, IUserTableField Field)> props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.Name != "Code" && p.Name != "Name")
                    .Where(p => !p.IsDefined(typeof(IgnoreFieldAttribute), true))
                    .Select(p =>
                    {
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
            return PropertyFieldCache.GetOrAdd(type, t =>
            {
                List<(PropertyInfo Property, IUserTableField Field)> props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.Name != "Code" && p.Name != "Name")
                    .Where(p => !p.IsDefined(typeof(IgnoreFieldAttribute), true))
                    .Select(p =>
                    {
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