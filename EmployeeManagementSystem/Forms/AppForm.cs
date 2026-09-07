using System;
using System.Windows.Forms;

namespace EmployeeManagementSystem.Forms
{
    /// <summary>
    /// Base class for this application's windows.
    ///
    /// Every form here uses <see cref="FormBorderStyle.None"/> to get the custom purple
    /// chrome. That also removes the title bar - and the title bar is what Windows uses
    /// to move a window. Without the code below, none of these windows could be moved
    /// at all: they open centred and stay there, even if they cover something the user
    /// needs to see.
    ///
    /// The fix is one message. Windows asks a window "what is under this point?" by
    /// sending WM_NCHITTEST; the answer decides what a drag there does. Letting the
    /// base class answer first and then upgrading "client area" to "title bar" makes
    /// the whole background draggable.
    ///
    /// Child controls are separate windows and get their own WM_NCHITTEST, so buttons,
    /// text boxes and the grid keep behaving normally - this only affects bare form
    /// background.
    /// </summary>
    public class AppForm : Form
    {
        private const int WM_NCHITTEST = 0x0084;
        private const int HTCLIENT = 1;    // "the point is over the client area"
        private const int HTCAPTION = 2;   // "the point is over the title bar"

        public AppForm()
        {
            // Dragging by the background would otherwise let a double-click maximise
            // the window, which a fixed-size borderless layout is not designed for.
            MaximizeBox = false;
        }

        /// <summary>Lets the window be dragged by its background. Set false to switch off.</summary>
        public bool DragByBackground { get; set; }

        /// <summary>
        /// Closes the window when Escape is pressed.
        ///
        /// Normally a form gets this from <see cref="Form.CancelButton"/>, but that
        /// property needs an <see cref="IButtonControl"/> and the purple X in the corner
        /// of these forms is a Label with a Click handler, not a Button. A Label cannot
        /// take focus either, so that X is unreachable from the keyboard - this at least
        /// gives the window an Escape route.
        /// </summary>
        public bool EscapeClosesForm { get; set; }

        /// <summary>
        /// Runs before the key reaches any control, which is what makes it a *command*
        /// key rather than ordinary input. Returning true means "handled, stop here".
        /// </summary>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (EscapeClosesForm && keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (DragByBackground
                && m.Msg == WM_NCHITTEST
                && m.Result == (IntPtr)HTCLIENT)
            {
                m.Result = (IntPtr)HTCAPTION;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Enabled here rather than in the constructor so the Visual Studio designer,
            // which hosts the form as a child control, is never affected.
            if (!DesignMode)
            {
                DragByBackground = true;
            }
        }
    }
}
