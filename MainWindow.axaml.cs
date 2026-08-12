using System;
using SkiaSharp;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Tmds.DBus.Protocol;

namespace SepiaSharp;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private Bitmap changeImageToSepia(Stream originalstream)
    {
        using var imageColorMap = SKBitmap.Decode(originalstream);
        var sepiaMap = new SKBitmap(imageColorMap.Width, imageColorMap.Height);
        var canvas = new SKCanvas(sepiaMap);

        float[] sepiaMapMatrix = new float[]
        {
            0.393f, 0.769f, 0.189f, 0.0f, 0.0f, 
            0.349f, 0.686f, 0.168f, 0.0f, 0.0f, 
            0.272f, 0.534f, 0.131f, 0.0f, 0.0f, 
            0.0f,   0.0f,   0.0f,   1.0f, 0.0f 
        };
        var sepiaPaint = new SKPaint();
        sepiaPaint.ColorFilter = SKColorFilter.CreateColorMatrix(sepiaMapMatrix);

        canvas.DrawBitmap(sepiaMap, 0, 0, sepiaPaint);

        using var image = SKImage.FromBitmap(sepiaMap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        var memoryImage = new MemoryStream();
        data.SaveTo(memoryImage);

        memoryImage.Position = 0;

        return new Bitmap(memoryImage);
    }
    private Bitmap changeitToInverted(Stream originalPhoto)
    {
        using var imageMap = SKBitmap.Decode(originalPhoto);
        var invertMap = new SKBitmap(imageMap.Width, imageMap.Height);
        var canvas = new SKCanvas(invertMap);
        float[] invertMapMatrix = new float[]
        {
            -0.393f, -0.769f, -0.189f, 0.0f, 0.0f, 
            -0.349f, -0.686f, -0.168f, 0.0f, 0.0f, 
            -0.272f, -0.534f, -0.131f, 0.0f, 0.0f, 
            -0.0f,   -0.0f,   -0.0f,   1.0f, 0.0f 
        }; 
        var invertPaint = new SKPaint();
        invertPaint.ColorFilter = SKColorFilter.CreateColorMatrix(invertMapMatrix);
        canvas.DrawBitmap(invertMap, 0, 0, invertPaint);
        var memoryImage = new MemoryStream();
        return new Bitmap(memoryImage);
    }
    private Bitmap greyScale(Stream originalImage)
    {
        using var greyMap = SKBitmap.Decode(originalImage);
        var anotherGreyMap = new SKBitmap(greyMap.Width, greyMap.Height);
        var canvas = new SKCanvas(anotherGreyMap);
        float[] graycolormap = new float[]
        {
            0.2126f, 0.7152f, 0.0722f, 0.0f, 0.0f, 
            0.2126f, 0.7152f, 0.0722f, 0.0f, 0.0f, 
            0.2126f, 0.7152f, 0.0722f, 0.0f, 0.0f, 
            0.0f,    0.0f,    0.0f,    1.0f, 0.0f  
        };
        var SKGreyPaint = new SKPaint();
        SKGreyPaint.ColorFilter = SKColorFilter.CreateColorMatrix(graycolormap);
        canvas.DrawBitmap(greyMap, 0, 0, SKGreyPaint);
        var memoryImage = new MemoryStream();
        return new Bitmap(memoryImage);
    }
    private Bitmap saturate(Stream originalImage)
    {
        using var greyMap = SKBitmap.Decode(originalImage);
        var anotherGreyMap = new SKBitmap(greyMap.Width, greyMap.Height);
        var canvas = new SKCanvas(anotherGreyMap);
        float[] saturatedMatrix = new float[]
        {
            1.5f,  -0.3f, -0.2f,  0.0f, 0.0f,
            -0.1f,  1.4f, -0.3f,  0.0f, 0.0f,  
            -0.1f, -0.2f,  1.3f,  0.0f, 0.0f,  
            0.0f,   0.0f,  0.0f,  1.0f, 0.0f   
        };
        var SKGreyPaint = new SKPaint();
        SKGreyPaint.ColorFilter = SKColorFilter.CreateColorMatrix(saturatedMatrix);
        canvas.DrawBitmap(greyMap, 0, 0, SKGreyPaint);
        var memoryImage = new MemoryStream();
        return new Bitmap(memoryImage);
    }
    public async void LoadPhotos(object sender, RoutedEventArgs e)
    {
        var topPhoto = TopLevel.GetTopLevel(this);

        if(topPhoto == null) return;
        // Dialog
        var photoDialog = await topPhoto.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Select a photo!",
            AllowMultiple = false,
            FileTypeFilter = new[] {FilePickerFileTypes.ImageAll}
        });

        if(photoDialog.Count > 0)
        {
            try {
            // first this will read the photo the chose
            await using var stream = await photoDialog[0].OpenReadAsync();
            // they create a bitmap
            var photoBitMap = new Bitmap(stream);
            // and then display it
            OutputPanel.Source = photoBitMap;
            } catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
        }
    }

    public async void SavePhoto(object reader, RoutedEventArgs e)
    {
        var topImage = TopLevel.GetTopLevel(this);

        if(topImage == null) return;

        var photoDialog = await topImage.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Select a photo to Save"
        });
        if(photoDialog != null)
        {
            try
            {
                await using var imageStream = await photoDialog.OpenWriteAsync();
                var imageMap = new Bitmap(imageStream);
                imageMap.Save(imageStream);  
            } catch(Exception ex)
            {
                Console.WriteLine("Unable to load");
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            
        }
    }

    public async void callSpeia(object sender, RoutedEventArgs e)
    {
         var topPhoto = TopLevel.GetTopLevel(this);

        if(topPhoto == null) return;
        // Dialog
        var photoDialog = await topPhoto.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Select a photo!",
            AllowMultiple = false,
            FileTypeFilter = new[] {FilePickerFileTypes.ImageAll}
        });

        if(photoDialog.Count > 0)
        {
            await using var photoStream = await photoDialog[0].OpenReadAsync();
            if(photoStream.CanSeek)
            {
                photoStream.Seek(0, SeekOrigin.Begin);
            }
            var applySepia = changeImageToSepia(photoStream);
            if(OutputPanel != null)
            {
                OutputPanel.Source = applySepia;
            }
        }
    }
    public async void callInvert(object sender, RoutedEventArgs e)
    {
         var topPhoto = TopLevel.GetTopLevel(this);

        if(topPhoto == null) return;
        // Dialog
        var photoDialog = await topPhoto.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Select a photo!",
            AllowMultiple = false,
            FileTypeFilter = new[] {FilePickerFileTypes.ImageAll}
        });

        if(photoDialog.Count > 0)
        {
            await using var photoStream = await photoDialog[0].OpenReadAsync();
            var applySepia = changeitToInverted(photoStream);
            OutputPanel.Source = applySepia;
        }
    }

    public async void callGrey(object sender, RoutedEventArgs e)
    {
         var topPhoto = TopLevel.GetTopLevel(this);

        if(topPhoto == null) return;
        // Dialog
        var photoDialog = await topPhoto.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Select a photo!",
            AllowMultiple = false,
            FileTypeFilter = new[] {FilePickerFileTypes.ImageAll}
        });

        if(photoDialog.Count > 0)
        {
            await using var photoStream = await photoDialog[0].OpenReadAsync();
            var applySepia = greyScale(photoStream);
            OutputPanel.Source = applySepia;
        }
    }
    public async void callSaturate(object sender, RoutedEventArgs e)
    {
         var topPhoto = TopLevel.GetTopLevel(this);

        if(topPhoto == null) return;
        // Dialog
        var photoDialog = await topPhoto.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
        {
            Title = "Select a photo!",
            AllowMultiple = false,
            FileTypeFilter = new[] {FilePickerFileTypes.ImageAll}
        });

        if(photoDialog.Count > 0)
        {
            await using var photoStream = await photoDialog[0].OpenReadAsync();
            var applySepia = saturate(photoStream);
            OutputPanel.Source = applySepia;
        }
    }
}