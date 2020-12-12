using Android.App;
using Android.Content;
using CgmStatusBar.Interfaces;
using CgmStatusBar.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace CgmStatusBar
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    [IntentFilter(new[] { "CgmStatusBar.CgmDataReceiver" })]
    public class CgmDataReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            var callback = (ICgmDataCallback)context;
            var cgmJsonData = intent.GetStringExtra("cgmJsonData");
            var data = JsonSerializer.Deserialize<IEnumerable<CgmEntry>>(cgmJsonData);

            callback.OnCgmDataChange(data);
        }
    }
}
