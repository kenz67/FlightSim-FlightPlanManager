﻿using DataGridViewAutoFilter;
using FlightPlanManager.DataObjects;
using FlightPlanManager.Models;
using FlightPlanManager.Services;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Device.Location;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace FlightPlanManager.Forms
{
    public partial class MainForm : Form
    {
        private List<DbPlanObject> sortableData = new List<DbPlanObject>();
        private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public MainForm()
        {
            var config = new ConfigureDb();
            config.InitDb();
            InitializeComponent();
            this.Load += this.MainForm_Load;
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView1.CellValueChanged += this.DataGridView1_CellValueChanged;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Text = $"Flight Plan Manager - {Application.ProductVersion}";

            sortableData = DbData.GetData();

            var bs = new BindingSource
            {
                DataSource = sortableData.ToDataTable()
            };
            dataGridView1.DataSource = bs;

            dataGridView1.Columns["id"].Visible = false;
            planDataGridViewTextBoxColumn.Visible = false;
            origFileNameDataGridViewTextBoxColumn.Visible = false;

            groupDataGridViewTextBoxColumn.HeaderCell.Style.Font = new Font(dataGridView1.Font, FontStyle.Bold | FontStyle.Underline);
            notesDataGridViewTextBoxColumn.HeaderCell.Style.Font = new Font(dataGridView1.Font, FontStyle.Bold | FontStyle.Underline);
            ratingDataGridViewTextBoxColumn.HeaderCell.Style.Font = new Font(dataGridView1.Font, FontStyle.Bold | FontStyle.Underline);
            AuthorDataGridViewTextBoxColumn.HeaderCell.Style.Font = new Font(dataGridView1.Font, FontStyle.Bold | FontStyle.Underline);

            importDateDataGridViewTextBoxColumn.DefaultCellStyle.Format = "yyyy-MM-dd";
            fileCreateDateDataTextBoxColumn.DefaultCellStyle.Format = "yyyy-MM-dd";

            dataGridView1.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            dataGridView1.UserDeletingRow += DataGridView1_UserDeletingRow;
            dataGridView1.EditingControlShowing += DataGridView1_EditingControlShowing;
            dataGridView1.MouseDown += DataGridView1_MouseDown;

            dataGridView1.Columns.OfType<DataGridViewColumn>().ToList().ForEach(col => col.Selected = false);

            SetupColumns();
            EnableGridFilter(true);

            this.FormClosing += AppClosing;

            RestoreWindowPosition();
        }

        private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var i = e.RowIndex;
            var sData = sortableData.Find(s => s.Id.Equals((int)dataGridView1.Rows[i].Cells["id"].Value));

            if (dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.Equals(DBNull.Value))
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = string.Empty;

            switch (dataGridView1.Columns[e.ColumnIndex].HeaderText)
            {
                case "Author": sData.Author = (string)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value ?? String.Empty; break;
                case "Group": sData.Group = (string)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value ?? String.Empty; break;
                case "Notes": sData.Notes = (string)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value ?? String.Empty; break;
                case "Rating": sData.Rating = (int)dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value; break;
            }

            DbData.Update(sData);
        }

        private void EnableGridFilter(bool value)
        {
            nameDataGridViewTextBoxColumn.FilteringEnabled = value;
            typeDataGridViewTextBoxColumn.FilteringEnabled = value;
            importDateDataGridViewTextBoxColumn.FilteringEnabled = value;
            departureDataGridViewTextBoxColumn.FilteringEnabled = value;
            destinationDataGridViewTextBoxColumn.FilteringEnabled = value;
            ratingDataGridViewTextBoxColumn = new FlightPlanManager.Controls.DataGridViewRatingColumn();
            distanceDataGridViewTextBoxColumn.FilteringEnabled = value;
            groupDataGridViewTextBoxColumn.FilteringEnabled = value;
            origFileNameDataGridViewTextBoxColumn.FilteringEnabled = value;
            notesDataGridViewTextBoxColumn.FilteringEnabled = value;
            //planDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            //origFullFileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            fileCreateDateDataTextBoxColumn.FilteringEnabled = value;
            DepartureNameTextBoxColumn.FilteringEnabled = value;
            DestinationNameTextBoxColumn.FilteringEnabled = value;
            AuthorDataGridViewTextBoxColumn.FilteringEnabled = value;
            AirportCountDataGridViewTextBoxColumn.FilteringEnabled = value;
            WaypointCountDataGridViewTextBoxColumn.FilteringEnabled = value;

            WaypointCountDataGridViewTextBoxColumn.FilteringEnabled = value;
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

        private void DataGridView_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            string filterStatus = DataGridViewAutoFilterColumnHeaderCell.GetFilterStatus(dataGridView1);
            if (string.IsNullOrEmpty(filterStatus))
            {
                ShowAllLabel.Visible = false;
                FilterStatusLabel.Visible = false;
            }
            else
            {
                ShowAllLabel.Visible = true;
                FilterStatusLabel.Visible = true;
                FilterStatusLabel.Text = filterStatus;
            }
        }

        private void DataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                var data = (sender as DataGridView)?.DataSource as SortableBindingList<DbPlanObject>;
                int column = dataGridView1.CurrentCell.ColumnIndex;
                string headerText = dataGridView1.Columns[column].HeaderText;

                if (headerText.Equals("Group"))
                {
                    if (e.Control is TextBox autoText)
                    {
                        autoText.AutoCompleteMode = AutoCompleteMode.Suggest;
                        autoText.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        AutoCompleteStringCollection DataCollection = new AutoCompleteStringCollection();
                        AddItemsTopAutoCompleteList(DataCollection, DbData.GetDistinct("groupFlownWith"));
                        autoText.AutoCompleteCustomSource = DataCollection;
                    }
                }
                else if (headerText.Equals("Author"))
                {
                    if (e.Control is TextBox autoText)
                    {
                        autoText.AutoCompleteMode = AutoCompleteMode.Suggest;
                        autoText.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        AutoCompleteStringCollection DataCollection = new AutoCompleteStringCollection();
                        AddItemsTopAutoCompleteList(DataCollection, DbData.GetDistinct("author"));
                        autoText.AutoCompleteCustomSource = DataCollection;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Auto Complete");
            }
        }

        public void AddItemsTopAutoCompleteList(AutoCompleteStringCollection col, List<string> data)
        {
            foreach (var s in data)
            {
                if (!string.IsNullOrEmpty(s) && !col.Contains(s))
                    col.Add(s);
            }
        }

        public void AddItemsToGroupAutoCompleteList(AutoCompleteStringCollection col, SortableBindingList<DbPlanObject> data)
        {
            foreach (var plan in data)
            {
                if (plan.Group != null)
                {
                    if (!col.Contains(plan.Group))
                        col.Add(plan.Group);
                }
            }
        }

        public void AddItemsToAuthorAutoCompleteList(AutoCompleteStringCollection col, SortableBindingList<DbPlanObject> data)
        {
            foreach (var plan in data)
            {
                if (plan.Author != null)
                {
                    if (!col.Contains(plan.Author))
                        col.Add(plan.Author);
                }
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

        private void ImportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            const string filter = "Select one or more Plan files (*.pln)|*.pln";

            var folder = DbSettings.GetSetting(DbCommon.SettingsDefaultFolder);
            var imported = new List<string>();
            var duplicates = new List<string>();
            var overwrite = DbSettings.GetBoolSetting(DbCommon.SettingsOverwrite);

            using (OpenFileDialog openFileDialog = new OpenFileDialog { Multiselect = true, InitialDirectory = folder, RestoreDirectory = true, Filter = filter })
            {
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    DbSettings.SaveSetting(DbCommon.SettingsDefaultFolder, new FileInfo(openFileDialog.FileName).DirectoryName);

                    foreach (var planFile in openFileDialog.FileNames)
                    {
                        Logger.Info($"Importing {planFile}");

                        try
                        {
                            var plan = SetupDataObject(planFile);
                            plan.Id = DbData.AddPlan(plan);

                            if (!plan.Id.Equals(-1))
                            {
                                sortableData.Add(plan);
                                dataGridView1.ClearSelection();
                                dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                                dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.SelectedRows[0].Index;

                                if (GeoCoordinates.HasError)
                                {
                                    var txt = $"There was an error calculating the total distance for {planFile}\n\n\nSee log file {Application.StartupPath}\\current.log for details";
                                    MessageBox.Show(txt, "Import Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }

                                imported.Add($"{plan.Name} from file {plan.OrigFileName}");
                            }
                            else
                            {
                                if (!overwrite && openFileDialog.FileNames.Length.Equals(1))
                                {
                                    var txt = $"The file \"{plan.OrigFileName}\" has already been imported for plan name \"{plan.Name}\"";
                                    Logger.Info(txt);
                                    MessageBox.Show(txt, "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                else
                                {
                                    DbData.OverwritePlan(plan);
                                    if (openFileDialog.FileNames.Length.Equals(1))
                                    {
                                        UpdateGrid();
                                    }
                                    else
                                    {
                                        duplicates.Add($"{plan.Name} from file {plan.OrigFileName}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            var txt = $"Error reading {planFile}, invalid format\n\n\nSee log file {Application.StartupPath}\\current.log for details";
                            Logger.Error(ex, txt);
                            MessageBox.Show(txt, "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            Cursor.Current = Cursors.Default;
                        }
                    }

                    if (openFileDialog.FileNames.Length > 1)
                    {
                        ShowResults(overwrite, imported, duplicates);
                        // UpdateGrid();
                    }

                    UpdateGrid();
                }
            }
        }

        private void ShowAllLabel_Click(object sender, EventArgs e)
        {
            DataGridViewAutoFilterTextBoxColumn.RemoveFilter(dataGridView1);
            DataGridViewAutoFilterStarRatingColumn.RemoveFilter(dataGridView1);
        }

        private void UpdateGrid()
        {
            sortableData = DbData.GetData();
            var bs = new BindingSource
            {
                DataSource = sortableData.ToDataTable()
            };
            dataGridView1.DataSource = bs;

            dataGridView1.Refresh();
            dataGridView1.Update();

            //sortableData = DbData.GetData();
            //dataGridView1.DataSource = sortableData;
            //dataGridView1.Refresh();
            //dataGridView1.Update();
        }

        private void ShowResults(bool overwrite, List<string> imported, List<string> duplicates)
        {
            var sb = new StringBuilder("New Imports:\n\n");
            foreach (var i in imported)
            {
                sb.Append("        \u2022 ").AppendLine(i);
            }

            if (overwrite)
            {
                sb.AppendLine("\nUpdated:");
            }
            else
            {
                sb.AppendLine("\nSkipped (already imported):");
            }

            foreach (var d in duplicates)
            {
                sb.Append("        \u2022 ").AppendLine(d);
            }

            FlexibleMessageBox.Show(sb.ToString(), "Import Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private DbPlanObject SetupDataObject(string planFile)
        {
            SimBase_Document data;
            XmlSerializer serializer = new XmlSerializer(typeof(SimBase_Document));
            XmlDocument doc = new XmlDocument();

            doc.Load(planFile);
            using (var sr = new StringReader(doc.InnerXml))
            {
                data = (SimBase_Document)serializer.Deserialize(sr);
            }

            var (distance, airports, waypoints) = CalcDistance(data);

            var fileInfo = new FileInfo(planFile);
            var plan = new DbPlanObject
            {
                Name = data.FlightPlan_FlightPlan.Title ?? DateTime.Now.ToString(),
                Departure = data.FlightPlan_FlightPlan.DepartureID ?? String.Empty,
                Destination = data.FlightPlan_FlightPlan.DestinationID ?? String.Empty,
                Distance = Math.Round(distance * 0.000539957, 1),
                Group = string.Empty,
                OrigFileName = fileInfo.Name,
                OrigFullFileName = planFile,
                Rating = 0,
                ImportDate = DateTime.Now.Date,
                Plan = doc.InnerXml,
                Type = data.FlightPlan_FlightPlan.FPType ?? "VFR",
                FileCreateDate = fileInfo.CreationTime.Date,
                DepartureName = data.FlightPlan_FlightPlan.DepartureName ?? String.Empty,
                DestinationName = data.FlightPlan_FlightPlan.DestinationName ?? String.Empty,
                AirportCount = airports,
                WaypointCount = waypoints
            };

            return plan;
        }

        private (double, int, int) CalcDistance(SimBase_Document data)
        {
            var waypoints = 0;
            var airports = 0;
            double distance = 0;
            GeoCoordinate lastPoint = null;

            foreach (var waypoint in data.FlightPlan_FlightPlan.ATCWaypoint)
            {
                if (lastPoint == null)
                {
                    lastPoint = GeoCoordinates.GetGeoCoodinate(waypoint.WorldPosition);
                }

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

            return (distance, airports, waypoints);
        }

        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var helpDialog = new FlightPlanManager.Forms.Help();
            helpDialog.ShowDialog();
        }

        private void ShowMapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var selectedRow = dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected);
            var id = (int)dataGridView1.Rows[selectedRow].Cells["id"].Value;
            var m = new Map();
            m.ConfigureMap(id);
            m.ShowDialog();
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataGridView1_Delete(sender, e);
        }

        private void DataGridView1_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            if (!DataGridView1_Delete(sender, e))
            {
                e.Cancel = true;
            }
        }

        private bool DataGridView1_Delete(object _, EventArgs __)
        {
            Int32 rowToDelete = dataGridView1.Rows.GetFirstRow(DataGridViewElementStates.Selected);
            if (MessageBox.Show($"Are you sure you want to delete \"{dataGridView1.Rows[rowToDelete].Cells["nameDataGridViewTextBoxColumn"].Value}\" plan?", "Deleting", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Logger.Info($"Deleting {dataGridView1.Rows[rowToDelete].Cells["nameDataGridViewTextBoxColumn"].Value}");
                DbData.Delete((int)dataGridView1.Rows[rowToDelete].Cells["id"].Value);
                dataGridView1.Rows.RemoveAt(rowToDelete);
                dataGridView1.ClearSelection();
                return true;
            }
            else
            {
                return false;
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

        private void SelectColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialog = new ColumnSelect();
            if (dialog.ShowDialog().Equals(DialogResult.OK))
            {
                var cols = new List<DbGridColumn>();
                foreach (DbGridColumn item in dialog.Available.Items)
                {
                    cols.Add(new DbGridColumn { ColumnKey = item.ColumnKey, ColumnName = item.ColumnName, DisplayOrder = -1 });
                }

                short cnt = 0;
                foreach (DbGridColumn item in dialog.Selected.Items)
                {
                    cols.Add(new DbGridColumn { ColumnKey = item.ColumnKey, ColumnName = item.ColumnName, DisplayOrder = cnt++ });
                }

                DbColumns.SaveData(cols);
                SetupColumns();
            }
        }

        private void SetupColumns()
        {
            dataGridView1.Columns.OfType<DataGridViewColumn>().ToList().ForEach(col => col.Visible = false);

            var cols = DbColumns.GetData();
            foreach (var c in cols)
            {
                if (!c.DisplayOrder.Equals(-1))
                {
                    dataGridView1.Columns[c.ColumnKey].Visible = true;
                    dataGridView1.Columns[c.ColumnKey].DisplayIndex = c.DisplayOrder;
                }
            }
        }

        private void SettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Settings().ShowDialog();
        }
    }
}