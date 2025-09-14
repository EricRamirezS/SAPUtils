using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using SAPUtils.__Internal.Utils;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace SAPUtils.Utils {
    /// <summary>
    /// Provides functionality to show modal dialog windows as top-most dialogs.
    /// </summary>
    public static class TopMostDialog {
        /// <summary>
        /// Displays a modal dialog window as a top-most dialog in a separate thread and returns the dialog result.
        /// </summary>
        /// <param name="dialog">The dialog to be displayed as a modal dialog.</param>
        /// <param name="icon">An optional icon to be used in the top-most window.</param>
        /// <returns>The <see cref="DialogResult"/> value that indicates the result of the modal dialog.</returns>
        public static DialogResult ShowDialog(CommonDialog dialog, Icon icon = null) {
            DialogResult dialogResult = DialogResult.None;
            Thread t = new Thread(() =>
            {
                Thread.CurrentThread.SetApartmentState(ApartmentState.STA);

                using (Form f = new DummyForm(icon)) {
                    f.Show();
                    dialogResult = dialog.ShowDialog();
                }
            });

            t.SetApartmentState(ApartmentState.STA);

            t.Start();
            t.Join();
            return dialogResult;
        }
    }
}