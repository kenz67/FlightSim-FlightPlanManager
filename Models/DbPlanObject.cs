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
        private string _name;
        private int _rating;

        public int Id { get; set; }

        public string Name
        {
            get { return _name; }
            set { _name = value; OnPropertyChanged(this, nameof(Name)); }
        }

        public string Type { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }

        public int Rating
        {
            get { return _rating; }
            set { _rating = value; OnPropertyChanged(this, nameof(Rating)); }
        }

        public double Distance { get; set; }
        public string Group { get; set; }
        public string Notes { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(object sender, string propertyName)
        {
            //todo update row in db
            //things change even when setting, do something with contsructor
            if (PropertyChanged != null)
            {
                // PropertyChanged(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string Plan { get; set; }
        public string OrigFileName { get; set; }
        public string OrigFullFileName { get; set; }
        public DateTime ImportDate { get; set; }
    }
}