using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CoverRenderer;

public sealed class CoverFileEntry : INotifyPropertyChanged
{
    public string FilePath { get; }
    public string FileName { get; }
    public bool IsNone { get; }

    private BitmapSource? _thumbnail;
    public BitmapSource? Thumbnail => _thumbnail;

    public CoverFileEntry(string filePath, bool isNone = false)
    {
        FilePath = filePath;
        IsNone = isNone;
        FileName = isNone ? string.Empty : Path.GetFileName(filePath);

        if (!isNone && !string.IsNullOrWhiteSpace(filePath))
            _ = LoadAsync(Dispatcher.CurrentDispatcher);
    }

    private async Task LoadAsync(Dispatcher dispatcher)
    {
        var src = await CoverArtLoader.LoadThumbnailSourceAsync(FilePath).ConfigureAwait(false);
        if (src is null) return;
        await dispatcher.InvokeAsync(() =>
        {
            _thumbnail = src;
            OnPropertyChanged(nameof(Thumbnail));
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
