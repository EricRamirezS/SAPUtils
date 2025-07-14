using System;
using System.IO;
using System.Xml;
using SAPbouiCOM.Framework;
using SAPUtils.Utils;
using Application = SAPbouiCOM.Application;
using Company = SAPbobsCOM.Company;

namespace SAPUtils.Forms
{
    public abstract partial class UserForm : FormBase
    {
        public Company Company => SapAddon.Instance().Company;
        public Application Application => SapAddon.Instance().Application;

        protected UserForm()
        {
            LoadForm();
            OnInitializeComponent();
            InitializedSetter = true;
        }

        private void LoadForm()
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FormResource));
            XmlNode formNode = xmlDoc.SelectSingleNode("//form");
            if (formNode == null)
            {
                throw new InvalidDataException("Form is not valid");
            }

            if (formNode.Attributes == null) return;

            formNode.Attributes["FormType"].Value = FormType;

            string uidValue;
            XmlAttribute attrb = formNode.Attributes?["uid"];
            if (attrb != null)
            {
                uidValue = formNode.Attributes["uid"].Value;
                if (!string.IsNullOrEmpty(uidValue) && FormUtils.ExistForm(uidValue, Application))
                {
                    Application.Forms.Item(uidValue).Close();
                }
                else if (string.IsNullOrEmpty(uidValue))
                {
                    uidValue = Guid.NewGuid().ToString("N").Substring(0, 10);
                    formNode.Attributes["uid"].Value = uidValue;
                }
            }
            else
            {
                attrb = xmlDoc.CreateAttribute("uid");
                uidValue = Guid.NewGuid().ToString("N").Substring(0, 10);
                attrb.Value = uidValue;
                formNode.Attributes?.Append(attrb);
            }

            Application.LoadBatchActions(xmlDoc.InnerXml);
        }
        
        public void Show()
        {
            if (!this.Alive)
                return;
            this.UIAPIRawForm.Visible = true;
        }
    }
}