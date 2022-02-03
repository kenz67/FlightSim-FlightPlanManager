using FlightPlanManager.Services;
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

        private void OnPropertyChanged(object sender, string _)
        {
            if (PropertyChanged != null)
            {
                DbData.Update(sender as DbPlanObject);
            }
        }
    }
}