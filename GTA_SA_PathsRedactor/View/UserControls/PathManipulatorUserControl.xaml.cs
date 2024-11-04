using System.Windows;
using System.Windows.Controls;

namespace GTA_SA_PathsRedactor.View.UserControls;

public partial class PathManipulatorUserControl : UserControl
{
    public static readonly DependencyProperty ImagePathProperty = 
        DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(PathManipulatorUserControl));
    
    public PathManipulatorUserControl()
    {
        InitializeComponent();
    }
    
    public string ImagePath
    {
        get => (string)GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }
}