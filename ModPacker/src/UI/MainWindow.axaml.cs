using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using ModPacker.Localization;

namespace ModPacker.UI;

public partial class MainWindow : Window
{

    private string? ModFolderPath;
    private string? DestZipName;
    private string? DescriptorFilePath;
    private readonly Random _random = new();
    
    public MainWindow()
    {
        InitializeComponent();
        ModRenameCombobox.Tag = "KeyWord=ASIS";
        LangComboBox.SelectedIndex = LocalizeManager.GetInstance.currentLangID;
        Console.WriteLine(LocalizeManager.GetInstance.currentLangID);
        LocalizeForm();
        
        HeadPanel.PointerPressed += (s, e) =>
        {
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            {
                this.BeginMoveDrag(e);
            }
        };
    }
    
    #region Localize

    void LocalizeForm(int? langID = null)
    {
        if (langID == null)
            langID = LocalizeManager.GetInstance.currentLangID;
        
        List<Control> controls = GetControlsWithTagPrefix("KeyWord=");
        foreach (var control in controls)
        {
            if (control.Tag is string tag)
            {
                if (tag.StartsWith("KeyWord="))
                {
                    string localizedKey =
                        tag.Split('=')[1].Translate(langID ?? 1); //Это моё расширение(extension) для строк, смотрет в LocalizeExtensions

                    switch (control) //Для разных типов контролов переводим разные вещи
                    {
                        case Button btn:
                            btn.Content = localizedKey;
                            break;
                        case TextBlock textBlock:
                            textBlock.Text = localizedKey;
                            break;
                        case TextBox textBox:
                            textBox.Watermark = localizedKey;
                            break;
                        case ComboBoxItem comboBoxItem:
                            comboBoxItem.Content = localizedKey;
                            break;
                        case ComboBox comboBox:
                            comboBox.Text = localizedKey;
                            break;
                    } 
                }
            }
        }
    }
    
    public List<Control> GetControlsWithTagPrefix(string prefix)
    {
        return GetAllControls(this)
            .Where(c => c.Tag?.ToString().StartsWith(prefix) == true)
            .ToList();
    }

    private IEnumerable<Control> GetAllControls(Control parent)
    {
        if (parent == null) yield break;
    
        yield return parent;
    
        if (parent is Panel panel)
        {
            foreach (var child in panel.Children.OfType<Control>())
            {
                foreach (var subChild in GetAllControls(child))
                {
                    yield return subChild;
                }
            }
        }
        else if (parent is ContentControl contentControl && contentControl.Content is Control child)
        {
            foreach (var subChild in GetAllControls(child))
            {
                yield return subChild;
            }
        }
        else if (parent is ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items.OfType<Control>())
            {
                foreach (var subChild in GetAllControls(item))
                {
                    yield return subChild;
                }
            }
        }
        else if (parent is Decorator decorator && decorator.Child is Control decoratedChild)
        {
            foreach (var subChild in GetAllControls(decoratedChild))
            {
                yield return subChild;
            }
        }
    }
    
    #endregion


    async void WindowOnLoaded(object? sender, RoutedEventArgs e)
    {
        for (Opacity = 0; Opacity < 1; Opacity += .2) await Task.Delay(20);
    }

    private void OnMinimize_Click(object? sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void OnClose_Click(object? sender, RoutedEventArgs e)
    {
        LocalizeManager.GetInstance.SaveConfiguration();
        Close();
    }

    async void BrowseModFolder_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);

        var folders = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = LocalizeManager.GetInstance.GetTranslation("SELECTMODFOLDER"),
            AllowMultiple = false
        });

        if (folders.Count >= 1)
        {
            string folderPath = folders[0].Path.AbsolutePath.Replace("%20", " ");
            if (CheckValidMod(folderPath))
            {
                string? modName = GetNameFromDescriptor();
                
                if(modName != null)
                    NewModNameTextBox.Text = modName;
                else
                    SetStatus("Invalid descriptor, there is no name of mod");
                
                ModFolderTextBox.Text = folderPath; // %20 это пробел еси чо

                DestZipName = Path.GetDirectoryName(folderPath).Replace(" ", "").Split('\\').Last() + ".zip";
                DestZipNameTextBox.Text = DestZipName;
                NewDotModFileName.Text = DestZipName.Replace(".zip", ".mod");

                ModFolderPath = folderPath;
            }
            else
            {
                SetStatus("Invalid mod folder, it doesn't contain descriptor.mod");
            }
        }
    }

    bool CheckValidMod(string modPath)
    {
        if (String.IsNullOrEmpty(modPath))
            return false;
        
        foreach (var file in Directory.GetFiles(modPath))
        {
            if (file.ToLower().Contains("descriptor.mod"))
            {
                DescriptorFilePath = file;
                return true;
            }
        }

        return false;
    }

    private string? GetNameFromDescriptor()
    {
        string[] lines = File.ReadAllLines(DescriptorFilePath);
        
        foreach (var line in lines)
        {
            if (line.StartsWith("name=\""))
            {
                string modName = line.Replace("name=", "").Replace("\"", "").Trim();
                return modName;
            }
        }

        return null;
    }

    private async Task PackMod()
    {
        if(!CheckValidMod(ModFolderTextBox.Text))
        {
            SetStatus("Invalid mod folder, it doesn't contain descriptor.mod");
            return;
        }
        string? modName = GetNameFromDescriptor();

        if (modName != null)
        {
            if(ModRenameCombobox.SelectedIndex == 0)
                NewModNameTextBox.Text = modName;
        }
        else
        {
            SetStatus("Invalid descriptor, there is no name of mod");
            return;
        }

        if (String.IsNullOrEmpty(NewDotModFileName.Text.Trim()) || !NewDotModFileName.Text.EndsWith(".mod"))
        {
            SetStatus("Invalid .mod file, change it");
            return;
        }

        if (ModRenameCombobox.SelectedIndex == 1 && String.IsNullOrEmpty(NewModNameTextBox.Text.Trim()))
        {
            SetStatus("Invalid new mod name, change it");
            return;
        }
        
        

        string destinationModPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"Dist\{NewDotModFileName.Text}");
        //Console.WriteLine(destinationModPath);
        //Console.WriteLine(ModFolderPath);
        string modFolderName = Path.GetDirectoryName(ModFolderPath).Split('\\').Last();
        
        string distPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dist");
        
        if(File.Exists(Path.Combine(distPath, DestZipName)))
            File.Delete(Path.Combine(distPath, DestZipName));
        
        File.Copy(DescriptorFilePath, destinationModPath);
        File.AppendAllLines(destinationModPath, new[] { $"\npath=\"mod/{modFolderName}\"" });

        UpdateModNameInFile(destinationModPath, NewModNameTextBox.Text?.Trim() ?? "");

        File.SetAttributes(destinationModPath, FileAttributes.ReadOnly);

        List<string> files = Directory.GetFiles(ModFolderPath.Replace('/', '\\'), "*.*", SearchOption.AllDirectories).ToList();
        PackingProgressBar.Minimum = 0;
        PackingProgressBar.Maximum = files.Count;
        PackingProgressBar.Value = 0;

        files.ForEach(x => Console.WriteLine(x));
        
        await CopyFilesRecursivelyAsync(ModFolderPath.Replace('/', '\\'), Path.Combine(distPath, modFolderName) + "\\", files.ToArray());

        string sourceFolder = Path.Combine(distPath, modFolderName) + "\\";
        AddWatermark(sourceFolder);

        using (ZipArchive zip = ZipFile.Open(Path.Combine(distPath, DestZipName), ZipArchiveMode.Create))
        {
            string sourceFolderName = Path.Combine(distPath, modFolderName).Split('\\').Last();
            zip.CreateEntryFromFile(destinationModPath, Path.GetFileName(destinationModPath));
            foreach (string file in Directory.GetFiles(sourceFolder, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceFolder, file);
                zip.CreateEntryFromFile(file, Path.Combine(sourceFolderName, relativePath));
            }
        }
        
        Directory.Delete(Path.Combine(distPath, modFolderName) + "\\", true);
        try
        {
            File.SetAttributes(destinationModPath, FileAttributes.Normal);
            File.Delete(destinationModPath);
        }
        catch
        {
            SetStatus("Failed to delete .mod file, access denied. Delete manually");
        }

        OpenSelectedExplorer(Path.Combine(distPath, DestZipName));
    }
    
    private async Task CopyFilesRecursivelyAsync(string sourcePath, string targetPath, string[] files)
    {
        int renameMode = ModRenameCombobox.SelectedIndex;
        string newName = NewModNameTextBox.Text?.Trim() ?? "";

        await Task.Run(() =>
        {
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            int processedFiles = 0;
            foreach (string newPath in files)
            {
                string newFilePath = newPath.Replace(sourcePath, targetPath);

                File.Copy(newPath, newFilePath, true);
                
                if (newFilePath.ToLower().EndsWith("descriptor.mod"))
                {
                    if (renameMode == 1)
                    {
                        UpdateModNameInFile(newFilePath, newName);
                    }
                }

                processedFiles++;
                int currentProgress = processedFiles;
                Dispatcher.UIThread.Post(() =>
                {
                    PackingProgressBar.Value = currentProgress;
                });
            }
        });
    }

    private void UpdateModNameInFile(string dotModFilePath, string newName)
    {
        StringBuilder builder = new();
        foreach (var line in File.ReadAllLines(dotModFilePath))
        {
            if (!line.StartsWith("name="))
            {
                string newLine = line.Trim();
                builder.AppendLine(newLine);
            }
            else
            {
                builder.AppendLine($"name=\"{newName}\"");
            }
        }
        File.WriteAllText(dotModFilePath, builder.ToString());
    }

    private string? GetCommentPrefix(string filePath)
    {
        string ext = Path.GetExtension(filePath).ToLower();
        return ext switch
        {
            ".txt" or ".gui" or ".res" or ".mod" or ".asset" or ".gfx" or ".yml" or ".yaml" or ".settings" => "#",
            ".fxh" or ".fx" or ".shader" => "//",
            ".lua" => "--",
            ".csv" => "#", // В HoI4 в CSV тоже часто комментят решеткой
            _ => null
        };
    }

    private void AddWatermark(string directoryPath)
    {
        try
        {
            var allFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
            // Выбираем только те файлы, для которых мы знаем тип комментария
            var targetFiles = allFiles.Where(f => GetCommentPrefix(f) != null).ToList();

            if (targetFiles.Count == 0) return;

            string selectedFile = targetFiles[_random.Next(targetFiles.Count)];
            string prefix = GetCommentPrefix(selectedFile)!;

            // Снимаем ReadOnly перед редактированием
            FileAttributes attributes = File.GetAttributes(selectedFile);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                File.SetAttributes(selectedFile, attributes & ~FileAttributes.ReadOnly);
            }

            File.AppendAllText(selectedFile, $"\n\n{prefix} Packed using ModPacker by CLR62");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Watermark error: {ex.Message}");
        }
    }

    private void SetStatus(string status)
    {
        StatusText.Text = status;
    }

    private void OnModRenameComboBox_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox comboBox)
        {
            if (NewModNameTextBox != null)
            {
                if (comboBox.SelectedIndex == 0)
                {
                    NewModNameTextBox.IsEnabled = false;
                    ModRenameCombobox.Tag = "KeyWord=ASIS";
                }
                else if (comboBox.SelectedIndex == 1)
                {
                    NewModNameTextBox.IsEnabled = true;
                    ModRenameCombobox.Tag = "KeyWord=CHANGETO";
                }
                
                LocalizeForm();
            }
        }
    }

    private async void PackButtonClick(object? sender, RoutedEventArgs e) => await PackMod();

    private void OpenSelectedExplorer(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo {FileName = "explorer", Arguments = $"/n, /select, {path}" });
        }
    }

    private void DestZipNameTextBox_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            if (!textBox.Text.EndsWith(".zip"))
            {
                PackButton.IsEnabled = false;
            }
            else
            {
                PackButton.IsEnabled = true;
                DestZipName =  textBox.Text;
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        LocalizeManager.GetInstance.SaveConfiguration();
        base.OnClosed(e);
    }

    private void OnSettingClick(object? sender, RoutedEventArgs e)
    {
        SettingsPanel.IsVisible = true;
    }

    void OnCloseSettingClick(object? sender, RoutedEventArgs e)
    {
        SettingsPanel.IsVisible = false;
    }

    private void OnApplySettingClick(object? sender, RoutedEventArgs e)
    {
        LocalizeManager.GetInstance.currentLangID = LangComboBox.SelectedIndex;
        LocalizeForm(LangComboBox.SelectedIndex);
    }
}