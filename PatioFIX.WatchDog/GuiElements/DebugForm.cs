using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace PatioFIX.WatchDog
{
    public partial class DebugForm : Form, IMonitor
    {
        public static DebugForm Instance;


        bool ShowErrors
        {
            get
            {
                return this.chkError.Checked;
            }
        }
        bool ShowWarnings
        {
            get
            {
                return this.chkWarning.Checked;
            }
        }
        bool ShowInfos
        {
            get
            {
                return this.chkInfo.Checked;
            }
        }
        bool ShowVerbose
        {
            get
            {
                return this.chkVerbose.Checked;
            }
        }


        public DebugForm()
        {
            if (Instance != null)
            {
                throw new Exception("DebugForm already exists!");
            }
            InitializeComponent();
            Instance = this;
            LocalSystem.Initialize();
        }

        private void DebugForm_Load(object sender, EventArgs e)
        {
            Globals.IsGuiPresent = true;
            Globals.GuiMonitorInstance = this;
            this.Text = "Patio.Management.Aggregator - disconnected!";

            this.chkVerbose.Checked = true;
            this.chkInfo.Checked = true;
            this.chkWarning.Checked = true;
            this.chkError.Checked = true;
        }

        private void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                TheController.Instance.Stop();
                TheController.Instance.Quit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Globals.ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (ex.InnerException != null)
                {
                    MessageBox.Show(ex.InnerException.Message, Globals.ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OnBtnStart(object sender, EventArgs e)
        {
            try
            {
                this.btnStart.Enabled = false;
                this.btnStop.Enabled = false;

                TheController.Instance.Start();

                this.btnStop.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Globals.ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (ex.InnerException != null)
                {
                    MessageBox.Show(ex.InnerException.Message, Globals.ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.btnStart.Enabled = true;
                this.btnStop.Enabled = false;
            }
        }

        private void OnBtnStop(object sender, EventArgs e)
        {
            try
            {
                TheController.Instance.Stop();

                this.btnStart.Enabled = true;
                this.btnStop.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Globals.ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (ex.InnerException != null)
                {
                    MessageBox.Show(ex.InnerException.Message, Globals.ServiceName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.btnStart.Enabled = false;
                this.btnStop.Enabled = true;
            }
        }

        private void OnBtnClear(object sender, EventArgs e)
        {
            traceListView.Items.Clear();
        }



        public void ShowMessage(TraceLevel level, string msg, string ownerName)
        {
            if (level == TraceLevel.Error && !ShowErrors)
                return;
            if (level == TraceLevel.Warning && !ShowWarnings)
                return;
            if (level == TraceLevel.Verbose && !ShowVerbose)
                return;
            if (level == TraceLevel.Info && !ShowInfos)
                return;
            traceListView.Invoke(new Action<TraceLevel, string, string>(ShowMessageImpl), new object[] { level, msg, ownerName });
        }
        void ShowMessageImpl(TraceLevel level, string msg, string ownerName)
        {
            int rowIndex = GetRowIndex();

            ListViewItem lvitem = new ListViewItem(level.ToString());
            lvitem.SubItems.Add(ownerName);
            lvitem.SubItems.Add(msg);

            traceListView.Items.Add(lvitem);
            traceListView.EnsureVisible(rowIndex);

            ColorizeListViewItem(level, lvitem);
        }

        public void ShowException(TraceLevel level, Exception ex, string ownerName)
        {
            if (level == TraceLevel.Error && !ShowErrors)
                return;
            if (level == TraceLevel.Warning && !ShowWarnings)
                return;
            if (level == TraceLevel.Verbose && !ShowVerbose)
                return;
            if (level == TraceLevel.Info && !ShowInfos)
                return;
            traceListView.Invoke(new Action<TraceLevel, Exception, string>(ShowExceptionImpl), new object[] { level, ex, ownerName });
        }
        void ShowExceptionImpl(TraceLevel level, Exception ex, string ownerName)
        {
            int rowIndex = GetRowIndex();

            ListViewItem lvitem = new ListViewItem(level.ToString());
            lvitem.Tag = ex;
            lvitem.SubItems.Add(ownerName);
            lvitem.SubItems.Add(ex.Message);

            traceListView.Items.Add(lvitem);
            traceListView.EnsureVisible(rowIndex);

            ColorizeListViewItem(level, lvitem);
        }

        private void OnViewDoubleClick(object sender, EventArgs e)
        {
            ListViewItem lvitem = traceListView.SelectedItems[0];
            if (lvitem == null)
                return;

            if (lvitem.Tag == null)
            {
                //Απλό text message
                TraceLevel level = (TraceLevel)Enum.Parse(typeof(TraceLevel), lvitem.SubItems[0].Text);
                switch (level)
                {
                    case TraceLevel.Error:
                        MessageBox.Show(lvitem.SubItems[2].Text, "MainControllerMonitor - Error Message:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    case TraceLevel.Warning:
                        MessageBox.Show(lvitem.SubItems[2].Text, "MainControllerMonitor - Warning Message:", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        break;
                    case TraceLevel.Info:
                        MessageBox.Show(lvitem.SubItems[2].Text, "MainControllerMonitor - Info Message:", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case TraceLevel.Verbose:
                        MessageBox.Show(lvitem.SubItems[2].Text, "MainControllerMonitor - Verbose Message:", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                }
            }
            else
            {
                //Εχουμε Exception
                ShowExceptionForm form = new ShowExceptionForm();
                form.LoggedException = (Exception)lvitem.Tag;
                form.ShowDialog();
            }
        }

        int GetRowIndex()
        {
            int rowIndex = traceListView.Items.Count;
            if (rowIndex > 800)
            {
                traceListView.Items.Clear();
                rowIndex = 0;
            }
            return rowIndex;
        }
        void ColorizeListViewItem(TraceLevel level, ListViewItem lvitem)
        {
            switch (level)
            {
                case TraceLevel.Error:
                    lvitem.ForeColor = Color.Red;
                    lvitem.Font = new Font(lvitem.Font, FontStyle.Bold);
                    break;
                case TraceLevel.Warning:
                    lvitem.ForeColor = Color.Coral;
                    lvitem.Font = new Font(lvitem.Font, FontStyle.Bold);
                    break;
                case TraceLevel.Info:
                    lvitem.ForeColor = Color.Black;
                    break;
                case TraceLevel.Verbose:
                    lvitem.ForeColor = Color.DimGray;
                    break;
            }
        }

    }
}
