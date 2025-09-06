using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NPOI.OpenXmlFormats.Spreadsheet;
using NPOI.SS;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using SAPbobsCOM;
using SAPbouiCOM;
using SAPUtils.__Internal.Attributes.UserTables;
using SAPUtils.__Internal.Enums;
using SAPUtils.__Internal.Models;
using SAPUtils.__Internal.Query;
using SAPUtils.Attributes.UserTables;
using SAPUtils.I18N;
using SAPUtils.Models.UserTables;
using SAPUtils.Query;
using SAPUtils.Utils;
using ChooseFromList = SAPbouiCOM.ChooseFromList;
using ICell = NPOI.SS.UserModel.ICell;
using IUserTable = SAPUtils.__Internal.Attributes.UserTables.IUserTable;
using ValidValue = SAPbouiCOM.ValidValue;

namespace SAPUtils.Forms {
    public abstract partial class ChangeTrackerMatrixForm<T> {
        #region Export

        [Localizable(false)]
        private void ExportToExcel(object sboObject, SBOItemEventArg pVal) {
            try {
                CancellationTokenSource cts = new CancellationTokenSource();
                Task<MemoryStream> generarExcelTask = Task.Run(() => GenerateExcel(cts.Token), cts.Token);

                string filePath = ChooseSavePath();

                if (string.IsNullOrEmpty(filePath)) {
                    cts.Cancel();
                    return;
                }

                generarExcelTask.Wait(cts.Token);

                if (generarExcelTask.IsCanceled || generarExcelTask.Result == null)
                    return;

                MemoryStream excelStream = generarExcelTask.Result;

                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write)) {
                    excelStream.Position = 0;
                    excelStream.CopyTo(fs);
                }

                int option = ShowMessageBox(
                    string.Format(Texts.ChangeTrackerMatrixForm_ExportToExcel_Export_completed___0_, filePath), 1,
                    Texts.ChangeTrackerMatrixForm_ExportToExcel_OK,
                    Texts.ChangeTrackerMatrixForm_ExportToExcel_Open);

                if (option == 2) {
                    Process.Start(filePath);
                }
            }
            catch (Exception ex) {
                SetStatusBarMessage(string.Format(
                    Texts.ChangeTrackerMatrixForm_ExportToExcel_The_Excel_could_not_be_generated___0_, ex.Message));
            }
        }

        private string ChooseSavePath() {
            const string excelFormat = "(*.xlsx)|*.xlsx";
            using (SaveFileDialog sfd = new SaveFileDialog()) {
                sfd.Title = Texts.ChangeTrackerMatrixForm_ChooseSavePath_Save_Excel;
                sfd.Filter = string.Format(Texts.ChangeTrackerMatrixForm_ChooseSavePath_Excel_files__0_, excelFormat);
                // ReSharper disable LocalizableElement
                sfd.DefaultExt = "xlsx";
                sfd.FileName = $"{Title.Replace(' ', '_')}.xlsx";
                // ReSharper restore LocalizableElement

                DialogResult result = TopMostDialog.ShowDialog(sfd);

                return result != DialogResult.OK ? null : sfd.FileName;
            }
        }

        [Localizable(false)]
        private MemoryStream GenerateExcel(CancellationToken token) {
            if (token.IsCancellationRequested)
                return null;
            XSSFWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet(Title);

            Dictionary<int, (List<(string Value, string Desc)> items, string rangeName, BoExpandType expandType)>
                comboBoxValidations =
                    new Dictionary<int, (List<(string Value, string Desc)> items, string rangeName, BoExpandType
                        expandType)>();

            IDataFormat dataFormat = workbook.CreateDataFormat();

            ICellStyle dateStyle = workbook.CreateCellStyle();
            dateStyle.DataFormat = dataFormat.GetFormat(DisplayInfo.FormatInfo.GetDateFullFormat());

            string CreateNumberFormat(int decimals, string prefix = "", string suffix = "") {
                return decimals <= 0 ? $"{prefix}#,##0{suffix}" : $"{prefix}#,##0.{new string('0', decimals)}{suffix}";
            }

            string CreateMoneyFormat(int decimales) {
                if (decimales < 0 || decimales > 6)
                    throw new ArgumentOutOfRangeException(nameof(decimales),
                        Texts
                            .ChangeTrackerMatrixForm_GenerateExcel_The_number_of_decimal_places_must_be_between_0_and_6_);

                string parteEntera = "#,##0";
                string parteDecimal = decimales > 0 ? "," + new string('0', decimales) : "";
                string patronNumero = parteEntera + parteDecimal;

                string formato =
                    $@"_($* {patronNumero}_);_($* ({patronNumero});_($* ""-""{new string('?', decimales)}_);_(@_)";

                return formato;
            }

            ICellStyle priceStyle = workbook.CreateCellStyle();
            priceStyle.DataFormat = dataFormat.GetFormat(CreateMoneyFormat(DisplayInfo.FormatInfo.PriceDec));

            ICellStyle sumStyle = workbook.CreateCellStyle();
            sumStyle.DataFormat = dataFormat.GetFormat(CreateNumberFormat(DisplayInfo.FormatInfo.SumDec));

            ICellStyle rateStyle = workbook.CreateCellStyle();
            rateStyle.DataFormat = dataFormat.GetFormat(CreateNumberFormat(DisplayInfo.FormatInfo.RateDec));

            ICellStyle qtyStyle = workbook.CreateCellStyle();
            qtyStyle.DataFormat = dataFormat.GetFormat(CreateNumberFormat(DisplayInfo.FormatInfo.QtyDec));

            ICellStyle percentStyle = workbook.CreateCellStyle();
            percentStyle.DataFormat =
                dataFormat.GetFormat(CreateNumberFormat(DisplayInfo.FormatInfo.PercentDec, "", "%"));

            ICellStyle measureStyle = workbook.CreateCellStyle();
            measureStyle.DataFormat = dataFormat.GetFormat(CreateNumberFormat(DisplayInfo.FormatInfo.MeasureDec));

            ICellStyle intStyle = workbook.CreateCellStyle();
            intStyle.DataFormat = dataFormat.GetFormat(CreateNumberFormat(0));

            ICellStyle floatStyle = workbook.CreateCellStyle();
            floatStyle.DataFormat = dataFormat.GetFormat(CreateNumberFormat(DisplayInfo.FormatInfo.QueryDec));

            IRow headerRow = sheet.CreateRow(0);
            List<(int MatrixCol, int ExcelCol)> visibleCols = new List<(int MatrixCol, int ExcelCol)>();
            int excelColIndex = 0;

            for (int col = 1; col < _matrix.Columns.Count - 1; col++) {
                if (token.IsCancellationRequested)
                    return null;

                Column column = _matrix.Columns.Item(col);
                if (!column.Visible)
                    continue;


                headerRow.CreateCell(excelColIndex).SetCellValue(column.Title);
                visibleCols.Add((col, excelColIndex));

                switch (column.Type) {
                    case BoFormItemTypes.it_COMBO_BOX:
                    {
                        List<(string Value, string Desc)> items = new List<(string Value, string Desc)>();
                        for (int i = 0; i < column.ValidValues.Count; i++) {
                            ValidValue v = column.ValidValues.Item(i);
                            items.Add((v.Value, v.Description));
                        }

                        string rangeName = $"ComboVals_{excelColIndex}";
                        comboBoxValidations[excelColIndex] = (items, rangeName, column.ExpandType);
                        break;
                    }
                    case BoFormItemTypes.it_CHECK_BOX:
                    {
                        List<(string Value, string Desc)> items = new List<(string Value, string Desc)> {
                            ("N", Texts.ChangeTrackerMatrixForm_GenerateExcel_No),
                            ("Y", Texts.ChangeTrackerMatrixForm_GenerateExcel_Yes),
                        };

                        string rangeName = $"ComboVals_{excelColIndex}";
                        comboBoxValidations[excelColIndex] = (items, rangeName, BoExpandType.et_DescriptionOnly);
                        break;
                    }
                    case BoFormItemTypes.it_LINKED_BUTTON:
                        ChooseFromList cfl = ChooseFromLists.Item(column.ChooseFromListUID);
                        try {
                            string tableName;
                            try {
                                tableName = SapUtils.GetTableNameFromObjectType(cfl.ObjectType);
                            }
                            catch {
                                tableName = $"@{cfl.ObjectType}";
                            }

                            IWhereBuilder where = Where.Builder();
                            Conditions conditions = cfl.GetConditions();
                            IEnumerator enumerator = conditions.GetEnumerator();
                            while (enumerator.MoveNext()) {
                                Condition cond = (Condition)enumerator.Current;
                                string dbColumn = cond.Alias;
                                BoConditionOperation op = cond.Operation;
                                switch (op) {
                                    case BoConditionOperation.co_NONE:
                                        break;
                                    case BoConditionOperation.co_EQUAL:
                                        where.Equals(dbColumn, cond.CondVal, true);
                                        break;
                                    case BoConditionOperation.co_GRATER_THAN:
                                        where.GreaterThan(dbColumn, cond.CondVal, true);
                                        break;
                                    case BoConditionOperation.co_LESS_THAN:
                                        where.LessThan(dbColumn, cond.CondVal, true);
                                        break;
                                    case BoConditionOperation.co_GRATER_EQUAL:
                                        where.GreaterThanOrEqual(dbColumn, cond.CondVal, true);
                                        break;
                                    case BoConditionOperation.co_LESS_EQUAL:
                                        where.LessThanOrEqual(dbColumn, cond.CondVal, true);
                                        break;
                                    case BoConditionOperation.co_NOT_EQUAL:
                                        where.NotEquals(dbColumn, cond.CondVal, true);
                                        break;
                                    case BoConditionOperation.co_CONTAIN:
                                        where.Like(dbColumn, $"%{cond.CondVal}%", true);
                                        break;
                                    case BoConditionOperation.co_NOT_CONTAIN:
                                        where.NotLike(dbColumn, $"%{cond.CondVal}%", true);
                                        break;
                                    case BoConditionOperation.co_START:
                                        where.Like(dbColumn, $"{cond.CondVal}%", true);
                                        break;
                                    case BoConditionOperation.co_END:
                                        where.Like(dbColumn, $"%{cond.CondVal}", true);
                                        break;
                                    case BoConditionOperation.co_BETWEEN:
                                        where.Between(dbColumn, cond.CondVal, cond.CondEndVal, true);
                                        break;
                                    case BoConditionOperation.co_NOT_BETWEEN:
                                        where.NotBetween(dbColumn, cond.CondVal, cond.CondEndVal, true);
                                        break;
                                    case BoConditionOperation.co_IS_NULL:
                                        where.IsNull(dbColumn, true);
                                        break;
                                    case BoConditionOperation.co_NOT_NULL:
                                        where.IsNotNull(dbColumn, true);
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            if (enumerator is IDisposable disposable) disposable.Dispose();
                            string quotedTableName = SapAddon.Instance().IsHana ? $"\"{tableName}\"" : $"[{tableName}]";
                            string query =
                                $"SELECT * FROM {quotedTableName} {new SqlWhereBuilder(where.Build()).Build()}";
                            Recordset rs = (Recordset)SapAddon.Instance().Company
                                .GetBusinessObject(BoObjectTypes.BoRecordset);
                            rs.DoQuery(query);
                            List<(string Value, string Desc)> items = new List<(string Value, string Desc)> {
                                ("", ""),
                            };
                            if (rs.RecordCount > 1000) break; // Demasiados Datos, se deja sin validación
                            while (!rs.EoF) {
                                string v;
                                string d;
                                try {
                                    string primaryKey = SapUtils.GetPrimaryKey(tableName);
                                    string readable = SapUtils.GetReadableColumn(tableName);
                                    v = rs.Fields.Item(primaryKey).Value.ToString();
                                    d = rs.Fields.Item(readable).Value.ToString();
                                }
                                catch {
                                    v = rs.Fields.Item(0).Value.ToString();
                                    d = rs.Fields.Item(1).Value.ToString();
                                }

                                items.Add((v, d));

                                rs.MoveNext();
                            }

                            string rangeName = $"ComboVals_{excelColIndex}";
                            comboBoxValidations[excelColIndex] = (items, rangeName, BoExpandType.et_ValueDescription);
                        }
                        catch {
                            // No validation
                        }

                        break;
                    case BoFormItemTypes.it_BUTTON:
                    case BoFormItemTypes.it_STATIC:
                    case BoFormItemTypes.it_EDIT:
                    case BoFormItemTypes.it_FOLDER:
                    case BoFormItemTypes.it_RECTANGLE:
                    case BoFormItemTypes.it_PICTURE:
                    case BoFormItemTypes.it_EXTEDIT:
                    case BoFormItemTypes.it_OPTION_BUTTON:
                    case BoFormItemTypes.it_MATRIX:
                    case BoFormItemTypes.it_GRID:
                    case BoFormItemTypes.it_PANE_COMBO_BOX:
                    case BoFormItemTypes.it_ACTIVE_X:
                    case BoFormItemTypes.it_BUTTON_COMBO:
                    case BoFormItemTypes.it_WEB_BROWSER:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                excelColIndex++;
            }

            ISheet mappingSheet = workbook.CreateSheet("ColumnUIDMapping");
            for (int i = 0; i < visibleCols.Count; i++) {
                int matrixColIndex = visibleCols[i].MatrixCol;
                string uid = _matrix.Columns.Item(matrixColIndex).UniqueID;
                IRow row = mappingSheet.CreateRow(i);
                row.CreateCell(0).SetCellValue(i); // Columna de Excel
                row.CreateCell(1).SetCellValue(uid); // UID de SAP
            }

            int mappingSheetIx = workbook.GetSheetIndex("ColumnUIDMapping");
#pragma warning disable CS0612 // Type or member is obsolete
            workbook.SetSheetHidden(mappingSheetIx, SheetVisibility.VeryHidden);
#pragma warning restore CS0612 // Type or member is obsolete

            if (_dataTable.Rows.Count == 0) {
                IRow excelRow = sheet.CreateRow(1);
                foreach ((int matrixCol, int excelCol) in visibleCols) {
                    string columnUid = _matrix.Columns.Item(matrixCol).UniqueID;
                    BoFieldsType fieldType = _dataTable.Columns.Item(columnUid).Type;

                    if (comboBoxValidations.TryGetValue(excelCol,
                            out (List<(string Value, string Desc)> items, string rangeName, BoExpandType expandType)
                            comboInfo)) {
                        (string Value, string Desc) match = comboInfo.items.FirstOrDefault();
                        string valueToSet = "";
                        if (match != default) {
                            switch (comboInfo.expandType) {
                                case BoExpandType.et_ValueOnly:
                                    valueToSet = match.Value;
                                    break;
                                case BoExpandType.et_DescriptionOnly:
                                    valueToSet = match.Desc;
                                    break;
                                case BoExpandType.et_ValueDescription:
                                    valueToSet = $"{match.Value} — {match.Desc}";
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        excelRow.CreateCell(excelCol).SetCellValue(valueToSet);
                    }
                    else {
                        ICell cell = excelRow.CreateCell(excelCol);
                        //Setting Style
                        switch (fieldType) {
                            case BoFieldsType.ft_ShortNumber:
                            case BoFieldsType.ft_Integer:
                                cell.SetCellValue(0);
                                cell.CellStyle = intStyle;
                                break;
                            case BoFieldsType.ft_Date:
                                cell.SetCellValue(DateTime.Today);
                                cell.CellStyle = dateStyle;
                                break;
                            case BoFieldsType.ft_Float:
                                cell.SetCellValue(0);
                                cell.CellStyle = floatStyle;
                                break;
                            case BoFieldsType.ft_Quantity:
                                cell.SetCellValue(0);
                                cell.CellStyle = qtyStyle;
                                break;
                            case BoFieldsType.ft_Price:
                                cell.SetCellValue(0);
                                cell.CellStyle = priceStyle;
                                break;
                            case BoFieldsType.ft_Rate:
                                cell.SetCellValue(0);
                                cell.CellStyle = rateStyle;
                                break;
                            case BoFieldsType.ft_Measure:
                                cell.SetCellValue(0);
                                cell.CellStyle = measureStyle;
                                break;
                            case BoFieldsType.ft_Sum:
                                cell.SetCellValue(0);
                                cell.CellStyle = sumStyle;
                                break;
                            case BoFieldsType.ft_Percent:
                                cell.CellStyle = percentStyle;
                                cell.SetCellValue(0);
                                break;
                            case BoFieldsType.ft_NotDefined:
                            case BoFieldsType.ft_AlphaNumeric:
                            case BoFieldsType.ft_Text:
                            default:
                                cell.SetCellValue("");
                                break;
                        }
                    }
                }
            }
            else {
                for (int row = 0; row < _dataTable.Rows.Count; row++) {
                    if (token.IsCancellationRequested)
                        return null;
                    IRow excelRow = sheet.CreateRow(row + 1);
                    foreach ((int matrixCol, int excelCol) in visibleCols) {
                        string rawValue = _dataTable.GetValue(matrixCol, row)?.ToString() ?? "";

                        if (comboBoxValidations.TryGetValue(excelCol,
                                out (List<(string Value, string Desc)> items, string rangeName, BoExpandType expandType)
                                comboInfo)) {
                            string valueToSet = rawValue;
                            (string Value, string Desc) match =
                                comboInfo.items.FirstOrDefault(x => x.Value == rawValue || x.Desc == rawValue);
                            if (!string.IsNullOrEmpty(match.Value)) {
                                switch (comboInfo.expandType) {
                                    case BoExpandType.et_ValueOnly:
                                        valueToSet = match.Value;
                                        break;
                                    case BoExpandType.et_DescriptionOnly:
                                        valueToSet = match.Desc;
                                        break;
                                    case BoExpandType.et_ValueDescription:
                                        valueToSet = $"{match.Value} — {match.Desc}";
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }
                            }

                            excelRow.CreateCell(excelCol).SetCellValue(valueToSet);
                        }
                        else {
                            ICell cell = excelRow.CreateCell(excelCol);
                            string columnUid = _matrix.Columns.Item(matrixCol).UniqueID;
                            BoFieldsType fieldType = _dataTable.Columns.Item(columnUid).Type;

                            switch (fieldType) {
                                case BoFieldsType.ft_ShortNumber:
                                case BoFieldsType.ft_Integer:
                                    if (int.TryParse(rawValue, out int intVal)) {
                                        cell.SetCellValue(intVal);
                                    }
                                    else {
                                        cell.SetCellValue(rawValue);
                                    }

                                    cell.CellStyle = intStyle;
                                    break;
                                case BoFieldsType.ft_Date:
                                    if (DateTime.TryParse(rawValue, out DateTime dtValue)) {
                                        cell.SetCellValue(dtValue);
                                    }
                                    else {
                                        cell.SetCellValue(rawValue);
                                    }

                                    cell.CellStyle = dateStyle;
                                    break;
                                case BoFieldsType.ft_Float:
                                    if (double.TryParse(rawValue, out double floatVal)) {
                                        cell.SetCellValue(floatVal);
                                    }
                                    else {
                                        cell.SetCellValue(rawValue);
                                    }

                                    cell.CellStyle = floatStyle;
                                    break;
                                case BoFieldsType.ft_Quantity:
                                    if (double.TryParse(rawValue, out double quantityVal)) {
                                        cell.SetCellValue(quantityVal);
                                    }
                                    else {
                                        cell.SetCellValue(rawValue);
                                    }

                                    cell.CellStyle = qtyStyle;
                                    break;
                                case BoFieldsType.ft_Price:
                                    if (double.TryParse(rawValue, out double priceVal)) {
                                        cell.SetCellValue(priceVal);
                                    }
                                    else {
                                        cell.SetCellValue(rawValue);
                                    }

                                    cell.CellStyle = priceStyle;
                                    break;
                                case BoFieldsType.ft_Rate:
                                    if (double.TryParse(rawValue, out double rateVal)) {
                                        cell.SetCellValue(rateVal);
                                    }
                                    else {
                                        cell.SetCellValue(rawValue);
                                    }

                                    cell.CellStyle = rateStyle;
                                    break;
                                case BoFieldsType.ft_Measure:
                                    if (double.TryParse(rawValue, out double measureVal)) {
                                        cell.SetCellValue(measureVal);
                                    }
                                    else {
                                        cell.SetCellValue(rawValue);
                                    }

                                    cell.CellStyle = measureStyle;
                                    break;
                                case BoFieldsType.ft_Sum:
                                    if (double.TryParse(rawValue, out double sumVal)) {
                                        cell.SetCellValue(sumVal);
                                    }
                                    else {
                                        cell.SetCellValue(rawValue);
                                    }

                                    cell.CellStyle = sumStyle;
                                    break;
                                case BoFieldsType.ft_Percent:
                                    if (double.TryParse(rawValue, out double percentVal)) {
                                        cell.SetCellValue(percentVal);
                                    }
                                    else {
                                        cell.SetCellValue(rawValue);
                                    }

                                    cell.CellStyle = percentStyle;
                                    break;
                                case BoFieldsType.ft_NotDefined:
                                case BoFieldsType.ft_AlphaNumeric:
                                case BoFieldsType.ft_Text:
                                default:
                                    cell.SetCellValue(rawValue);
                                    break;
                            }
                        }
                    }
                }
            }


            IDataValidationHelper dvHelper = sheet.GetDataValidationHelper();
            workbook.CreateSheet("ValidationSheet");
            foreach (KeyValuePair<int, (List<(string Value, string Desc)> items, string rangeName, BoExpandType
                         expandType)> kvp in comboBoxValidations) {
                int colIndex = kvp.Key;
                (List<(string Value, string Desc)> items, string rangeName, BoExpandType expandType) = kvp.Value;

                ISheet hiddenSheet = workbook.GetSheet("ValidationSheet");
                for (int i = 2; i < items.Count + 2; i++) {
                    IRow row = hiddenSheet.GetRow(i - 2) ?? hiddenSheet.CreateRow(i - 2);
                    row.CreateCell(colIndex * 3 + 0).SetCellValue(items[i - 2].Value);
                    row.CreateCell(colIndex * 3 + 1).SetCellValue(items[i - 2].Desc);
                    row.CreateCell(colIndex * 3 + 2).SetCellValue($"{items[i - 2].Value} — {items[i - 2].Desc}");
                }

                string colLetter;
                switch (expandType) {
                    case BoExpandType.et_ValueOnly:
                        colLetter = CellReference.ConvertNumToColString(colIndex * 3 + 0);
                        break;
                    case BoExpandType.et_DescriptionOnly:
                        colLetter = CellReference.ConvertNumToColString(colIndex * 3 + 1);
                        break;
                    case BoExpandType.et_ValueDescription:
                        colLetter = CellReference.ConvertNumToColString(colIndex * 3 + 2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                string formulaRange = $"ValidationSheet!${colLetter}${1}:${colLetter}${items.Count}";
                XSSFName name = (XSSFName)workbook.CreateName();
                name.NameName = rangeName;
                name.RefersToFormula = formulaRange;

                CellRangeAddressList addressList =
                    new CellRangeAddressList(1, Math.Max(_dataTable.Rows.Count, 1), colIndex, colIndex);
                IDataValidationConstraint constraint = dvHelper.CreateFormulaListConstraint(rangeName);
                IDataValidation validation = dvHelper.CreateValidation(constraint, addressList);
                validation.ShowErrorBox = true;
                validation.SuppressDropDownArrow = true;

                sheet.AddValidationData(validation);
            }

            int valSheetIx = workbook.GetSheetIndex("ValidationSheet");
#pragma warning disable CS0612 // Type or member is obsolete
            workbook.SetSheetHidden(valSheetIx, SheetVisibility.VeryHidden);
#pragma warning restore CS0612 // Type or member is obsolete

            XSSFSheet xssfSheet = (XSSFSheet)sheet;
            XSSFTable table = xssfSheet.CreateTable();

            CT_Table ctTable = table.GetCTTable();
            ctTable.tableType = ST_TableType.worksheet;
            ctTable.id = (uint)(workbook.GetAllPictures().Count + 1);

            ctTable.name = Regex.Replace(Title, @"[^\w]", "_");
            ctTable.displayName = Regex.Replace(Title, @"[^\w]", "_");

            CellReference topLeft = new CellReference(0, 0);
            CellReference bottomRight = new CellReference(Math.Max(_dataTable.Rows.Count, 1), visibleCols.Count - 1);
            AreaReference area = new AreaReference(topLeft, bottomRight, SpreadsheetVersion.EXCEL2007);

            ctTable.@ref = area.FormatAsString();
            ctTable.headerRowCount = 1;

            CT_TableColumns ctColumns = ctTable.AddNewTableColumns();
            ctColumns.count = (uint)visibleCols.Count;

            for (int i = 0; i < visibleCols.Count; i++) {
                CT_TableColumn ctCol = ctColumns.AddNewTableColumn();
                ctCol.id = (uint)(i + 1);
                string colTitle = sheet.GetRow(0).GetCell(i).StringCellValue;
                ctCol.name = string.IsNullOrWhiteSpace(colTitle) ? $"Col{i + 1}" : colTitle;
            }

            ctTable.tableStyleInfo = new CT_TableStyleInfo {
                name = "TableStyleMedium2",
                showFirstColumn = false,
                showLastColumn = false,
                showRowStripes = true,
                showColumnStripes = false,
            };

            foreach ((int matrixCol, int excelCol) in visibleCols) {
                string columnUid = _matrix.Columns.Item(matrixCol).UniqueID;
                BoFieldsType fieldType = _dataTable.Columns.Item(columnUid).Type;

                IDataValidationConstraint constraint = null;

                switch (fieldType) {
                    case BoFieldsType.ft_ShortNumber:
                        constraint = dvHelper.CreateintConstraint(
                            OperatorType.BETWEEN, short.MinValue.ToString(), short.MaxValue.ToString());
                        break;
                    case BoFieldsType.ft_Integer:
                        constraint = dvHelper.CreateintConstraint(
                            OperatorType.BETWEEN, int.MinValue.ToString(), int.MaxValue.ToString());
                        break;
                    case BoFieldsType.ft_Float:
                    case BoFieldsType.ft_Quantity:
                    case BoFieldsType.ft_Price:
                    case BoFieldsType.ft_Rate:
                    case BoFieldsType.ft_Measure:
                    case BoFieldsType.ft_Sum:
                    case BoFieldsType.ft_Percent:
                        constraint = dvHelper.CreateDecimalConstraint(
                            OperatorType.BETWEEN, "-99999999999", "99999999999");
                        break;
                    case BoFieldsType.ft_Date:
                        constraint = dvHelper.CreateDateConstraint(
                            OperatorType.BETWEEN,
                            "Date(1990, 1, 1)", "Date(9999, 12, 31)",
                            null
                        );
                        break;
                    case BoFieldsType.ft_NotDefined:
                    case BoFieldsType.ft_AlphaNumeric:
                    case BoFieldsType.ft_Text:
                    default:
                        break;
                }

                if (constraint == null) continue;
                CellRangeAddressList addressList = new CellRangeAddressList(
                    1, Math.Max(_dataTable.Rows.Count, 1), excelCol, excelCol);

                IDataValidation validation = dvHelper.CreateValidation(constraint, addressList);
                validation.ShowErrorBox = true;
                switch (constraint.GetValidationType()) {
                    case 1:
                        validation.CreateErrorBox(Texts.ChangeTrackerMatrixForm_GenerateExcel_Invalid_value,
                            Texts.ChangeTrackerMatrixForm_GenerateExcel_Only_integers_are_allowed_); break;
                    case 2:
                        validation.CreateErrorBox(Texts.ChangeTrackerMatrixForm_GenerateExcel_Invalid_value,
                            Texts.ChangeTrackerMatrixForm_GenerateExcel_Only_decimal_numbers_are_allowed_); break;
                    case 4:
                        validation.CreateErrorBox(Texts.ChangeTrackerMatrixForm_GenerateExcel_Invalid_value,
                            Texts.ChangeTrackerMatrixForm_GenerateExcel_Only_dates_are_allowed_); break;
                }

                sheet.AddValidationData(validation);
            }

            const double minCharWidth = 10;
            const double minWidth = minCharWidth * 256;

            foreach ((int _, int excelCol) in visibleCols) {
                if (token.IsCancellationRequested)
                    return null;
                sheet.AutoSizeColumn(excelCol);
                double currentWidth = sheet.GetColumnWidth(excelCol);
                if (currentWidth < minWidth)
                    sheet.SetColumnWidth(excelCol, minWidth);
            }

            using (MemoryStream ms = new MemoryStream()) {
                workbook.Write(ms, true);
                ms.Position = 0;

                return token.IsCancellationRequested ? null : new MemoryStream(ms.ToArray());
            }
        }

        #endregion

        #region Import

        [Localizable(false)]
        private void ImportFromExcel(object sboObject, SBOItemEventArg pVal) {
            if (UnsavedChanges()) {
                int messageBox = Application.MessageBox(
                    Texts
                        .ChangeTrackerMatrixForm_ImportFromExcel_There_are_unsaved_changes__Do_you_want_to_discard_the_changes_,
                    2,
                    Texts.ChangeTrackerMatrixForm_ImportFromExcel_Continue,
                    Texts.ChangeTrackerMatrixForm_ImportFromExcel_Cancel);

                if (messageBox == 2) return;
            }

            try {
                string filePath = ChooseOpenPath();
                if (string.IsNullOrEmpty(filePath))
                    return;

                List<Dictionary<string, object>> importedData = ParseExcel(filePath);

                _failedAdd.Clear();
                _failedDelete.Clear();
                _failedUpdate.Clear();
                LoadData();
                _dataReload = true;

                string keyField = ObjectCodePropertyName;
                (DataColumn _, Column column) = ColumnInfo[keyField];
                IUserTable table = UserTableMetadataCache.GetUserTableAttribute(typeof(T));
                List<(PropertyInfo Property, IUserTableField Field)> userFields =
                    UserTableMetadataCache.GetUserFields(typeof(T));
                foreach (Dictionary<string, object> data in importedData) {
                    if (!ColumnToProperty.TryGetValue(column.UniqueID, out PropertyInfo keyProperty))
                        continue;
                    if (!data.TryGetValue(column.UniqueID, out object keyValue))
                        continue;
                    T original = _observableData.FirstOrDefault(e =>
                        keyProperty.GetValue(e) != null && keyProperty.GetValue(e).ToString() == keyValue.ToString());
                    T item;
                    if (original == null) {
                        item = new T();
                    }
                    else {
                        item = (T)original.Clone();
                    }

                    foreach (KeyValuePair<string, object> keyValuePair in data) {
                        string columnName = keyValuePair.Key;
                        object value = keyValuePair.Value;
                        if (!ColumnToProperty.TryGetValue(columnName, out PropertyInfo propertyInfo))
                            continue;
                        switch (propertyInfo.Name) {
                            case "Code":
                                if (original == null && table.PrimaryKeyStrategy == PrimaryKeyStrategy.Manual) {
                                    item.Code = value?.ToString();
                                }

                                break;
                            case "Name":
                                item.Name = value?.ToString();
                                break;
                            default:
                                (PropertyInfo _, IUserTableField field) =
                                    userFields.FirstOrDefault(e => e.Property.Name == propertyInfo.Name);
                                if (field != null) {
                                    if (field is DateTimeFieldAttribute dtf) {
                                        if (columnName.EndsWith("D")) {
                                            DateTime? date = propertyInfo.GetValue(item) as DateTime?;
                                            if (date.HasValue) {
                                                DateTime readDate = dtf.ParseDateValue(value);
                                                date = new DateTime(
                                                    readDate.Year,
                                                    readDate.Month,
                                                    readDate.Day,
                                                    date.Value.Hour,
                                                    date.Value.Minute,
                                                    date.Value.Second
                                                );
                                            }
                                            else {
                                                date = dtf.ParseDateValue(value);
                                            }

                                            propertyInfo.SetValue(item, date);
                                        }

                                        if (columnName.EndsWith("T")) {
                                            DateTime? time = propertyInfo.GetValue(item) as DateTime?;
                                            if (time.HasValue) {
                                                DateTime readTime = dtf.ParseTimeValue(value);
                                                time = new DateTime(
                                                    time.Value.Year,
                                                    time.Value.Month,
                                                    time.Value.Day,
                                                    readTime.Hour,
                                                    readTime.Minute,
                                                    readTime.Second
                                                );
                                            }
                                            else {
                                                time = dtf.ParseTimeValue(value);
                                            }

                                            propertyInfo.SetValue(item, time);
                                        }
                                    }
                                    else {
                                        if (field is PercentageFieldAttribute) {
                                            try {
                                                double d = Convert.ToDouble(value);
                                                value = d * 100;
                                            }
                                            catch {
                                                //ignored
                                            }
                                        }

                                        object newValue = field.ParseValue(value);
                                        propertyInfo.SetValue(item, newValue);
                                    }
                                }

                                break;
                        }
                    }

                    if (original == null) {
                        _observableData.Add(item);
                        int findIndex = _data.FindIndex(e => ReferenceEquals(e.Item, item));
                        if (findIndex >= 0) {
                            _data[findIndex] = (item, Status.New);
                        }
                    }
                    else {
                        bool equals = ObjectComparer.AreEqualByPublicMembers(original, item);
                        if (equals) continue;
                        int indexOf = _observableData.IndexOf(original);
                        _observableData[indexOf] = item;
                        _data[indexOf] = (item, Status.Modify);
                    }
                }
            }
            catch (Exception ex) {
                SetStatusBarMessage(
                    string.Format(Texts.ChangeTrackerMatrixForm_ImportFromExcel_Could_not_read_Excel___0_, ex.Message));
            }
            finally {
                _dataReload = false;
                UpdateMatrix();
            }
        }

        private string ChooseOpenPath() {
            const string excelFormat = "(*.xlsx)|*.xlsx";
            using (OpenFileDialog sfd = new OpenFileDialog()) {
                sfd.Title = Texts.ChangeTrackerMatrixForm_ChooseOpenPath_Import_Excel_file;
                sfd.Filter = string.Format(Texts.ChangeTrackerMatrixForm_ChooseOpenPath_Excel_files__0_, excelFormat);
                // ReSharper disable LocalizableElement
                sfd.DefaultExt = "xlsx";
                sfd.FileName = $"{Title.Replace(' ', '_')}.xlsx";
                // ReSharper restore LocalizableElement

                DialogResult result = TopMostDialog.ShowDialog(sfd);

                return result != DialogResult.OK ? null : sfd.FileName;
            }
        }

        [Localizable(false)]
        private List<Dictionary<string, object>> ParseExcel(string path) {
            List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read)) {
                XSSFWorkbook workbook = new XSSFWorkbook(fs);

                ISheet sheet = workbook.GetSheet(Title);
                if (sheet == null)
                    throw new Exception(string.Format(
                        Texts
                            .ChangeTrackerMatrixForm_ParseExcel_The_file_does_not_contain_the_sheet___0____Please_generate_the_Excel_file_using_the_export_button_,
                        Title));

                ISheet validationSheet = workbook.GetSheet("ValidationSheet");
                if (validationSheet == null)
                    throw new Exception(Texts
                        .ChangeTrackerMatrixForm_ParseExcel_Invalid_Excel_file__Please_generate_the_Excel_file_using_the_export_button_);

                ISheet mappingSheet = workbook.GetSheet("ColumnUIDMapping");
                if (mappingSheet == null)
                    throw new Exception(Texts
                        .ChangeTrackerMatrixForm_ParseExcel_Invalid_Excel__Please_generate_the_Excel_file_using_the_export_button__);

                IRow headerRow = sheet.GetRow(0);
                int columnCount = headerRow.LastCellNum;

                Dictionary<int, string> columnUiDs = new Dictionary<int, string>();
                for (int i = 0; i <= mappingSheet.LastRowNum; i++) {
                    IRow row = mappingSheet.GetRow(i);
                    if (row == null) continue;

                    int excelIndex = (int)row.GetCell(0).NumericCellValue;
                    string uid = row.GetCell(1).StringCellValue;
                    columnUiDs[excelIndex] = uid;
                }

                Dictionary<int, BoExpandType> expandTypes = new Dictionary<int, BoExpandType>();
                foreach (XSSFName name in workbook.GetAllNames().OfType<XSSFName>()) {
                    if (!name.NameName.StartsWith("ComboVals_")) continue;

                    int colIndex = int.Parse(name.NameName.Substring("ComboVals_".Length));
                    string formula = name.RefersToFormula;
                    if (formula.Contains("$")) {
                        int column = CellReference.ConvertColStringToIndex(formula.Split('$')[1]);
                        int relative = column - colIndex * 3;

                        switch (relative) {
                            case 0:
                                expandTypes[colIndex] = BoExpandType.et_ValueOnly;
                                break;
                            case 1:
                                expandTypes[colIndex] = BoExpandType.et_DescriptionOnly;
                                break;
                            case 2:
                                expandTypes[colIndex] = BoExpandType.et_ValueDescription;
                                break;
                        }
                    }
                }

                Dictionary<int, Dictionary<string, string>> valueMappings =
                    new Dictionary<int, Dictionary<string, string>>();
                for (int colIndex = 0; colIndex < columnCount; colIndex++) {
                    valueMappings[colIndex] = new Dictionary<string, string>();
                    for (int rowIndex = 0; rowIndex <= validationSheet.LastRowNum; rowIndex++) {
                        IRow row = validationSheet.GetRow(rowIndex);
                        if (row == null) continue;

                        string value = row.GetCell(colIndex * 3 + 0)?.ToString().Trim();
                        string desc = row.GetCell(colIndex * 3 + 1)?.ToString().Trim();
                        string combo = row.GetCell(colIndex * 3 + 2)?.ToString().Trim();

                        if (string.IsNullOrEmpty(value) && string.IsNullOrEmpty(desc) &&
                            string.IsNullOrEmpty(combo)) break;

                        if (!string.IsNullOrEmpty(value)) valueMappings[colIndex]["__VAL__" + value] = value;
                        if (!string.IsNullOrEmpty(desc)) valueMappings[colIndex]["__DESC__" + desc] = value;
                        if (!string.IsNullOrEmpty(combo)) valueMappings[colIndex]["__COMBO__" + combo] = value;
                    }
                }

                for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++) {
                    IRow row = sheet.GetRow(rowIdx);
                    if (row == null) continue;

                    Dictionary<string, object> dict = new Dictionary<string, object>();

                    for (int colIdx = 0; colIdx < columnCount; colIdx++) {
                        if (!columnUiDs.TryGetValue(colIdx, out string uid)) continue;

                        ICell cell = row.GetCell(colIdx);
                        object parsedValue;
                        if (valueMappings.TryGetValue(colIdx, out Dictionary<string, string> mapping) &&
                            expandTypes.TryGetValue(colIdx, out BoExpandType expandType)) {
                            string[] keys;
                            string rawText = cell?.ToString().Trim() ?? "";
                            parsedValue = rawText;
                            switch (expandType) {
                                case BoExpandType.et_ValueOnly:
                                    keys = new[] {
                                        "__VAL__" + rawText,
                                    };
                                    break;
                                case BoExpandType.et_DescriptionOnly:
                                    keys = new[] {
                                        "__DESC__" + rawText,
                                    };
                                    break;
                                case BoExpandType.et_ValueDescription:
                                    keys = new[] {
                                        "__COMBO__" + rawText,
                                    };
                                    break;
                                default:
                                    keys = new[] {
                                        "__VAL__" + rawText,
                                        "__DESC__" + rawText,
                                        "__COMBO__" + rawText,
                                    };
                                    break;
                            }

                            foreach (string key in keys) {
                                if (!mapping.TryGetValue(key, out string found)) continue;
                                parsedValue = found;
                                break;
                            }
                        }
                        else {
                            parsedValue = null;
                            if (cell != null) {
                                switch (cell.CellType) {
                                    case CellType.Numeric:
                                        parsedValue = cell.NumericCellValue;
                                        break;
                                    case CellType.Formula:
                                        switch (cell.CachedFormulaResultType) {
                                            case CellType.Numeric:
                                                parsedValue = cell.NumericCellValue;
                                                break;
                                            case CellType.Unknown:
                                            case CellType.Formula:
                                            case CellType.Blank:
                                            case CellType.Boolean:
                                            case CellType.Error:
                                            case CellType.String:
                                            default:
                                                parsedValue = cell.StringCellValue.Trim();
                                                break;
                                        }

                                        break;
                                    case CellType.Boolean:
                                        parsedValue = cell.BooleanCellValue;
                                        break;
                                    case CellType.String:
                                    case CellType.Blank:
                                    case CellType.Unknown:
                                    case CellType.Error:
                                    default:
                                        parsedValue = cell.StringCellValue.Trim();
                                        break;
                                }
                            }
                        }

                        dict[uid] = parsedValue;
                    }

                    if (dict.Values.All(e => e == null || string.IsNullOrWhiteSpace(e.ToString())))
                        continue;

                    result.Add(dict);
                }

                return result;
            }
        }

        #endregion
    }

    internal static class ObjectComparer {
        private static readonly Type[] ExcludedInterfaces = {
            typeof(IAuditable),
            typeof(IAuditableDate),
            typeof(IAuditableUser),
            typeof(ISoftDeletable),
        };

        public static bool AreEqualByPublicMembers(object obj1, object obj2) {
            if (obj1 == null || obj2 == null)
                return obj1 == obj2;

            Type type1 = obj1.GetType();
            Type type2 = obj2.GetType();

            if (type1 != type2)
                return false;

            List<PropertyInfo> props = type1.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead
                            && !IsFromExcludedInterface(p)
                            && !IsIgnored(p))
                .ToList();

            if ((from prop in props
                    let val1 = prop.GetValue(obj1)
                    let val2 = prop.GetValue(obj2)
                    where !Equals(val1, val2)
                    select val1).Any()) {
                return false;
            }

            List<FieldInfo> fields = type1.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(f => !IsFromExcludedInterface(f)
                            && !IsIgnored(f))
                .ToList();

            return !(from field in fields
                let val1 = field.GetValue(obj1)
                let val2 = field.GetValue(obj2)
                where !Equals(val1, val2)
                select val1).Any();
        }

        private static bool IsFromExcludedInterface(MemberInfo member) {
            Type declaringType = member.DeclaringType;

            return declaringType != null && ExcludedInterfaces.Any(iFace => iFace.IsAssignableFrom(declaringType));
        }

        private static bool IsIgnored(MemberInfo member) {
            return Attribute.IsDefined(member, typeof(IgnoreFieldAttribute));
        }
    }
}