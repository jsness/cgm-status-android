using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using CgmStatusBar.Extensions;
using CgmStatusBar.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using Xamarin.Forms;
using XamarinTextDrawable;

namespace CgmStatusBar.Services
{
    [Service]
    public class CgmMonitorService : Service
    {
        private readonly string _tag = typeof(CgmMonitorService).FullName;
        private Handler _handler;
        private Action _action;
        private CgmSettings _settings = new CgmSettings();

        private const int FontSize = 60;
        private const int Width = 60;
        private const int Height = 60;
        private const int MinutesToCheck = 1;
        private const string CgmEntriesEndpoint = "/api/v1/entries.json";
        private string ChannelId = "NCN9R8YESFIUHH";

        private static readonly HttpClient _httpClient = new HttpClient();

        public override IBinder OnBind(Intent intent)
        {
            // Intentionally returning null, not using a binder.
            return null;
        }

        public override void OnCreate()
        {
            Log.Debug(_tag, "Creating service");

            CreateNotificationChannel();

            MessagingCenter.Subscribe<CgmSettings>(this, typeof(CgmSettings).FullName, (sender) =>
            {
                _settings = sender;
            });

            _handler = new Handler();

            _action = new Action(() =>
            {
                if (!string.IsNullOrWhiteSpace(_settings.CgmMonitorUrl)
                    && Uri.TryCreate($"{_settings.CgmMonitorUrl}{CgmEntriesEndpoint}", UriKind.Absolute, out var cgmUri)
                    && cgmUri.Scheme == Uri.UriSchemeHttps)
                {
                    Log.Debug(_tag, "Getting CGM data...");

                    var responseTask = _httpClient.GetAsync(cgmUri);

                    responseTask.ContinueWith(result =>
                    {
                        var readTask = result.Result.Content.ReadAsStringAsync();
                        readTask.ContinueWith(readResult =>
                        {
                            HandleResult(readResult.Result);
                            var message = new Intent(typeof(CgmDataReceiver).FullName);
                            message.PutExtra("cgmJsonData", readResult.Result);
                            SendBroadcast(message);
                        });
                    });
                }

                _handler.PostDelayed(_action, (long)TimeSpan.FromMinutes(MinutesToCheck).TotalMilliseconds);
            });

            base.OnCreate();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Log.Debug(_tag, "Starting service");

            var settingsJson = intent.GetStringExtra("settingsJson");
            _settings = JsonSerializer.Deserialize<CgmSettings>(settingsJson);

            _handler.Post(_action);

            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            Log.Debug(_tag, "Stopping service");

            _handler.RemoveCallbacks(_action);

            base.OnDestroy();
        }

        private void HandleResult(string result)
        {
            var entries = JsonSerializer.Deserialize<IEnumerable<CgmEntry>>(result);

            if (entries.Any())
            {
                var firstEntry = entries.First();
                var directionArrow = firstEntry.DirectionArrow;
                var text = $"{firstEntry.Glucose}{directionArrow}";

                var icon = new TextDrawable.Builder()
                    .BeginConfig()
                    .Width(Width)
                    .Height(Height)
                    .FontSize(FontSize)
                    .TextColor(Android.Graphics.Color.Black)
                    .EndConfig()
                    .BuildRect(directionArrow, firstEntry.GetColor(_settings))
                    .ToBitmap()
                    .CreateIcon();

                var builder = new Notification.Builder(this, ChannelId)
                    .SetSmallIcon(icon)
                    .SetLargeIcon(icon)
                    .SetContentText(text);

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.Notify(0, builder.Build());
            }
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                return;
            }

            var channelName = Resources.GetString(Resource.String.channel_name);
            var channelDescription = GetString(Resource.String.channel_description);
            var channel = new NotificationChannel(ChannelId, channelName, NotificationImportance.Default)
            {
                Description = channelDescription
            };

            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
    }
}
