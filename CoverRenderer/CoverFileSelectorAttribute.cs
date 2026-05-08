using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Settings;
using YukkuriMovieMaker.Views.Converters;

namespace CoverArt;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CoverFileSelectorAttribute : PropertyEditorAttribute2
{
    const string FilterPattern = "*.mp3;*.m4a;*.wav;*.flac;*.ogg;*.aac;*.wma;*.opus";

    static string FilterName
    {
        get
        {
            var raw = Texts.FileFilter;
            var sep = raw.IndexOf('|');
            return sep >= 0 ? raw[..sep] : raw;
        }
    }

    public CoverFileSelectorAttribute()
    {
        PropertyEditorSize = PropertyEditorSize.FullWidth;
    }

    public override FrameworkElement Create()
    {
        return new FileSelector();
    }

    public override void SetBindings(FrameworkElement control, ItemProperty[] itemProperties)
    {
        var editor = (FileSelector)control;
        editor.FileType = FileType.None;
        editor.ShowThumbnail = true;
        editor.Filter = FilterPattern;
        editor.FilterName = FilterName;

        var currentItemProperty = itemProperties[0];
        var file = (string?)currentItemProperty.PropertyInfo.GetValue(currentItemProperty.PropertyOwner);
        editor.DirectoryPath = string.IsNullOrEmpty(file) ? null : Path.GetDirectoryName(file);

        var targetProperties = GetTargetProperties(itemProperties).ToArray();
        editor.SetBinding(FileSelector.ValueProperty, ItemPropertiesBinding.Create2(targetProperties));
    }

    public override void ClearBindings(FrameworkElement control)
    {
        BindingOperations.ClearBinding(control, FileSelector.ValueProperty);
    }

    static IEnumerable<ItemProperty> GetTargetProperties(ItemProperty[] itemProperties)
    {
        foreach (var itemProperty in itemProperties)
        {
            if (itemProperty.PropertyInfo.GetCustomAttribute<CoverFileSelectorAttribute>() is null)
                continue;
            yield return itemProperty;
        }
    }
}
