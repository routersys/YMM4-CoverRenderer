using YukkuriMovieMaker.Plugin.Shape;

namespace CoverArt;

public class CoverArtPlugin : IShapePlugin
{
    public string Name => Texts.PluginName;

    public bool IsExoShapeSupported => false;

    public bool IsExoMaskSupported => false;

    public IShapeParameter CreateShapeParameter(YukkuriMovieMaker.Project.SharedDataStore? sharedData)
    {
        return new CoverArtParameter(sharedData);
    }
}
