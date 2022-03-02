using FlightPlanManager.Services;
using NLog;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace FlightPlanManager.DataObjects
{
    public class DbPlanObject : INotifyPropertyChanged
    {
        private int _rating;
        private string _group;
        private string _notes;
        private string _author;

        private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public double Distance { get; set; }
        public string Plan { get; set; }
        public string OrigFileName { get; set; }
        public string OrigFullFileName { get; set; }
        public DateTime ImportDate { get; set; }
        public DateTime? FileCreateDate { get; set; }
        public string DepartureName { get; set; }
        public string DestinationName { get; set; }
        public int AirportCount { get; set; }
        public int WaypointCount { get; set; }

        public int Rating
        {
            get { return _rating; }
            set { _rating = value; OnPropertyChanged(this, nameof(Rating)); }
        }

        public string Group
        {
            get { return _group; }
            set { _group = value; OnPropertyChanged(this, nameof(Group)); }
        }

        public string Notes
        {
            get { return _notes; }
            set { _notes = value; OnPropertyChanged(this, nameof(Notes)); }
        }

        public string Author
        {
            get { return _author; }
            set { _author = value; OnPropertyChanged(this, nameof(Author)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(object sender, string _)
        {
            try
            {
                if (PropertyChanged != null)
                {
                    Logger.Info($"Updating {(sender as DbPlanObject)?.Name}");
                    DbData.Update(sender as DbPlanObject);
                }
            }
            catch (Exception ex)
            {
                var txt = $"Error Updating {(sender as DbPlanObject)?.Name}, invalid format\n\n\nSee log file {Application.StartupPath}\\current.log for details";
                Logger.Error(ex, txt);
                MessageBox.Show(txt, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}