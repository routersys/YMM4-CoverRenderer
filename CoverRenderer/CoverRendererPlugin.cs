using YukkuriMovieMaker.Plugin.Shape;

namespace CoverRenderer;

public class CoverRendererPlugin : IShapePlugin
{
    public string Name => Texts.PluginName;

    public bool IsExoShapeSupported => false;

    public bool IsExoMaskSupported => false;

    public IShapeParameter CreateShapeParameter(YukkuriMovieMaker.Project.SharedDataStore? sharedData)
    {
        return new CoverRendererParameter(sharedData);
    }
}
