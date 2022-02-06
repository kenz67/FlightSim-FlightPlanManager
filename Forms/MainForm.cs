using FlightPlanManager.DataObjects;
using FlightPlanManager.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace FlightPlanManager
{
    public partial class MainForm : Form
    {
        private SortableBindingList<DbPlanObject> d = new SortableBindingList<DbPlanObject>();

        public MainForm()
        {
            string appDir = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\FlightPlanManager";
            if (!Directory.Exists(appDir))
            {
                Directory.CreateDirectory(appDir);
            }

            new ConfigureDb().InitDb();

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = $"Flight Plan Manager - {Application.ProductVersion}";

            d = DbData.GetData();

            dataGridView1.DataSource = d;
            dataGridView1.Columns[0].Visible = false;
            dataGridView1.Columns["importDateDataGridViewTextBoxColumn"].DefaultCellStyle.Format = "yyyy-MM-dd";

            dataGridView1.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            dataGridView1.UserDeletingRow += DataGridView1_UserDeletingRow;
            dataGridView1.EditingControlShowing += DataGridView1_EditingControlShowing;
            dataGridView1.MouseDown += DataGridView1_MouseDown;
            deleteToolStripMenuItem.Click += DataGridView1_Delete;

            this.SizeChanged += Form_sizeChanged;
            this.FormClosing += AppClosing;

            RestoreWindowPosition();
        }

        private void RestoreWindowPosition()
        {
            var position = DbSettings.GetSetting(DbCommon.SettingsWindowPosition);
            List<int> settings = position.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(v => int.Parse(v)).ToList();

            if (settings.Count == 5)
            {
                this.SetBounds(settings[1], settings[2], settings[3], settings[4]);
                this.WindowState = (FormWindowState)settings[0];
            }
        }

        private void AppClosing(object sender, EventArgs e)
        {
            Rectangle rect = (WindowState == FormWindowState.Normal) ? DesktopBounds : RestoreBounds;
            DbSettings.SaveSetting(DbCommon.SettingsWindowPosition, String.Format("{0},{1},{2},{3},{4}",
                (int)this.WindowState,
                rect.Left, rect.Top, rect.Width, rect.Height));
        }

        private void Form_sizeChanged(object sender, EventArgs e)
        {
            //shouldn't need this, but anchor not working
            dataGridView1.MinimumSize = new Size(this.Width - 22, this.Height - 22);
        }

        private void DataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var data = (sender as DataGridView)?.DataSource as SortableBindingList<DbPlanObject>;
            int column = dataGridView1.CurrentCell.ColumnIndex;
            string headerText = dataGridView1.Columns[column].HeaderText;

            if (headerText.Equals("Group"))
            {
                TextBox autoText = e.Control as TextBox;
                autoText.AutoCompleteMode = AutoCompleteMode.Suggest;
                autoText.AutoCompleteSource = AutoCompleteSource.CustomSource;
                AutoCompleteStringCollection DataCollection = new AutoCompleteStringCollection();
                AddItemsToAutoCompleteList(DataCollection, data);
                autoText.AutoCompleteCustomSource = DataCollection;
            }
        }

        public void AddItemsToAutoCompleteList(AutoCompleteStringCollection col, SortableBindingList<DbPlanObject> data)
        {
            foreach (var plan in data)
            {
                if (!col.Contains(plan.Group))
                    col.Add(plan.Group);
            }
        }

        private void DataGridView1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var hti = dataGridView1.HitTest(e.X, e.Y);
                if (hti?.RowIndex >= 0)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.Rows[hti.RowIndex].Selected = true;
                }
            }
        }

        private void ShowMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRow = dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected);
            var id = (int)dataGridView1.Rows[selectedRow].Cells["id"].Value;
            var m = new Map();
            m.ConfigureMap(id);
            m.ShowDialog();
        }

        private void DataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            DataGridView1_Delete(sender, e);
        }

        private void DataGridView1_Delete(object sender, EventArgs e)
        {
            Int32 rowToDelete = dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected);
            if (MessageBox.Show($"Are you sure you want to delete \"{dataGridView1.Rows[rowToDelete].Cells["nameDataGridViewColumn"].Value}\" plan?", "Deleting", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DbData.Delete((int)dataGridView1.Rows[rowToDelete].Cells["id"].Value);
                dataGridView1.Rows.RemoveAt(rowToDelete);
                dataGridView1.ClearSelection();
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Int32 rowToExport = dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected);
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                var folder = DbSettings.GetSetting(DbCommon.SettingsDefaultFolder);
                var rowData = DbData.GetData((int)dataGridView1.Rows[rowToExport].Cells["id"].Value);
                saveFileDialog.Filter = "Plan files (*.pln)|*.pln";
                saveFileDialog.FileName = rowData.OrigFileName;
                saveFileDialog.InitialDirectory = folder;
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    DbSettings.SaveSetting(DbCommon.SettingsDefaultFolder, new FileInfo(saveFileDialog.FileName).DirectoryName);
                    using (StreamWriter stream = new StreamWriter(saveFileDialog.FileName))
                    {
                        var planXml = new XmlDocument();
                        planXml.LoadXml(rowData.Plan);
                        planXml.Save(stream);
                    }
                }
            }
        }

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                var folder = DbSettings.GetSetting(DbCommon.SettingsDefaultFolder);
                openFileDialog.Filter = "Select one or more Plan files (*.pln)|*.pln";
                openFileDialog.Multiselect = true;
                openFileDialog.InitialDirectory = folder;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    DbSettings.SaveSetting(DbCommon.SettingsDefaultFolder, new FileInfo(openFileDialog.FileName).DirectoryName);

                    foreach (var planFile in openFileDialog.FileNames)
                    {
                        var waypoints = 0;
                        var airports = 0;
                        try
                        {
                            SimBase_Document data;
                            XmlSerializer serializer = new XmlSerializer(typeof(SimBase_Document));
                            XmlDocument doc = new XmlDocument();

                            doc.Load(planFile);
                            using (var sr = new StringReader(doc.InnerXml))
                            {
                                data = (SimBase_Document)serializer.Deserialize(sr);
                            }

                            double distance = 0;

                            var lastPoint = GeoCoordinates.GetGeoCoodinate(data.FlightPlan_FlightPlan?.DepartureLLA);
                            foreach (var waypoint in data.FlightPlan_FlightPlan.ATCWaypoint)
                            {
                                if (waypoint.ATCWaypointType.Equals("Airport"))
                                {
                                    airports++;
                                }
                                else
                                {
                                    waypoints++;
                                }
                                var newPoint = GeoCoordinates.GetGeoCoodinate(waypoint.WorldPosition);
                                distance += lastPoint.GetDistanceTo(newPoint);
                                lastPoint = newPoint;
                            }

                            distance += lastPoint.GetDistanceTo(GeoCoordinates.GetGeoCoodinate(data.FlightPlan_FlightPlan?.DestinationLLA));

                            var plan = new DbPlanObject
                            {
                                Name = data.FlightPlan_FlightPlan.Title,
                                Departure = data.FlightPlan_FlightPlan.DepartureID,
                                Destination = data.FlightPlan_FlightPlan.DestinationID,
                                Distance = Math.Round(distance * 0.000539957, 1),
                                Group = string.Empty,
                                Notes = $"{waypoints} waypt, {airports} arpt",
                                OrigFileName = new FileInfo(planFile).Name,
                                OrigFullFileName = planFile,
                                Rating = 0,
                                ImportDate = DateTime.Now,
                                Plan = doc.InnerXml,
                                Type = data.FlightPlan_FlightPlan.FPType ?? "VFR",
                            };

                            plan.Id = DbData.AddPlan(plan);

                            if (!plan.Id.Equals(-1))
                            {
                                d.Add(plan);
                                dataGridView1.ClearSelection();
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.SelectedRows[0].Index;
                            }
                            else
                            {
                                MessageBox.Show($"The file \"{plan.OrigFileName}\" has already been imported for plan name \"{plan.Name}\"", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                        catch
                        {
                            MessageBox.Show($"Error reading {planFile}, invalid format", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var helpDialog = new FlightPlanManager.Forms.Help();
            helpDialog.ShowDialog();
        }
    }
}