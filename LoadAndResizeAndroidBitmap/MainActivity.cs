using System.IO;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Media;
using Android.OS;
using Android.Widget;

using Path = System.IO.Path;
using Orientation = Android.Media.Orientation;

namespace LoadAndResizeAndroidBitmap
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            var resizeButton = FindViewById<Button>(Resource.Id.resizeButton);
            var imageView = FindViewById<ImageView>(Resource.Id.imageView);

            var filePath = Path.Combine(FilesDir.AbsolutePath, "sintel-wallpaper.jpg");

            // copy the asset onto the device
            using (var asset = Assets.Open("sintel-wallpaper.jpg"))
            using (var local = File.Create(filePath))
            {
                asset.CopyTo(local);
            }

            // set up the button for the resize operation
            resizeButton.Click += delegate
            {
                // To avoid OutOfMemory exceptions, make sure we dispose the old image
                var oldImage = imageView.Drawable as BitmapDrawable;
                if (oldImage != null && oldImage.Bitmap != null)
                {
                    // Clean up the old bitmap
                    imageView.SetImageBitmap(null);
                    var oldBitmap = oldImage.Bitmap;
                    oldBitmap.Recycle();
                    oldBitmap.Dispose();
                    oldBitmap = null;
                }

                var smallImage = LoadAndResizeBitmap(filePath, 200, 200);

                //// Just clean up
                //smallImage.Recycle();
                //smallImage.Dispose();
                //smallImage = null;

                // Show in ImageView
                imageView.SetImageBitmap(smallImage);
            };
        }

        public Bitmap LoadAndResizeBitmap(string fileName, int width, int height)
        {
            var screenHeight = Resources.DisplayMetrics.HeightPixels;
            var screenWidth = Resources.DisplayMetrics.WidthPixels;

            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth = options.OutWidth;
            int inSampleSize = 1;
            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight ? outHeight / height : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            // Clean up the loading options
            options.Dispose();
            options = null;

            // Images are being saved in landscape, so rotate them back to portrait if they were taken in portrait
            ExifInterface exif = new ExifInterface(fileName);
            var o = exif.GetAttribute(ExifInterface.TagOrientation);
            Orientation orientation = (Orientation)int.Parse(o);

            // Clean up the exif
            exif.Dispose();
            exif = null;

            // Get the rotation of the image
            int rotation = 0;
            if (outWidth < outHeight || screenWidth < screenHeight)
            {
                rotation = 90;
            }
            switch (orientation)
            {
                case Orientation.Rotate90:
                    rotation = 90;
                    break;
                case Orientation.Rotate180:
                    rotation = 180;
                    break;
                case Orientation.Rotate270:
                    rotation = 270;
                    break;
            }

            // Rotate the bitmap
            Matrix mtx = new Matrix();
            mtx.PreRotate(rotation);
            Bitmap rotatedBitmap = Bitmap.CreateBitmap(resizedBitmap, 0, 0, resizedBitmap.Width, resizedBitmap.Height, mtx, false);
            mtx.Dispose();
            mtx = null;

            // Clean up the old resized bitmap
            if (resizedBitmap != rotatedBitmap)
            {
                resizedBitmap.Recycle();
                resizedBitmap.Dispose();
                resizedBitmap = null;
            }

            // Scale the bitmap
            var scaledBitmap = Bitmap.CreateScaledBitmap(rotatedBitmap, screenWidth, screenHeight, false);

            // Clean up the old rotated bitmap
            rotatedBitmap.Recycle();
            rotatedBitmap.Dispose();
            rotatedBitmap = null;

            return scaledBitmap;
        }
    }
}

