using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ModPacker.Localization;
using ModPacker.UI;

namespace ModPacker;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        
        
        if(!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data")))
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data"));

        if (!File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data\\langs.json")))
        {
            Console.WriteLine("Creating langs.json");
            LocalizationHealer.CreateDefaultConfiguration();
        }
        else
        {
            Console.WriteLine("Loading langs.json");
            LocalizeManager.GetInstance.LoadConfiguration();
        }

        if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dist")))
        {
            Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dist"));
        }
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}