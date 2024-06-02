using System;

namespace HeatingSystemJournal
{
    public class HeatingRecord
    {
        public string Location { get; set; }
        public int BoilerNumber { get; set; }
        public int HeatingObjectsCount { get; set; }
        public DateTime StartDate { get; set; }
        public double StartTemperature { get; set; }
        public double EndTemperature { get; set; }
        public DateTime EndDate { get; set; }
        public int GetSeasonDuration()
        {
            return (EndDate - StartDate).Days;
        }
    }
}
