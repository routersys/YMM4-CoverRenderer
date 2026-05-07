using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using YukkuriMovieMaker.Commons;

namespace CoverRenderer;

public partial class CoverSelector : UserControl, IPropertyEditorControl
{
    public static readonly DependencyProperty FilePathProperty =
        DependencyProperty.Register(
            nameof(FilePath),
            typeof(string),
            typeof(CoverSelector),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnFilePathChanged));

    public string FilePath
    {
        get => (string)GetValue(FilePathProperty);
        set => SetValue(FilePathProperty, value);
    }

    public event EventHandler? BeginEdit;
    public event EventHandler? EndEdit;

    private readonly CoverSelectorViewModel _viewModel;

    public CoverSelector()
    {
        InitializeComponent();
        _viewModel = new CoverSelectorViewModel();
        DataContext = _viewModel;
        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
    }

    private static void OnFilePathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var selector = (CoverSelector)d;
        var newPath = e.NewValue as string ?? string.Empty;
        if (selector._viewModel.FilePath != newPath)
        {
            selector._viewModel.FilePath = newPath;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CoverSelectorViewModel.FilePath))
        {
            if (FilePath != _viewModel.FilePath)
            {
                BeginEdit?.Invoke(this, EventArgs.Empty);
                SetCurrentValue(FilePathProperty, _viewModel.FilePath);
                BindingOperations.GetBindingExpression(this, FilePathProperty)?.UpdateSource();
                EndEdit?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
