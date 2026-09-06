using System;
using System.ComponentModel;
using System.Windows.Forms;
using EmployeeManagementSystem.Data;
using EmployeeManagementSystem.Forms;

namespace EmployeeManagementSystem.Views
{
    /// <summary>
    /// Base class for the three panels hosted by MainForm.
    ///
    /// It owns the two things they all got wrong before: querying the database from
    /// the constructor (which also ran inside the Visual Studio designer), and
    /// swallowing every exception into a MessageBox down in the data layer.
    /// </summary>
    public class DataView : UserControl
    {
        private readonly bool _createdByDesigner;
        private readonly IEmployeeRepository _injectedRepository;

        protected DataView()
            : this(null)
        {
        }

        /// <summary>Injects a repository. Passing null falls back to <see cref="AppServices"/>.</summary>
        protected DataView(IEmployeeRepository employees)
        {
            // LicenseManager only reports Designtime while a component is being
            // constructed - by the time OnLoad runs it is back to Runtime. So the
            // answer has to be captured here and remembered.
            _createdByDesigner = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
            _injectedRepository = employees;
        }

        /// <summary>
        /// The employee repository. Resolved lazily so that constructing a view never
        /// requires AppServices to be initialised - which it is not inside the designer.
        /// </summary>
        protected IEmployeeRepository Employees
        {
            get { return _injectedRepository ?? AppServices.Employees; }
        }

        /// <summary>
        /// True when Visual Studio, not the application, is hosting this control.
        /// Both checks are needed: DesignMode is false for a control nested inside
        /// another designed control, and LicenseManager is only accurate during
        /// construction.
        /// </summary>
        protected bool IsDesignTime
        {
            get { return _createdByDesigner || DesignMode; }
        }

        /// <summary>False inside the designer, where Program.Main never wired anything up.</summary>
        private bool HasRepository
        {
            get { return _injectedRepository != null || AppServices.IsInitialized; }
        }

        /// <summary>Reloads this view from the database, marshalling onto the UI thread.</summary>
        public void RefreshData()
        {
            if (IsDesignTime || !HasRepository)
            {
                return;
            }

            if (InvokeRequired)
            {
                Invoke((MethodInvoker)RefreshData);
                return;
            }

            try
            {
                LoadData();
            }
            catch (Exception ex)
            {
                UiMessage.Error(ex);
            }
        }

        /// <summary>Pulls this view's data. Exceptions are reported by <see cref="RefreshData"/>.</summary>
        protected virtual void LoadData()
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            RefreshData();
        }
    }
}
