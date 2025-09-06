using System;
using SAPbouiCOM;
using SAPUtils.__Internal.Enums;
using SAPUtils.I18N;
using SAPUtils.Models.UserTables;

namespace SAPUtils.Forms {
    public abstract partial class ChangeTrackerMatrixForm<T> {
        private void AddContextMenuItems() {
            try {
                Menus menus = Application.Menus;
                // ReSharper disable once LocalizableElement
                MenuItem popupMenu = menus.Item("1280"); // Base contextual Menú

                if (menus.Exists(_addRowMenuUid)) {
                    menus.RemoveEx(_addRowMenuUid);
                }
                if (menus.Exists(_deleteRowMenuUid)) {
                    menus.RemoveEx(_deleteRowMenuUid);
                }

                if (_useAddContextButton) {
                    if (!menus.Exists(_addRowMenuUid)) {
                        MenuCreationParams creationParams = (MenuCreationParams)Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                        creationParams.Type = BoMenuType.mt_STRING;
                        creationParams.UniqueID = _addRowMenuUid;
                        creationParams.String = Texts.ChangeTrackerMatrixForm_AddContextMenuItems_Add_row;
                        creationParams.Enabled = true;
                        creationParams.Position = 1;
                        popupMenu.SubMenus.AddEx(creationParams);
                    }
                }

                // ReSharper disable once InvertIf, Kept for Readability
                if (_useDeleteContextButton) {
                    if (menus.Exists(_deleteRowMenuUid)) return;
                    int rowIndex = _matrix.GetNextSelectedRow(0, BoOrderType.ot_RowOrder);
                    bool restore = false;
                    if (rowIndex > 0) {
                        (T item, Status status) = _data[rowIndex - 1];
                        if (item is ISoftDeletable e) {
                            restore = !e.Active && status != Status.Modify;
                        }
                        else {
                            restore = status == Status.Delete || status == Status.Discard;
                        }
                    }
                    MenuCreationParams creationParams = (MenuCreationParams)Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                    creationParams.Type = BoMenuType.mt_STRING;
                    creationParams.UniqueID = _deleteRowMenuUid;
                    creationParams.String = restore ? Texts.ChangeTrackerMatrixForm_AddContextMenuItems_Restore_row : Texts.ChangeTrackerMatrixForm_AddContextMenuItems_Delete_row;
                    creationParams.Enabled = rowIndex >= 0;
                    creationParams.Position = 2;
                    popupMenu.SubMenus.AddEx(creationParams);
                }
            }
            catch (Exception ex) {
                Application.SetStatusBarMessage(string.Format(Texts.ChangeTrackerMatrixForm_AddContextMenuItems_Error_adding_context_menu___0_, ex.Message), BoMessageTime.bmt_Short);
            }
        }

        private void RemoveContextMenuItems() {
            Menus menus = Application.Menus;

            if (menus.Exists(_addRowMenuUid)) {
                menus.RemoveEx(_addRowMenuUid);
            }

            if (menus.Exists(_deleteRowMenuUid)) {
                menus.RemoveEx(_deleteRowMenuUid);
            }
        }
    }
}