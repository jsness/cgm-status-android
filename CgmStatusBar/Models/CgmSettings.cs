using System.ComponentModel;

namespace CgmStatusBar.Models
{
    public class CgmSettings
    {
        [DisplayName("CGM Monitor Url")]
        public string CgmMonitorUrl { get; set; }

        [DisplayName("High Glucose")]
        public int GlucoseHigh { get; set; }

        [DisplayName("Range Top")]
        public int GlucoseTargetTop { get; set; }

        [DisplayName("Range Bottom")]
        public int GlucoseTargetBottom { get; set; }

        [DisplayName("Low Glucose")]
        public int GlucoseLow { get; set; }

        public object this[string propertyName]
        {
            get
            {
                var propInfo = typeof(CgmSettings).GetProperty(propertyName);
                return propInfo.GetValue(this, null);
            }
            set
            {
                var propInfo = typeof(CgmSettings).GetProperty(propertyName);
                propInfo.SetValue(this, value, null);
            }
        }
    }
}
