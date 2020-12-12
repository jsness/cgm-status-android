using Android.Graphics;
using System;
using System.Text.Json.Serialization;

namespace CgmStatusBar.Models
{
    public class CgmEntry
    {
        [JsonPropertyName("_id")]
        public string Id { get; set; }

        [JsonPropertyName("sgv")]
        public int Glucose { get; set; }

        [JsonPropertyName("dateString")]
        public DateTime Date { get; set; }

        [JsonPropertyName("direction")]
        public string Direction { get; set; }

        public string DirectionArrow
        {
            get
            {
                switch (Direction.ToLower())
                {
                    case "flat":
                        return "→";
                    case "fortyfiveup":
                        return "↗";
                    case "singleup":
                        return "↑";
                    case "doubleup":
                        return "↑↑";
                    case "fortyfivedown":
                        return "↘";
                    case "singledown":
                        return "↓";
                    case "doubledown":
                        return "↓↓";
                    default:
                        return "?";
                }
            }
        }

        public Color GetColor(CgmSettings settings)
        {
            if (Glucose >= settings.GlucoseHigh || Glucose <= settings.GlucoseLow)
            {
                return Color.Red;
            }

            if (Glucose >= settings.GlucoseTargetTop || Glucose <= settings.GlucoseTargetBottom)
            {
                return Color.DarkOrange;
            }

            return Color.Green;
        }
    }
}
