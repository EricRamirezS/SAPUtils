using System.Linq;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPUtils.Attributes.UserTables;
using SAPUtils.Internal.Attributes.UserTables;
using SAPUtils.Models.UserTables;
using ChooseFromList = SAPbouiCOM.ChooseFromList;

namespace SAPUtils.Utils {
    public static class FormUtils {
        public static bool ExistForm(string formId, SAPbouiCOM.Application app) {
            try {
                return app.Forms.Cast<IForm>().FirstOrDefault(x => x.UniqueID == formId) != null;
            }
            catch {
                return false;
            }
        }

        public static BoFieldsType GetFieldType(BoFieldTypes fieldType, BoFldSubTypes subType) {
            switch (fieldType) {
                case BoFieldTypes.db_Alpha:
                    return BoFieldsType.ft_AlphaNumeric;
                case BoFieldTypes.db_Memo:
                    return BoFieldsType.ft_Text;
                case BoFieldTypes.db_Date:
                    return subType == BoFldSubTypes.st_Time ? BoFieldsType.ft_ShortNumber : BoFieldsType.ft_Date;
                case BoFieldTypes.db_Numeric:
                    return BoFieldsType.ft_Integer;
                case BoFieldTypes.db_Float:
                    switch (subType) {
                        case BoFldSubTypes.st_Quantity:
                            return BoFieldsType.ft_Quantity;
                        case BoFldSubTypes.st_Price:
                            return BoFieldsType.ft_Price;
                        case BoFldSubTypes.st_Rate:
                            return BoFieldsType.ft_Rate;
                        case BoFldSubTypes.st_Sum:
                            return BoFieldsType.ft_Sum;
                        case BoFldSubTypes.st_Percentage:
                            return BoFieldsType.ft_Percent;
                        case BoFldSubTypes.st_Measurement:
                            return BoFieldsType.ft_Measure;
                        default:
                            return BoFieldsType.ft_Float;
                    }
                default:
                    return BoFieldsType.ft_AlphaNumeric;
            }
        }

        internal static Column AddColumnFromUserTableField(this Matrix matrix, 
            string uid, 
            IUserTableField field, 
            IForm form,
            Application application) {
            Column column;

            bool isBoolean = field.Type == typeof(bool) ||
                             (field.ValidValues?.Count == 2 &&
                              field.ValidValues.Any(v => v.Value == "Y") &&
                              field.ValidValues.Any(v => v.Value == "N"));

            bool isCombo = !isBoolean && field.ValidValues != null && field.ValidValues.Count > 0;

            bool isLinked = field.LinkedSystemObject.HasValue ||
                            !string.IsNullOrWhiteSpace(field.LinkedTable) ||
                            !string.IsNullOrWhiteSpace(field.LinkedUdo);

            if (isBoolean) {
                column = matrix.Columns.Add(uid, BoFormItemTypes.it_CHECK_BOX);
            }
            else if (isCombo) {
                column = matrix.Columns.Add(uid, BoFormItemTypes.it_COMBO_BOX);

                foreach (IUserFieldValidValue validValue in field.ValidValues) {
                    column.ValidValues.Add(validValue.Value, validValue.Description);
                }
            }
            else if (isLinked)
            {
                column = matrix.Columns.Add(uid, BoFormItemTypes.it_EDIT);

                string cflId = $"_CFL{uid}";

                if (form.ChooseFromLists.OfType<ChooseFromList>().All(c => c.UniqueID != cflId))
                {
                    ChooseFromListCreationParams cflParams = (ChooseFromListCreationParams)application.CreateObject(BoCreatableObjectType.cot_ChooseFromListCreationParams);
                    cflParams.UniqueID = cflId;
                    cflParams.MultiSelection = false;
                    cflParams.ObjectType = field.LinkedSystemObject?.ToString()
                                           ?? field.LinkedUdo
                                           ?? field.LinkedTable;

                    ChooseFromList cfl = form.ChooseFromLists.Add(cflParams);

                    Conditions cflCond = (Conditions)application.CreateObject(BoCreatableObjectType.cot_Conditions);
                    cfl.SetConditions(cflCond);
                }

                column.ChooseFromListUID = cflId;
                column.ChooseFromListAlias = field.Name;
            }
            else {
                column = matrix.Columns.Add(uid, BoFormItemTypes.it_EDIT);
            }

            column.TitleObject.Caption = field.Description;
            column.Visible = true;

            if (field.Size > 0)
                column.Width = field.Size * 5;

            column.Editable = true;

            return column;
        }

        internal static (Column Date, Column Time) AddDateTimeColumnsFromUserTableField(this Matrix matrix, 
            ref int index, 
            DateTimeUserTableFieldAttribute field) {

            Column dateColumn = matrix.Columns.Add($"_C{index++}", BoFormItemTypes.it_EDIT);
            Column timeColumn = matrix.Columns.Add($"_C{index++}", BoFormItemTypes.it_EDIT);
            
            dateColumn.TitleObject.Caption = field.DateDescription;
            dateColumn.Visible = true;

            timeColumn.TitleObject.Caption = field.TimeDescription;
            timeColumn.Visible = true;

            dateColumn.Width = 50;
            timeColumn.Width = 20;

            dateColumn.Editable = true;
            timeColumn.Editable = true;

            return (dateColumn, timeColumn);
        }
    }
}