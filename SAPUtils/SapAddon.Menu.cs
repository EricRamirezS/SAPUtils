using System;
using System.Collections.Generic;
using System.IO;
using SAPbouiCOM;
using SAPUtils.I18N;
using SAPUtils.Models.Menu;
using MenuItem = SAPbouiCOM.MenuItem;
using UtilsMenuItem = SAPUtils.Models.Menu.IMenuItem;


namespace SAPUtils {
    public partial class SapAddon {
        private const string MenuId = "43520";
        private readonly Dictionary<string, Action> _menuActions = new Dictionary<string, Action>();

        /// <summary>
        /// Creates a menu in the SAP Business One application with the provided menu items.
        /// </summary>
        /// <param name="menuItems">A list of menu items to be added to the menu structure.</param>
        public void CreateMenu(params UtilsMenuItem[] menuItems) {
            Logger.Debug(Texts.SapAddon_CreateMenu_Loading_Menus);
            try {
                Application.SetStatusBarMessage(Texts.SapAddon_CreateMenu_Loading_Menus, BoMessageTime.bmt_Medium, false);
                Logger.Trace(Texts.SapAddon_CreateMenu_Creating_BoCreatableObjectType_cot_MenuCreationParams);
                MenuCreationParams creationPackage =
                    Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams) as MenuCreationParams;
                Logger.Trace(Texts.SapAddon_CreateMenu_Get_Menus_Object);
                Menus mainMenu = Application.Menus;
                Logger.Trace(Texts.SapAddon_CreateMenu_Getting_Menu_Item_by_id__0_, MenuId);
                MenuItem sapMenuItem = Application.Menus.Item(MenuId);
                Menus menus = sapMenuItem.SubMenus;

                Logger.Trace(Texts.SapAddon_CreateMenu_Setting_new_Menus_ItemPosition_to_last_index_if_no_position_is_set);
                Array.ForEach(menuItems, e => e.ItemPosition = e.ItemPosition ?? -1);
                AddMenuItems(mainMenu, creationPackage, menus, menuItems);

                Logger.Trace(Texts.SapAddon_CreateMenu_Subscribing_to_Application_MenuEvent);
                Application.MenuEvent += SBO_Application_MenuEvent;
            }
            catch (Exception ex) {
                Logger.Error(ex);
                Application.SetStatusBarMessage(
                    string.Format(Texts.SapAddon_CreateMenu_The_new_menu_could_not_be_created__Error___0_, ex.Message),
                    BoMessageTime.bmt_Short);
            }
        }


        private void AddMenuItems(Menus mainMenu, MenuCreationParams creationPackage, Menus menus,
            IList<UtilsMenuItem> menuItems) {
            foreach (UtilsMenuItem menuItem in menuItems) {
                if (!menuItem.Available.Invoke()) continue;

                if (menuItem.MenuType == BoMenuType.mt_POPUP) {
                    if (!(menuItem is SapMenuItem)) {
                        Logger.Debug(Texts.SapAddon_AddMenuItems_Adding_menu_folder__0_, menuItem.Uid);
                        AddMenuItem(creationPackage, menus, menuItem);
                    }

                    Logger.Debug(Texts.SapAddon_AddMenuItems_Adding_menu_folder__0__item, menuItem.Uid);
                    AddMenuItems(mainMenu, creationPackage, mainMenu.Item(menuItem.Uid).SubMenus, menuItem.SubMenus);
                }
                else {
                    Logger.Debug(Texts.SapAddon_AddMenuItems_Adding_menu_item__0_, menuItem.Uid);
                    AddMenuItem(creationPackage, menus, menuItem);
                }
            }
        }

        private void AddMenuItem(MenuCreationParams creationPackage, Menus menus, UtilsMenuItem menuItem) {
            if (menuItem is SapMenuItem) {
                Logger.Trace(Texts.SapAddon_AddMenuItem_Menu_item__0__is_Sap_Menu__skipping_, menuItem.Uid);
                return;
            }

            Logger.Trace(Texts.SapAddon_AddMenuItem_Checking_if_menu_item__0__exists, menuItem.Uid);
            if (menus.Exists(menuItem.Uid)) {
                Logger.Trace(Texts.SapAddon_AddMenuItem_Removing_menu_item__0_, menuItem.Uid);
                menus.RemoveEx(menuItem.Uid);
            }

            creationPackage.Type = menuItem.MenuType;
            creationPackage.UniqueID = menuItem.Uid;
            creationPackage.String = menuItem.Name;
            creationPackage.Enabled = true;

            if (menuItem.ItemPosition.HasValue) {
                creationPackage.Position = menuItem.ItemPosition.Value;
            }

            if (!string.IsNullOrEmpty(menuItem.ImageUrl)) {
                Logger.Trace(Texts.SapAddon_AddMenuItem_Setting_menu_item__0__s_image, menuItem.Uid);
                creationPackage.Image = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, menuItem.ImageUrl);
            }

            _menuActions.Add(menuItem.Uid, menuItem.OnClick);
            menus.AddEx(creationPackage);
        }

        private void SBO_Application_MenuEvent(ref MenuEvent pval, out bool bubbleevent) {
            Logger.Trace(Texts.SapAddon_SBO_Application_MenuEvent_SBO_Application_MenuEvent_called_with_pval_MenuUID__0_, pval.MenuUID);
            bubbleevent = true;

            try {
                if (!pval.BeforeAction) return;
                if (!_menuActions.TryGetValue(pval.MenuUID, out Action action)) return;
                Logger.Trace(Texts.SapAddon_SBO_Application_MenuEvent_Invoke__0__related_method, pval.MenuUID);
                action.Invoke();
            }
            catch (Exception ex) {
                Logger.Error(ex);
                Application.MessageBox(ex.Message);
            }
        }
    }
}