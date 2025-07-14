using System;
using SAPbouiCOM;
using SAPUtils.Models.UserTables;

namespace SAPUtils.Forms {
    public abstract partial class ChangeTrackerMatrixForm<T> {
        private void AddContextMenuItems() {
            try {
                Menus menus = Application.Menus;
                MenuItem popupMenu = menus.Item("1280"); // Menú contextual base

                if (_useAddContextButton) {
                    if (!menus.Exists("My_AddRow")) {
                        MenuCreationParams creationParams = (MenuCreationParams)Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                        creationParams.Type = BoMenuType.mt_STRING;
                        creationParams.UniqueID = "My_AddRow";
                        creationParams.String = "Añadir fila";
                        creationParams.Enabled = true;
                        creationParams.Position = 1;
                        popupMenu.SubMenus.AddEx(creationParams);
                    }
                }
                if (_userDeleteContextButton) {
                    if (!menus.Exists("My_DeleteRow")) {
                        int rowIndex = _matrix.GetNextSelectedRow(0, BoOrderType.ot_RowOrder);
                        bool restore;
                        (T item, Status status) = _data[rowIndex - 1];
                        if (item is ISoftDeletable e) {
                            restore = !e.Active;
                        }
                        else {
                            restore = status == Status.Delete || status == Status.NewDelete;
                        }
                        MenuCreationParams creationParams = (MenuCreationParams)Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams);
                        creationParams.Type = BoMenuType.mt_STRING;
                        creationParams.UniqueID = "My_DeleteRow";
                        creationParams.String = restore ? "RestaurarFila" : "Eliminar fila";
                        creationParams.Enabled = rowIndex >= 0;
                        creationParams.Position = 2;
                        popupMenu.SubMenus.AddEx(creationParams);
                    }
                }
            }
            catch (Exception ex) {
                Application.SetStatusBarMessage("Error al agregar menú contextual: " + ex.Message, BoMessageTime.bmt_Short);
            }
        }
        private void RemoveContextMenuItems() {
            Menus menus = Application.Menus;

            if (menus.Exists("My_AddRow")) {
                menus.RemoveEx("My_AddRow");
            }

            if (menus.Exists("My_DeleteRow")) {
                menus.RemoveEx("My_DeleteRow");
            }
        }
    }
}