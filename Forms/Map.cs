using FlightPlanManager.DataObjects;
using FlightPlanManager.Services;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

// https://www.programmerall.com/article/63301262890/

namespace FlightPlanManager
{
    public partial class Map : Form
    {
        public Map()
        {
            InitializeComponent();
        }

        private void Map_Load(object sender, EventArgs e)
        {
            this.FormClosing += Map_Closing;

            var position = DbSettings.GetSetting(DbCommon.SettingsMapWindowPosition);
            List<int> settings = position.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(v => int.Parse(v)).ToList();

            if (settings.Count == 5)
            {
                this.SetBounds(settings[1], settings[2], settings[3], settings[4]);
                this.WindowState = (FormWindowState)settings[0];
            }

            gMapControl1.DragButton = MouseButtons.Left;
        }

        private void Map_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Rectangle rect = (WindowState == FormWindowState.Normal) ? DesktopBounds : RestoreBounds;
            DbSettings.SaveSetting(DbCommon.SettingsMapWindowPosition, String.Format("{0},{1},{2},{3},{4}",
                (int)this.WindowState,
                rect.Left, rect.Top, rect.Width, rect.Height));
        }

        public void ConfigureMap(int id)
        {
            var planData = DbData.GetData(id);
            var plan = planData.Plan;

            this.Text = $"{planData.Departure} to {planData.Destination}";

            SimBase_Document data;
            XmlSerializer serializer = new XmlSerializer(typeof(SimBase_Document));
            XmlDocument doc = new XmlDocument();

            doc.LoadXml(plan);

            using (var sr = new StringReader(doc.InnerXml))
            {
                data = (SimBase_Document)serializer.Deserialize(sr);

                var points = new List<PointLatLng>();
                var markers = new GMapOverlay("markers");
                GMarkerGoogle marker = null;

                for (int i = 0; i < data.FlightPlan_FlightPlan.ATCWaypoint.Count; i++)
                {
                    var waypoint = data.FlightPlan_FlightPlan.ATCWaypoint[i];
                    var latlong = GeoCoordinates.GetGeoCoodinate(waypoint.WorldPosition);
                    var point = new PointLatLng(latlong.Latitude, latlong.Longitude);

                    if (i == 0)
                    {
                        marker = new GMarkerGoogle(point, GMarkerGoogleType.green);
                    }
                    else if (i == data.FlightPlan_FlightPlan.ATCWaypoint.Count - 1)
                    {
                        marker = new GMarkerGoogle(point, GMarkerGoogleType.red);
                    }
                    else
                    {
                        marker = new GMarkerGoogle(point, waypoint.ATCWaypointType.Equals("Airport") ? GMarkerGoogleType.orange : GMarkerGoogleType.blue);
                    }

                    if (i == data.FlightPlan_FlightPlan.ATCWaypoint.Count / 2)
                    {
                        gMapControl1.Position = point;
                    }

                    marker.ToolTipText = $"{waypoint.ATCWaypointType} - {waypoint.Id}";
                    markers.Markers.Add(marker);
                    points.Add(point);
                }

                var route = new GMapRoute(points, "route");
                var routes = new GMapOverlay("routes");
                route.Stroke = new Pen(Color.Black, 3);
                routes.Routes.Add(route);

                gMapControl1.Overlays.Add(markers);
                gMapControl1.Overlays.Add(routes);
            }

            gMapControl1.MapProvider = GMapProviders.OpenStreetMap;
            gMapControl1.ShowCenter = false;

            switch (planData.Distance)
            {
                case double n when (n < 100): gMapControl1.Zoom = 9; break;
                case double n when (n < 300): gMapControl1.Zoom = 7; break;
                case double n when (n < 1000): gMapControl1.Zoom = 6; break;
                case double n when (n < 5500): gMapControl1.Zoom = 4; break;
                case double n when (n < 8000): gMapControl1.Zoom = 3; break;
                default: gMapControl1.Zoom = 3; break;
            }
        }
    }
}