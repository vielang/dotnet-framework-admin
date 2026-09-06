using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows.Forms;
using EmployeeManagementSystem.Forms;
using EmployeeManagementSystem.Views;
using Xunit;

namespace EmployeeManagementSystem.Tests
{
    /// <summary>
    /// Reproduces how the Visual Studio designer hosts these controls, and asserts
    /// that nothing touches the database or the service locator while it does.
    ///
    /// This exists because of a real bug: LicenseManager.UsageMode only reports
    /// Designtime *while a component is being constructed*. A guard that checks it
    /// in the constructor and again in OnLoad gets two different answers, so the
    /// constructor skipped wiring the repository and OnLoad then dereferenced the
    /// null it had left behind - a NullReferenceException in a MessageBox the
    /// moment anyone opened MainForm in the designer.
    ///
    /// Ordinary tests cannot catch it: they construct forms but never show them,
    /// so OnLoad never runs. These tests show them, under a design-time license
    /// context, with AppServices deliberately left uninitialised.
    /// </summary>
    public class DesignerSafetyTests
    {
        public static IEnumerable<object[]> DesignableTypes()
        {
            yield return new object[] { typeof(DashboardView) };
            yield return new object[] { typeof(EmployeeView) };
            yield return new object[] { typeof(SalaryView) };
            yield return new object[] { typeof(MainForm) };
            yield return new object[] { typeof(LoginForm) };
            yield return new object[] { typeof(RegisterForm) };
        }

        [Theory]
        [MemberData(nameof(DesignableTypes))]
        public void Can_be_hosted_by_the_designer_without_touching_the_database(Type type)
        {
            List<Exception> thrown = RunOnStaThread(() => HostLikeTheDesigner(type));

            Assert.True(thrown.Count == 0,
                type.Name + " raised " + thrown.Count + " exception(s) while the designer hosted it. " +
                "First: " + (thrown.Count > 0 ? thrown[0].ToString() : ""));
        }

        /// <summary>
        /// Constructs the control the way the designer does, then hosts it so its
        /// handle is created and OnLoad fires. Returns every exception raised,
        /// including ones the application catches - a swallowed NullReferenceException
        /// is exactly the failure this guards against.
        /// </summary>
        private static void HostLikeTheDesigner(Type type)
        {
            // AppServices is never initialised here, matching devenv.exe where
            // Program.Main has not run.
            Assert.False(AppServices.IsInitialized,
                "AppServices must stay uninitialised for this test to be meaningful.");

            Control control;
            try
            {
                // 1. the designer creates components under a design-time license context
                LicenseManager.CurrentContext = new DesigntimeLicenseContext();
                control = (Control)Activator.CreateInstance(type);
            }
            finally
            {
                // 2. and then goes back to a runtime context - the step that broke it
                LicenseManager.CurrentContext = null;
            }

            // 3. the control goes onto the design surface
            var form = control as Form;
            if (form != null)
            {
                form.Show();
                Application.DoEvents();
                form.Dispose();
            }
            else
            {
                using (var host = new Form())
                {
                    host.Controls.Add(control);
                    host.Show();
                    Application.DoEvents();
                }
                control.Dispose();
            }
        }

        /// <summary>
        /// WinForms needs an STA thread. Collects first-chance exceptions so that
        /// failures the application swallows into a MessageBox are still visible,
        /// ignoring ODP.NET's internal ONS chatter.
        /// </summary>
        private static List<Exception> RunOnStaThread(Action action)
        {
            var thrown = new List<Exception>();
            Exception fatal = null;
            int staThreadId = -1;

            // FirstChanceException is raised for the whole process and xUnit runs
            // test classes in parallel, so this must ignore exceptions belonging to
            // other tests - several of them throw on purpose. The handler runs on
            // whichever thread threw, so the thread id is the filter.
            EventHandler<FirstChanceExceptionEventArgs> handler = (s, e) =>
            {
                if (Thread.CurrentThread.ManagedThreadId != Volatile.Read(ref staThreadId)) return;
                if (e.Exception.GetType().Name == "ONSException") return;
                lock (thrown) { thrown.Add(e.Exception); }
            };

            var thread = new Thread(() =>
            {
                Volatile.Write(ref staThreadId, Thread.CurrentThread.ManagedThreadId);
                AppDomain.CurrentDomain.FirstChanceException += handler;
                try { action(); }
                catch (Exception ex) { fatal = ex; }
                finally { AppDomain.CurrentDomain.FirstChanceException -= handler; }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.True(thread.Join(TimeSpan.FromSeconds(60)),
                "Timed out - a modal dialog is probably blocking, which is itself the bug.");

            if (fatal != null)
            {
                throw new Xunit.Sdk.XunitException("Hosting threw: " + fatal);
            }

            return thrown;
        }
    }
}
