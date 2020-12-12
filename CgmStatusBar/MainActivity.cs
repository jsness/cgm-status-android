using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using CgmStatusBar.Extensions;
using CgmStatusBar.Interfaces;
using CgmStatusBar.Models;
using CgmStatusBar.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using XamarinTextDrawable;

namespace CgmStatusBar
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, BottomNavigationView.IOnNavigationItemSelectedListener, ICgmDataCallback
    {
        private string ChannelId = "NCN9R8YESFIUHH";
        private const string SettingsFile = "CgmStatusBar\\SettingData.txt";

        private const int FontSize = 60;
        private const int Width = 60;
        private const int Height = 60;

        TextView mainTextView;
        TextView glucoseTextView;
        CgmDataReceiver cgmDataReceiver;
        LinearLayout mainLinearLayout;
        (CgmSettings values, IEnumerable<View> views) settings;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            mainTextView = FindViewById<TextView>(Resource.Id.mainTextView);
            glucoseTextView = FindViewById<TextView>(Resource.Id.glucose);

            BottomNavigationView navigation = FindViewById<BottomNavigationView>(Resource.Id.navigation);
            navigation.SetOnNavigationItemSelectedListener(this);

            CreateNotificationChannel();

            mainLinearLayout = FindViewById<LinearLayout>(Resource.Id.mainLinearLayout);

            var settingsJson = LoadSettings();
            settings = CgmSettingsViewFactory.GetDefaultSettingsViews(this, SaveSettings, LoadSettings());

            var cgmMontorIntent = new Intent(this, typeof(CgmMonitorService));
            cgmMontorIntent.PutExtra("cgmUrl", settings.values.CgmMonitorUrl);
            StartService(cgmMontorIntent);

            cgmDataReceiver = new CgmDataReceiver();
        }

        public void SaveSettings(CgmSettings settings)
        {
            var file = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), SettingsFile);
            var json = JsonSerializer.Serialize(settings);
            System.IO.File.WriteAllText(file, json);

            Xamarin.Forms.MessagingCenter.Send(settings.CgmMonitorUrl, settings.GetType().FullName);
        }

        public CgmSettings LoadSettings()
        {
            var file = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), SettingsFile);

            if (!System.IO.File.Exists(file))
            {
                return new CgmSettings();
            }

            var json = System.IO.File.ReadAllText(file);

            return JsonSerializer.Deserialize<CgmSettings>(json);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();
            RegisterReceiver(cgmDataReceiver, new IntentFilter(typeof(CgmDataReceiver).FullName));
        }

        protected override void OnPause()
        {
            UnregisterReceiver(cgmDataReceiver);
            base.OnPause();
        }

        public bool OnNavigationItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.navigation_home:
                    HandleHomeTab();
                    return true;
                case Resource.Id.navigation_settings:
                    HandleSettingsTab();
                    return true;
                default:
                    return false;
            }
        }

        private void HandleHomeTab()
        {
            mainTextView.SetText(Resource.String.home_text);
            mainLinearLayout.RemoveAllViews();
            mainLinearLayout.AddView(glucoseTextView);
        }

        private void HandleSettingsTab()
        {
            mainTextView.SetText(Resource.String.settings_text);
            mainLinearLayout.RemoveAllViews();
            foreach (var settingView in settings.views)
            {
                var param = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
                mainLinearLayout.AddView(settingView, param);
            }
        }

        public void OnCgmDataChange(IEnumerable<CgmEntry> entries)
        {
            if (entries.Any())
            {
                var firstEntry = entries.First();
                var directionArrow = firstEntry.DirectionArrow;
                var text = $"{firstEntry.Glucose}{directionArrow}";

                glucoseTextView.SetText(text, TextView.BufferType.Normal);

                var icon = new TextDrawable.Builder()
                    .BeginConfig()
                    .Width(Width)
                    .Height(Height)
                    .FontSize(FontSize)
                    .EndConfig()
                    .BuildRect(directionArrow, firstEntry.GetColor(settings.values))
                    .ToBitmap()
                    .CreateIcon();

                var builder = new Notification.Builder(this, ChannelId)
                    .SetSmallIcon(icon)
                    .SetLargeIcon(icon)
                    .SetContentText(text);

                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.Notify(0, builder.Build());
            }
            else
            {
                glucoseTextView.SetText(Resource.String.no_glucose_reading);
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
