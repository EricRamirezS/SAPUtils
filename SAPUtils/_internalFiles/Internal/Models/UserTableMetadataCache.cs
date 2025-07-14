using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Internal.Attributes.UserTables;

namespace SAPUtils.Internal.Models {
    internal static class UserTableMetadataCache
    {
        private static readonly ConcurrentDictionary<Type, List<(PropertyInfo Property, IUserTableField Field)>> PropertyFieldCache = new ConcurrentDictionary<Type, List<(PropertyInfo Property, IUserTableField Field)>>();
        private static readonly ConcurrentDictionary<Type, UserTableAttribute> UserTableAttributeCache = new ConcurrentDictionary<Type, UserTableAttribute>();

        public static List<(PropertyInfo Property, IUserTableField Field)> GetUserFields(Type type)
        {
            return PropertyFieldCache.GetOrAdd(type, t =>
            {
                List<(PropertyInfo Property, IUserTableField Field)> props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.Name != "Code" && p.Name != "Name")
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

        public static UserTableAttribute GetUserTableAttribute(Type type)
        {
            return UserTableAttributeCache.GetOrAdd(type, t =>
                t.GetCustomAttributes(typeof(UserTableAttribute), true)
                    .Cast<UserTableAttribute>()
                    .FirstOrDefault()
            );
        }
    }
}