using FlightPlanManager.Services;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightPlanManager.DataObjects
{
    public class DbPlanObject : INotifyPropertyChanged
    {
        private int _rating;
        private string _group;
        private string _notes;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }

        private readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public int Rating
        {
            get { return _rating; }
            set { _rating = value; OnPropertyChanged(this, nameof(Rating)); }
        }

        public double Distance { get; set; }

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

        public string Plan { get; set; }
        public string OrigFileName { get; set; }
        public string OrigFullFileName { get; set; }
        public DateTime ImportDate { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

#pragma warning disable S1172 // Unused method parameters should be removed

        private void OnPropertyChanged(object sender, string _)
        {
            if (PropertyChanged != null)
            {
                Logger.Info($"Updating {(sender as DbPlanObject)?.Name}");
                DbData.Update(sender as DbPlanObject);
            }
        }

#pragma warning restore S1172 // Unused method parameters should be removed
    }
}