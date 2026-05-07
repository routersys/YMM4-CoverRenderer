using System.Windows;
using System.Windows.Data;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Views.Converters;

namespace CoverRenderer;

[AttributeUsage(AttributeTargets.Property)]
public sealed class CoverFileSelectorAttribute : PropertyEditorAttribute2
{
    public override FrameworkElement Create()
    {
        var control = new CoverSelector();
        return control;
    }

    public override void SetBindings(FrameworkElement control, ItemProperty[] itemProperties)
    {
        if (control is not CoverSelector selector) return;
        selector.SetBinding(
            CoverSelector.FilePathProperty,
            ItemPropertiesBinding.Create2(itemProperties));
    }

    public override void ClearBindings(FrameworkElement control)
    {
        if (control is not CoverSelector selector) return;
        BindingOperations.ClearBinding(selector, CoverSelector.FilePathProperty);
    }
}
