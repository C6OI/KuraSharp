using System;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace KuraSharp.Extensions; 

public static class WindowExtensions {
    public static Background RandomBackground() {
        IAssetLoader assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!; 
        
        string[] backgrounds = {
            @"avares://KuraSharp/Assets/Background_1.png",
            @"avares://KuraSharp/Assets/Background_2.png",
            @"avares://KuraSharp/Assets/Background_3.png"
        };

        Random random = new();
        string bgUri = backgrounds[random.Next(0, backgrounds.Length)];
            
        ImageBrush brush = new(new Bitmap(assets.Open(new Uri(bgUri)))) {
            Stretch = Stretch.Fill
        };

        Background background = new(brush, Path.GetFileName(bgUri));

        return background;
    }
}

public class Background {
    public Background(IBrush brush, string brushName) {
        Brush = brush;
        BrushName = brushName;
    }
    
    public IBrush Brush { get; }
    public string BrushName { get; }
}
