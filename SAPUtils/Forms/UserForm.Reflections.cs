using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using SAPbouiCOM.Framework;

namespace SAPUtils.Forms {
    public abstract partial class UserForm {
        private static readonly Func<FormBase, string> GetFormResource;
        private static readonly Action<FormBase, bool> SetInitialized;

        [Localizable(false)]
        static UserForm() {
            PropertyInfo formResourceProp = typeof(FormBase).GetProperty("FormResource", BindingFlags.NonPublic | BindingFlags.Instance);
            if (formResourceProp != null) {
                ParameterExpression instance = Expression.Parameter(typeof(FormBase), "instance");
                MemberExpression property = Expression.Property(instance, formResourceProp);
                GetFormResource = Expression.Lambda<Func<FormBase, string>>(property, instance).Compile();
            }

            PropertyInfo initializedProp = typeof(FormBase).GetProperty("Initialized", BindingFlags.NonPublic | BindingFlags.Instance);
            // ReSharper disable once InvertIf
            if (initializedProp != null) {
                ParameterExpression instance = Expression.Parameter(typeof(FormBase), "instance");
                ParameterExpression value = Expression.Parameter(typeof(bool), "value");
                BinaryExpression body = Expression.Assign(Expression.Property(instance, initializedProp), value);
                SetInitialized = Expression.Lambda<Action<FormBase, bool>>(body, instance, value).Compile();
            }
        }

        private string FormResource => GetFormResource?.Invoke(this);

        internal bool InitializedSetter
        {
            set => SetInitialized?.Invoke(this, value);
        }
    }
}