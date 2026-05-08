using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Vortice.Direct2D1;
using Vortice.Mathematics;
using Windows.Storage;
using Windows.Storage.FileProperties;
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
            var source = Task.Run(() => LoadThumbnailSourceAsync(targetPath)).GetAwaiter().GetResult();
            if (source != null)
                _bitmap = CreateD2DBitmap(_devices.DeviceContext, source);
        }
        catch
        {
        }
    }

    private static async Task<BitmapSource?> LoadThumbnailSourceAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(path);

            using var musicThumb = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 512, ThumbnailOptions.UseCurrentScale);
            if (musicThumb != null)
                return CreateBitmapSource(musicThumb.AsStream());

            using var videoThumb = await file.GetThumbnailAsync(ThumbnailMode.VideosView, 512, ThumbnailOptions.UseCurrentScale);
            if (videoThumb != null)
                return CreateBitmapSource(videoThumb.AsStream());

            using var singleThumb = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 512, ThumbnailOptions.UseCurrentScale);
            return singleThumb is null ? null : CreateBitmapSource(singleThumb.AsStream());
        }
        catch
        {
            return null;
        }
    }

    private static BitmapSource CreateBitmapSource(Stream stream)
    {
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        var converted = new FormatConvertedBitmap(decoder.Frames[0], PixelFormats.Pbgra32, null, 0.0);
        converted.Freeze();
        return converted;
    }

    private static ID2D1Bitmap CreateD2DBitmap(ID2D1DeviceContext deviceContext, BitmapSource source)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var stride = width * 4;
        var byteLen = stride * height;

        var buffer = ArrayPool<byte>.Shared.Rent(byteLen);
        var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        try
        {
            source.CopyPixels(buffer, stride, 0);
            var props = new BitmapProperties1(
                new Vortice.DCommon.PixelFormat(Vortice.DXGI.Format.B8G8R8A8_UNorm, Vortice.DCommon.AlphaMode.Premultiplied),
                96f, 96f, BitmapOptions.None);
            return deviceContext.CreateBitmap(new SizeI(width, height), handle.AddrOfPinnedObject(), stride, props);
        }
        finally
        {
            handle.Free();
            ArrayPool<byte>.Shared.Return(buffer);
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
