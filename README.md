# Loading & Resizing a Bitmap in Xamarin.Android

When working with `Bitmap`s in Xamarin.Android, we need to remember that we are dealing with 2 
garbage collectors: one in the Java world and one in the .NET world. Although this may seem like 
extra work, it is simple to do.

In the simple case of `Bitmap` processing, we need to be aggressive with the disposal - aka
`Recycle`:

```csharp
// create the bitmap instance
Bitmap myBitmap = BitmapFactory.DecodeFile("path/to/image.jpg");

// do some image processing ...

// let Java know we are finished
myBitmap.Recycle();

// let .NET know we are finished
myBitmap.Dispose();

// optional cleanup
myBitmap = null;
```

After calling `Recycle` we cannot make use of the image anymore, since we actually just deleted 
all the data in memory. So, if there is image caching, we can't do this.

Sometimes we need to do a little more, typically when we are accessing the `Bitmap` from an
`ImageView` or some other Java-based reference. We can usually add a `GC.Collect` call, but 
we just need to make sure that we don't overdo it as the GC is relatively slow.

```csharp
// get the bitmap
Bitmap myBitmap = anotherObject.Bitmap;

// remove the reference
anotherObject.Bitmap = null;

// clean up
myBitmap.Recycle();
myBitmap.Dispose();
myBitmap = null;
```

This can be seen when updating the picture inside an `ImageView`:

```csharp
// clean old image
var oldImage = imageView.Drawable as BitmapDrawable;
if (oldImage != null && oldImage.Bitmap != null)
{
    imageView.SetImageBitmap(null);

    var oldBitmap = oldImage.Bitmap;
    oldBitmap.Recycle();
    oldBitmap.Dispose();
    oldBitmap = null;

    System.GC.Collect();
}

// set new image
imageView.SetImageBitmap(LoadBitmap());
```

More information can be found in the docs:
 - https://developer.xamarin.com/guides/android/advanced_topics/garbage_collection/#Helping_the_GC

Bugzilla:
 - https://bugzilla.xamarin.com/show_bug.cgi?id=3024
