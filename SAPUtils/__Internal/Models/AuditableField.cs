using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SAPbobsCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.Attributes.UserTables;
using SAPUtils.I18N;
using SAPUtils.Models.UserTables;
using IValidValue = SAPbouiCOM.IValidValue;

namespace SAPUtils.__Internal.Models {
    internal static class AuditableField {

        private static readonly Type[] AuditableInterfaces = {
            typeof(ISoftDeletable),
            typeof(IAuditableDate),
            typeof(IAuditableUser),
        };

        private static readonly Dictionary<string, Type> AuditableFieldSources =
            AuditableInterfaces
                .SelectMany(t => t.GetProperties().Select(p => new {
                    p.Name,
                    DeclaringInterface = t,
                }))
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
                        Mandatory = true,
                        ValidValues = new List<IValidValue> {
                            new UserFieldValidValue("Y", Texts.AuditableField_GetUserTableField_Active),
                            new UserFieldValidValue("N", Texts.AuditableField_GetUserTableField_Inactive),
                        },
                        Description = Texts.AuditableField_GetUserTableField_Active,
                    };
                case "CreatedAt" when Implements<IAuditableDate>():
                    return new DateTimeFieldAttribute {
                        Name = propertyInfo.Name,
                        DefaultValue = DateTime.Now,
                        Mandatory = true,
                        DateDescription = Texts.AuditableField_GetUserTableField_Created_at_date,
                        TimeDescription = Texts.AuditableField_GetUserTableField_Created_at_time,
                    };
                case "UpdatedAt" when Implements<IAuditableDate>():
                    return new DateTimeFieldAttribute {
                        Name = propertyInfo.Name,
                        DefaultValue = DateTime.Now,
                        Mandatory = true,
                        DateDescription = Texts.AuditableField_GetUserTableField_Updated_at_date,
                        TimeDescription = Texts.AuditableField_GetUserTableField_Updated_at_time,
                    };
                case "CreatedBy" when Implements<IAuditableUser>():
                    return new NumericFieldAttribute {
                        Name = propertyInfo.Name,
                        Description = Texts.AuditableField_GetUserTableField_Created_by,
                        Mandatory = true,
                        LinkedSystemObject = UDFLinkedSystemObjectTypesEnum.ulUsers,
                    };
                case "UpdatedBy" when Implements<IAuditableUser>():
                    return new NumericFieldAttribute {
                        Name = propertyInfo.Name,
                        Description = Texts.AuditableField_GetUserTableField_Updated_by,
                        Mandatory = true,
                        LinkedSystemObject = UDFLinkedSystemObjectTypesEnum.ulUsers,
                    };
                default:
                    throw new NotSupportedException(string.Format(Texts.AuditableField_GetUserTableField__0__is_not_supported_in_IAuditable, propertyInfo.Name));
            }
        }
    }
}