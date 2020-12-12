using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CgmStatusBar.Models
{
    public static class CgmSettingsViewFactory
    {
        public static (CgmSettings values, IEnumerable<View> views) GetDefaultSettingsViews(
            Context context,
            Action<CgmSettings> saveSettings,
            CgmSettings savedSettings)
        {
            var values = savedSettings;
            var views = new List<View>();
            var props = typeof(CgmSettings)
                .GetProperties()
                .Where(x => x.GetCustomAttributes(typeof(DisplayNameAttribute), false)
                .Any());

            foreach (var prop in props)
            {
                var attribute = (DisplayNameAttribute)prop
                    .GetCustomAttributes(typeof(DisplayNameAttribute), false)
                    .First();

                var linearLayout = new LinearLayout(context);
                linearLayout.Orientation = Orientation.Horizontal;

                var textView = new TextView(context);
                textView.Text = attribute.DisplayName;
                linearLayout.AddView(textView);

                var editText = new EditText(context);
                editText.SetWidth(400);
                editText.Text = values[prop.Name]?.ToString();
                editText.TextAlignment = TextAlignment.Center;
                editText.FocusChange += (s, e) =>
                {
                    if (!e.HasFocus)
                    {
                        if (prop.PropertyType == typeof(int) && int.TryParse(editText.Text, out var intResult))
                        {
                            values[prop.Name] = intResult;
                        }
                        else
                        {
                            values[prop.Name] = editText.Text;
                        }

                        saveSettings(values);
                    }
                };
                linearLayout.AddView(editText);
                views.Add(linearLayout);
            }

            return (values, views);
        }
    }
}
