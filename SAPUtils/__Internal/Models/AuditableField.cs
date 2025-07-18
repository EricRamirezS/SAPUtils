using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Models.UserTables;

namespace SAPUtils.__Internal.Models {
    internal static class AuditableField {

        private static readonly Type[] AuditableInterfaces = {
            typeof(ISoftDeletable),
            typeof(IAuditableDate),
            typeof(IAuditableUser),
        };

        private static readonly Dictionary<string, Type> AuditableFieldSources =
            AuditableInterfaces
                .SelectMany(t => t.GetProperties().Select(p => new { p.Name, DeclaringInterface = t }))
                .GroupBy(x => x.Name)
                .ToDictionary(g => g.Key, g => g.First().DeclaringInterface);

        internal static bool IsAuditableField(Type table, PropertyInfo propertyInfo) {
            return AuditableFieldSources.TryGetValue(propertyInfo.Name, out Type declaringInterface) &&
                   declaringInterface.IsAssignableFrom(table);

        }
        internal static IUserTableField GetUserTableField(Type table, PropertyInfo propertyInfo) {
            bool Implements<TInterface>() => typeof(TInterface).IsAssignableFrom(table);

            switch (propertyInfo.Name) {
                case "Active" when Implements<ISoftDeletable>():
                    return new BooleanFieldAttribute {
                        Name = propertyInfo.Name,
                        DefaultValue = true,
                        Required = true,
                        ValidValues = new List<IUserFieldValidValue> {
                            new UserFieldValidValue("Y", "Activo"),
                            new UserFieldValidValue("N", "Inactivo"),
                        },
                        Description = "Activo",
                    };
                case "CreatedAt" when Implements<IAuditableDate>():
                    return new DateTimeFieldAttribute {
                        Name = propertyInfo.Name,
                        DefaultValue = DateTime.Now,
                        Required = true,
                        DateDescription = "Fecha de creación",
                        TimeDescription = "Hora de creación",
                    };
                case "UpdatedAt" when Implements<IAuditableDate>():
                    return new DateTimeFieldAttribute {
                        Name = propertyInfo.Name,
                        DefaultValue = DateTime.Now,
                        Required = true,
                        DateDescription = "Fecha de actualización",
                        TimeDescription = "Hora de actualización",
                    };
                case "CreatedBy" when Implements<IAuditableUser>():
                    return new NumericFieldAttribute {
                        Name = propertyInfo.Name,
                        Description = "Creado por",
                        Required = true,
                        LinkedSystemObject = UDFLinkedSystemObjectTypesEnum.ulUsers,
                    };
                case "UpdatedBy" when Implements<IAuditableUser>():
                    return new NumericFieldAttribute {
                        Name = propertyInfo.Name,
                        Description = "Actualizado por",
                        Required = true,
                        LinkedSystemObject = UDFLinkedSystemObjectTypesEnum.ulUsers,
                    };
                default:
                    throw new NotSupportedException(propertyInfo.Name + " is not supported in IAuditable");
            }
        }
    }
}