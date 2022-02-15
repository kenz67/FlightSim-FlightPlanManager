using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightPlanManager.Models
{
    public class DbGridColumn
    {
        public string ColumnKey { get; set; }
        public string ColumnName { get; set; }
        public short DisplayOrder { get; set; }
    }
}