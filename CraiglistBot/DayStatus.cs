using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapchatBot
{
    public class DayStatus
    {
        public string Day { get; set; }
        public bool Status { get; set; }

        public DayStatus(string day, bool status)
        {
            Day = day;
            Status = status;
        }
    }
}
