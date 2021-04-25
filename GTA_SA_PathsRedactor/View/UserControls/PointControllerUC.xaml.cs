using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GTA_SA_PathsRedactor.Services;

namespace GTA_SA_PathsRedactor.View
{
    /// <summary>
    /// Логика взаимодействия для PointControllerUC.xaml
    /// </summary>
    public partial class PointControllerUC : UserControl
    {
        private ViewModel.PathVM m_pathVM;

        public PointControllerUC() : this(new ViewModel.PathVM())
        {}
        public PointControllerUC(ViewModel.PathVM pathVM)
        {
            InitializeComponent();

            PathVM = pathVM;

            if (pathVM.CurrentPath != null) 
            {
                PathColorCP.SelectedColor = pathVM.CurrentPath.Color.Color;
            }
            else
            {
                PathColorCP.SelectedColor = Colors.Transparent;
            }

            DataContext = PathVM;

            PathVM.PropertyChanged += PathVM_PropertyChanged;
        }

        private void PathVM_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPathIndex" && m_pathVM.CurrentPathIndex != -1)
            {
                PathColorCP.SelectedColor = m_pathVM.CurrentPath.Color.Color;
            }
        }

        public ViewModel.PathVM PathVM
        {
            get { return m_pathVM; }
            set
            {
                if (m_pathVM != null)
                {
                    m_pathVM.PathSelected -= ChangeSelection_PathSelected;
                }

                m_pathVM = value;
                DataContext = value;

                m_pathVM.PathSelected += ChangeSelection_PathSelected;
            }
        }

        private void ChangeSelection_PathSelected(ViewModel.PathVM sender, Services.PathSelectionArgs e)
        {
            AvailablePathsListBox.UpdateLayout();

            if (sender.Paths.Count == 0 )
            {
                return;
            }

            if (e.OldIndex != -1 && e.OldIndex < sender.Paths.Count)
            {
                sender.Paths[e.OldIndex].WorkField.Visibility = Visibility.Hidden;

                var oldItem = (ListBoxItem)AvailablePathsListBox.ItemContainerGenerator.ContainerFromIndex(e.OldIndex);
                var oldItemContentPresenter = oldItem.FindVisualChild<ContentPresenter>();

                if (oldItemContentPresenter == null)
                {
                    return;
                }

                var oldTextBlock = (TextBlock)oldItemContentPresenter.ContentTemplate.FindName("SelectionStarTextBlock", oldItemContentPresenter);

                oldTextBlock.Text = "";
            }
            if (e.NewIndex != -1 && e.NewIndex < sender.Paths.Count)
            {
                sender.Paths[e.NewIndex].WorkField.Visibility = Visibility.Visible;

                var newItem = (ListBoxItem)AvailablePathsListBox.ItemContainerGenerator.ContainerFromIndex(e.NewIndex);
                var newItemContentPresenter = newItem.FindVisualChild<ContentPresenter>();

                if (newItemContentPresenter == null)
                {
                    return;
                }

                var newTextBlock = (TextBlock)newItemContentPresenter.ContentTemplate.FindName("SelectionStarTextBlock", newItemContentPresenter);

                newTextBlock.Text = "*";
            }
        }

        private void PathColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue && m_pathVM.CurrentPathIndex != -1)
            {
                m_pathVM.CurrentPath.Color.Color = e.NewValue.Value;
            }
        }

        private void ClearMapButton_Click(object sender, RoutedEventArgs e)
        {
            m_pathVM.CurrentPath.Clear();
        }
    }
}
