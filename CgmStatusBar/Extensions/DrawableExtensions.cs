using Android.Graphics;
using Android.Graphics.Drawables;

namespace CgmStatusBar.Extensions
{
    public static class DrawableExtensions
    {
        public static Bitmap ToBitmap(this Drawable @this)
        {
            if (@this is BitmapDrawable bitmapDrawable && bitmapDrawable.Bitmap != null)
            {
                return bitmapDrawable.Bitmap;
            }

            Bitmap bitmap;
            if (@this.IntrinsicWidth <= 0 || @this.IntrinsicHeight <= 0)
            {
                bitmap = Bitmap.CreateBitmap(1, 1, Bitmap.Config.Argb8888);
            }
            else
            {
                bitmap = Bitmap.CreateBitmap(@this.IntrinsicWidth, @this.IntrinsicHeight, Bitmap.Config.Argb8888);
            }

            var canvas = new Canvas(bitmap);

            @this.SetBounds(0, 0, canvas.Width, canvas.Height);
            @this.Draw(canvas);

            return bitmap;
        }

        public static Icon CreateIcon(this Bitmap @this) => Icon.CreateWithBitmap(@this);
    }
}
