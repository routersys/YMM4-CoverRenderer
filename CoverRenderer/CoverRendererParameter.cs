using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Shape;
using YukkuriMovieMaker.Project;

namespace CoverRenderer;

public class CoverRendererParameter : ShapeParameterBase
{
    private string _filePath = string.Empty;

    [Display(Name = nameof(Texts.FilePath), Description = nameof(Texts.FilePathDescription), ResourceType = typeof(Texts))]
    [CoverFileSelector]
    public string FilePath
    {
        get => _filePath;
        set => Set(ref _filePath, value);
    }

    [Display(Name = nameof(Texts.Zoom), ResourceType = typeof(Texts))]
    [AnimationSlider("F1", "%", 0, 200)]
    public Animation Zoom { get; } = new Animation(100, 0, 5000);

    public CoverRendererParameter(SharedDataStore? sharedData) : base(sharedData)
    {
    }

    public CoverRendererParameter() : this(null)
    {
    }

    public override IEnumerable<string> CreateMaskExoFilter(int keyFrameIndex, ExoOutputDescription exoOutputDescription, ShapeMaskExoOutputDescription shapeMaskExoOutputDescription)
    {
        return Enumerable.Empty<string>();
    }

    public override IEnumerable<string> CreateShapeItemExoFilter(int keyFrameIndex, ExoOutputDescription exoOutputDescription)
    {
        return Enumerable.Empty<string>();
    }

    public override IShapeSource CreateShapeSource(IGraphicsDevicesAndContext devices)
    {
        return new CoverRendererSource(devices, this);
    }

    protected override IEnumerable<IAnimatable> GetAnimatables()
    {
        yield return Zoom;
    }

    protected override void LoadSharedData(SharedDataStore store)
    {
        var sharedData = store.Load<SharedData>();
        if (sharedData is null)
            return;
        sharedData.CopyTo(this);
    }

    protected override void SaveSharedData(SharedDataStore store)
    {
        store.Save(new SharedData(this));
    }

    class SharedData
    {
        public Animation Zoom { get; } = new Animation(100, 0, 5000);
        public string FilePath { get; set; } = string.Empty;

        public SharedData(CoverRendererParameter param)
        {
            Zoom.CopyFrom(param.Zoom);
            FilePath = param.FilePath;
        }

        public void CopyTo(CoverRendererParameter param)
        {
            param.Zoom.CopyFrom(Zoom);
            param.FilePath = FilePath;
        }
    }
}
