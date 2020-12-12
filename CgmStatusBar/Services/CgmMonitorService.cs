using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using CgmStatusBar.Models;
using System;
using System.Net.Http;
using Xamarin.Forms;

namespace CgmStatusBar.Services
{
    [Service]
    public class CgmMonitorService : Service
    {
        private readonly string _tag = typeof(CgmMonitorService).FullName;
        private Handler _handler;
        private Action _action;
        private string _cgmUrl;
        private const string CgmEntriesEndpoint = "/api/v1/entries.json";
        
        private static readonly HttpClient _httpClient = new HttpClient();

        public override IBinder OnBind(Intent intent)
        {
            // Intentionally returning null, not using a binder.
            return null;
        }

        public override void OnCreate()
        {
            Log.Debug(_tag, "Creating service");

            MessagingCenter.Subscribe<string>(this, typeof(CgmSettings).FullName, (sender) =>
            {
                _cgmUrl = sender;
            });

            _handler = new Handler();

            _action = new Action(() =>
            {
                if (!string.IsNullOrWhiteSpace(_cgmUrl) && Uri.TryCreate($"{_cgmUrl}{CgmEntriesEndpoint}", UriKind.Absolute, out var cgmUri) && cgmUri.Scheme == Uri.UriSchemeHttps)
                {
                    Log.Debug(_tag, "Getting CGM data...");

                    var responseTask = _httpClient.GetAsync(cgmUri);

                    responseTask.ContinueWith(result =>
                    {
                        var readTask = result.Result.Content.ReadAsStringAsync();
                        readTask.ContinueWith(readResult =>
                        {
                            var message = new Intent(typeof(CgmDataReceiver).FullName);
                            message.PutExtra("cgmJsonData", readResult.Result);
                            SendBroadcast(message);
                        });
                    });
                }

                _handler.PostDelayed(_action, (long)TimeSpan.FromSeconds(3).TotalMilliseconds);
            });

            base.OnCreate();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Log.Debug(_tag, "Starting service");

            _cgmUrl = intent.GetStringExtra("cgmUrl");

            _handler.PostDelayed(_action, (long)TimeSpan.FromSeconds(3).TotalMilliseconds);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            Log.Debug(_tag, "Stopping service");

            _handler.RemoveCallbacks(_action);

            base.OnDestroy();
        }
    }
}
