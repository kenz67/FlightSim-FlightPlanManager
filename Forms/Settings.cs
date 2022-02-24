using FlightPlanManager.DataObjects;
using FlightPlanManager.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FlightPlanManager.Forms
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Settings_Load(object sender, EventArgs e)
        {
            checkBoxOverwrite.Checked = bool.Parse(DbSettings.GetSetting(DbCommon.SettingsOverwrite));
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            DbSettings.SaveSetting(DbCommon.SettingsOverwrite, checkBoxOverwrite.Checked.ToString());
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}