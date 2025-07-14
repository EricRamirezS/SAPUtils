using SAPbouiCOM;

namespace SAPUtils.Forms
{
    public abstract partial class UserForm : IForm
    {
        public void Select()
        {
            UIAPIRawForm.Select();
        }

        public void Close()
        {
            UIAPIRawForm.Close();
        }

        public void Refresh()
        {
            UIAPIRawForm.Refresh();
        }

        public string GetAsXML()
        {
            return UIAPIRawForm.GetAsXML();
        }

        public void Update()
        {
            UIAPIRawForm.Update();
        }

        public void Freeze(bool newVal)
        {
            UIAPIRawForm.Freeze(newVal);
        }

        public void EnableMenu(string MenuUID, bool EnableFlag)
        {
            UIAPIRawForm.EnableMenu(MenuUID, EnableFlag);
        }

        public void ResetMenuStatus()
        {
            UIAPIRawForm.ResetMenuStatus();
        }

        public void Resize(int lWidth, int lHeight)
        {
            UIAPIRawForm.Resize(lWidth, lHeight);
        }

        public void EnableFormatSearch()
        {
            UIAPIRawForm.EnableFormatSearch();
        }

        public Items Items => UIAPIRawForm.Items;

        public DataSource DataSources => UIAPIRawForm.DataSources;

        public int TypeCount => UIAPIRawForm.TypeCount;

        public string Title
        {
            get => UIAPIRawForm.Title;
            set => UIAPIRawForm.Title = value;
        }

        public BoFormStateEnum State
        {
            get => UIAPIRawForm.State;
            set => UIAPIRawForm.State = value;
        }

        public int Type => UIAPIRawForm.Type;

        public bool Visible
        {
            get => UIAPIRawForm.Visible;
            set => UIAPIRawForm.Visible = value;
        }

        public string DefButton
        {
            get => UIAPIRawForm.DefButton;
            set => UIAPIRawForm.DefButton = value;
        }

        public bool Modal => UIAPIRawForm.Modal;

        public BoFormMode Mode
        {
            get => UIAPIRawForm.Mode;
            set => UIAPIRawForm.Mode = value;
        }

        public int PaneLevel
        {
            get => UIAPIRawForm.PaneLevel;
            set => UIAPIRawForm.PaneLevel = value;
        }

        public int Top
        {
            get => UIAPIRawForm.Top;
            set => UIAPIRawForm.Top = value;
        }

        public int Left
        {
            get => UIAPIRawForm.Left;
            set => UIAPIRawForm.Left = value;
        }

        public int Height
        {
            get => UIAPIRawForm.Height;
            set => UIAPIRawForm.Height = value;
        }

        public int Width
        {
            get => UIAPIRawForm.Width;
            set => UIAPIRawForm.Width = value;
        }

        public bool Selected => UIAPIRawForm.Selected;

        public string UniqueID => UIAPIRawForm.UniqueID;

        public int ClientHeight
        {
            get => UIAPIRawForm.ClientHeight;
            set => UIAPIRawForm.ClientHeight = value;
        }

        public int ClientWidth
        {
            get => UIAPIRawForm.ClientWidth;
            set => UIAPIRawForm.ClientWidth = value;
        }

        public Menus Menu => UIAPIRawForm.Menu;

        public BoFormBorderStyle BorderStyle => UIAPIRawForm.BorderStyle;

        public string TypeEx => UIAPIRawForm.TypeEx;

        public int SupportedModes
        {
            get => UIAPIRawForm.SupportedModes;
            set => UIAPIRawForm.SupportedModes = value;
        }

        public BusinessObject BusinessObject => UIAPIRawForm.BusinessObject;

        public bool AutoManaged
        {
            get => UIAPIRawForm.AutoManaged;
            set => UIAPIRawForm.AutoManaged = value;
        }

        public DataBrowser DataBrowser => UIAPIRawForm.DataBrowser;

        public ChooseFromListCollection ChooseFromLists => UIAPIRawForm.ChooseFromLists;

        public FormSettings Settings => UIAPIRawForm.Settings;

        public bool IsSystem => UIAPIRawForm.IsSystem;

        public string ActiveItem
        {
            get => UIAPIRawForm.ActiveItem;
            set => UIAPIRawForm.ActiveItem = value;
        }

        public string UDFFormUID => UIAPIRawForm.UDFFormUID;

        public string ReportType
        {
            get => UIAPIRawForm.ReportType;
            set => UIAPIRawForm.ReportType = value;
        }

        public bool VisibleEx
        {
            get => UIAPIRawForm.VisibleEx;
            set => UIAPIRawForm.VisibleEx = value;
        }

        public int MaxWidth
        {
            get => UIAPIRawForm.MaxWidth;
            set => UIAPIRawForm.MaxWidth = value;
        }

        public int MaxHeight
        {
            get => UIAPIRawForm.MaxHeight;
            set => UIAPIRawForm.MaxHeight = value;
        }
    }
}