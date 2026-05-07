using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Win32;
using YukkuriMovieMaker.Commons;

namespace CoverRenderer;

public sealed class CoverSelectorViewModel : INotifyPropertyChanged
{
    private static readonly HashSet<string> MediaExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".mp4", ".mp3", ".wav", ".m4a", ".avi", ".mkv", ".mov", ".wmv", ".webm", ".flv",
        ".flac", ".ogg", ".aac", ".wma", ".opus",
        ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff", ".webp"
    };

    private string _currentDirectory = string.Empty;
    private bool _suppressSync;

    public ObservableCollection<CoverFileEntry> Files { get; } = new ObservableCollection<CoverFileEntry>();

    public CoverSelectorViewModel()
    {
        BrowseCommand = new ActionCommand(_ => true, _ => Browse());
        RefreshFiles();
    }

    private CoverFileEntry? _selectedFile;
    public CoverFileEntry? SelectedFile
    {
        get => _selectedFile;
        set
        {
            if (ReferenceEquals(_selectedFile, value)) return;
            _selectedFile = value;
            OnPropertyChanged();
            if (!_suppressSync)
            {
                FilePath = value?.IsNone == true ? string.Empty : (value?.FilePath ?? string.Empty);
            }
        }
    }

    private string _filePath = string.Empty;
    public string FilePath
    {
        get => _filePath;
        set
        {
            if (_filePath == value) return;
            _filePath = value ?? string.Empty;
            OnPropertyChanged();
            OnFilePathChanged();
        }
    }

    public ICommand BrowseCommand { get; }

    private void Browse()
    {
        var dialog = new OpenFileDialog
        {
            Filter = Texts.FileFilter,
            InitialDirectory = string.IsNullOrWhiteSpace(_currentDirectory)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : _currentDirectory
        };
        if (dialog.ShowDialog() == true)
            FilePath = dialog.FileName;
    }

    private void OnFilePathChanged()
    {
        var dir = string.IsNullOrWhiteSpace(FilePath) ? string.Empty : Path.GetDirectoryName(FilePath) ?? string.Empty;

        if (!string.Equals(dir, _currentDirectory, StringComparison.OrdinalIgnoreCase))
        {
            _currentDirectory = dir;
            RefreshFiles();
        }

        SyncSelection();
    }

    private void RefreshFiles()
    {
        _suppressSync = true;
        try
        {
            Files.Clear();
            Files.Add(new CoverFileEntry(string.Empty, true));

            if (!string.IsNullOrWhiteSpace(_currentDirectory) && Directory.Exists(_currentDirectory))
            {
                try
                {
                    var mediaFiles = Directory.GetFiles(_currentDirectory)
                        .Where(f => MediaExtensions.Contains(Path.GetExtension(f)))
                        .OrderBy(f => f)
                        .Select(f => new CoverFileEntry(f));

                    foreach (var f in mediaFiles)
                        Files.Add(f);
                }
                catch
                {
                }
            }
        }
        finally
        {
            _suppressSync = false;
        }
        SyncSelection();
    }

    private void SyncSelection()
    {
        _suppressSync = true;
        try
        {
            if (string.IsNullOrWhiteSpace(FilePath))
            {
                SelectedFile = Files.FirstOrDefault(f => f.IsNone);
                return;
            }

            var entry = Files.FirstOrDefault(f => string.Equals(f.FilePath, FilePath, StringComparison.OrdinalIgnoreCase));
            if (entry is null)
            {
                entry = new CoverFileEntry(FilePath);
                Files.Add(entry);
            }
            SelectedFile = entry;
        }
        finally
        {
            _suppressSync = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
