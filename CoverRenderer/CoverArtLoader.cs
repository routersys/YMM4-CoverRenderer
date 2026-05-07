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

namespace CoverRenderer;

public static class CoverArtLoader
{
    public static async Task<BitmapSource?> LoadThumbnailSourceAsync(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            return null;

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(path);

            using var musicThumb = await file.GetThumbnailAsync(ThumbnailMode.MusicView, 512, ThumbnailOptions.UseCurrentScale);
            if (musicThumb != null)
                return CreateSource(musicThumb.AsStream());

            using var videoThumb = await file.GetThumbnailAsync(ThumbnailMode.VideosView, 512, ThumbnailOptions.UseCurrentScale);
            if (videoThumb != null)
                return CreateSource(videoThumb.AsStream());

            using var singleThumb = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 512, ThumbnailOptions.UseCurrentScale);
            return singleThumb is null ? null : CreateSource(singleThumb.AsStream());
        }
        catch
        {
            return null;
        }
    }

    private static BitmapSource CreateSource(Stream stream)
    {
        var decoder = BitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        var converted = new FormatConvertedBitmap(decoder.Frames[0], PixelFormats.Pbgra32, null, 0.0);
        converted.Freeze();
        return converted;
    }

    public static ID2D1Bitmap? CreateD2DBitmap(ID2D1DeviceContext deviceContext, BitmapSource source)
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
}
