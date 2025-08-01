using System;
using System.Collections.Generic;
using System.IO;
using SAPbouiCOM;
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
            Logger.Debug("Creating menu");
            try {
                Application.SetStatusBarMessage("Cargando menus", BoMessageTime.bmt_Medium, false);
                Logger.Trace("Creating BoCreatableObjectType.cot_MenuCreationParams");
                MenuCreationParams creationPackage =
                    Application.CreateObject(BoCreatableObjectType.cot_MenuCreationParams) as MenuCreationParams;
                Logger.Trace("Get Menus Object");
                Menus mainMenu = Application.Menus;
                Logger.Trace("Getting Meny Item by id {0}", MenuId);
                MenuItem sapMenuItem = Application.Menus.Item(MenuId);
                Menus menus = sapMenuItem.SubMenus;

                Logger.Trace("Setting new Menus ItemPosition to last index if no position is set");
                Array.ForEach(menuItems, e => e.ItemPosition = e.ItemPosition ?? -1);
                AddMenuItems(mainMenu, creationPackage, menus, menuItems);

                Logger.Trace("Subscribing to Application.MenuEvent");
                Application.MenuEvent += SBO_Application_MenuEvent;
            }
            catch (Exception ex) {
                Logger.Error(ex);
                Application.SetStatusBarMessage(
                    "No logro crearse el nuevo menu. Error: " + ex.Message,
                    BoMessageTime.bmt_Short);
            }
        }


        private void AddMenuItems(Menus mainMenu, MenuCreationParams creationPackage, Menus menus,
            IList<UtilsMenuItem> menuItems) {
            foreach (UtilsMenuItem menuItem in menuItems) {
                if (!menuItem.Available.Invoke()) continue;

                if (menuItem.MenuType == BoMenuType.mt_POPUP) {
                    if (!(menuItem is SapMenuItem)) {
                        Logger.Debug("Adding menu folder {0}", menuItem.Uid);
                        AddMenuItem(creationPackage, menus, menuItem);
                    }

                    Logger.Debug("Adding menu folder {0} item", menuItem.Uid);
                    AddMenuItems(mainMenu, creationPackage, mainMenu.Item(menuItem.Uid).SubMenus, menuItem.SubMenus);
                }
                else {
                    Logger.Debug("Adding menu item {0}", menuItem.Uid);
                    AddMenuItem(creationPackage, menus, menuItem);
                }
            }
        }

        private void AddMenuItem(MenuCreationParams creationPackage, Menus menus, UtilsMenuItem menuItem) {
            if (menuItem is SapMenuItem) {
                Logger.Trace("Menu item {0} is Sap Menu, skipping.", menuItem.Uid);
                return;
            }

            Logger.Trace("Checking if menu item {0} exists", menuItem.Uid);
            if (menus.Exists(menuItem.Uid)) {
                Logger.Trace("Removing menu item {0}", menuItem.Uid);
                menus.RemoveEx(menuItem.Uid);
            }

            creationPackage.Type = menuItem.MenuType;
            creationPackage.UniqueID = menuItem.Uid;
            creationPackage.String = menuItem.Name;
            creationPackage.Enabled = true;

            if (menuItem.ItemPosition.HasValue) {
                creationPackage.Position = menuItem.ItemPosition.Value;
            }

            if (string.IsNullOrEmpty(menuItem.ImageUrl) == false) {
                Logger.Trace("Setting menu item {0}'s image", menuItem.Uid);
                creationPackage.Image = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, menuItem.ImageUrl);
            }

            _menuActions.Add(menuItem.Uid, menuItem.OnClick);
            menus.AddEx(creationPackage);
        }

        private void SBO_Application_MenuEvent(ref MenuEvent pval, out bool bubbleevent) {
            Logger.Trace("SBO_Application_MenuEvent called with pval.MenuUID {0}", pval.MenuUID);
            bubbleevent = true;

            try {
                if (!pval.BeforeAction) return;
                if (!_menuActions.TryGetValue(pval.MenuUID, out Action action)) return;
                Logger.Trace("Invoke {0} related method", pval.MenuUID);
                action.Invoke();
            }
            catch (Exception ex) {
                Logger.Error(ex);
                Application.MessageBox(ex.ToString());
            }
        }
    }
}