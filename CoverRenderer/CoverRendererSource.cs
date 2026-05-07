using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace CoverRenderer;

public class CoverRendererSource : IShapeSource
{
    private const float CanonicalSize = 512f;

    private readonly IGraphicsDevicesAndContext _devices;
    private readonly CoverRendererParameter _parameter;

    private string _currentFilePath = string.Empty;
    private ID2D1Bitmap? _bitmap;
    private ID2D1CommandList? _commandList;
    private bool _disposedValue;

    public ID2D1Image Output => _commandList ?? throw new InvalidOperationException();

    public CoverRendererSource(IGraphicsDevicesAndContext devices, CoverRendererParameter parameter)
    {
        _devices = devices;
        _parameter = parameter;
    }

    public void Update(TimelineItemSourceDescription timelineItemSourceDescription)
    {
        var fps = timelineItemSourceDescription.FPS;
        var frame = timelineItemSourceDescription.ItemPosition.Frame;
        var length = timelineItemSourceDescription.ItemDuration.Frame;
        var zoom = (float)_parameter.Zoom.GetValue(frame, length, fps) / 100f;

        UpdateBitmapIfPathChanged();

        var dc = _devices.DeviceContext;

        _commandList?.Dispose();
        _commandList = dc.CreateCommandList();

        dc.Target = _commandList;
        dc.BeginDraw();
        dc.Clear(null);

        if (_bitmap != null)
        {
            var half = CanonicalSize / 2f * zoom;
            dc.DrawBitmap(_bitmap, new Vortice.RawRectF(-half, -half, half, half), 1.0f, BitmapInterpolationMode.Linear, null);
        }

        dc.EndDraw();
        dc.Target = null;
        _commandList.Close();
    }

    private void UpdateBitmapIfPathChanged()
    {
        var targetPath = _parameter.FilePath;
        if (string.Equals(_currentFilePath, targetPath, StringComparison.OrdinalIgnoreCase))
            return;

        _currentFilePath = targetPath;
        _bitmap?.Dispose();
        _bitmap = null;

        if (string.IsNullOrWhiteSpace(targetPath))
            return;

        try
        {
            var source = Task.Run(() => CoverArtLoader.LoadThumbnailSourceAsync(targetPath)).GetAwaiter().GetResult();
            if (source != null)
            {
                _bitmap = CoverArtLoader.CreateD2DBitmap(_devices.DeviceContext, source);
            }
        }
        catch
        {
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _commandList?.Dispose();
                _bitmap?.Dispose();
            }
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
