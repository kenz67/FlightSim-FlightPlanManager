using FlightPlanManager.Models;
using FlightPlanManager.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListBox;

namespace FlightPlanManager.Forms
{
    public partial class ColumnSelect : Form
    {
        public ListBox Availble { get { return listBoxAvailable; } }
        public ListBox Selected { get { return listBoxSelected; } }

        public ColumnSelect()
        {
            InitializeComponent();

            listBoxSelected.AllowDrop = true;
            listBoxSelected.MouseDown += ListBoxSelected_MouseDown;
            listBoxSelected.DragOver += ListBoxSelected_DragOver;
            listBoxSelected.DragDrop += ListBox1_DragDrop;

            var data = DbColumns.GetData();

            foreach (var c in data.Where(d => d.DisplayOrder.Equals(-1)))
            {
                listBoxAvailable.Items.Add(new DbGridColumn { ColumnKey = c.ColumnKey, ColumnName = c.ColumnName, DisplayOrder = -1 });
            }

            foreach (var c in data.Where(d => d.DisplayOrder > -1).OrderBy(o => o.DisplayOrder))
            {
                listBoxSelected.Items.Add(new DbGridColumn { ColumnKey = c.ColumnKey, ColumnName = c.ColumnName, DisplayOrder = -1 });
            }
        }

        private void ButtonAdd_Click(object sender, EventArgs e)
        {
            while (listBoxAvailable.SelectedItems.Count > 0)
            {
                var item = listBoxAvailable.SelectedItems[0];
                listBoxSelected.Items.Add(item);
                listBoxAvailable.Items.Remove(item);
            }
        }

        private void ButtonRemove_Click(object sender, EventArgs e)
        {
            while (listBoxSelected.SelectedItems.Count > 0)
            {
                var item = listBoxSelected.SelectedItems[0];
                listBoxSelected.Items.Remove(item);
                listBoxAvailable.Items.Add(item);
            }
        }

        private void ListBoxSelected_MouseDown(object sender, MouseEventArgs e)
        {
            if (this.listBoxSelected.SelectedItem == null) return;
            this.listBoxSelected.DoDragDrop(this.listBoxSelected.SelectedItem, DragDropEffects.Move);
        }

        private void ListBoxSelected_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void ListBox1_DragDrop(object sender, DragEventArgs e)
        {
            Point point = listBoxSelected.PointToClient(new Point(e.X, e.Y));
            int index = this.listBoxSelected.IndexFromPoint(point);
            if (index < 0) index = this.listBoxSelected.Items.Count - 1;
            object data = listBoxSelected.SelectedItem;
            this.listBoxSelected.Items.Remove(data);
            this.listBoxSelected.Items.Insert(index, data);
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}