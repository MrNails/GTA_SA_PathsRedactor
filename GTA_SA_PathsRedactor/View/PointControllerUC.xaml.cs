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

namespace GTA_SA_PathsRedactor.View
{
    /// <summary>
    /// Логика взаимодействия для PointControllerUC.xaml
    /// </summary>
    public partial class PointControllerUC : UserControl
    {
        ViewModel.PathEditor m_pathEditor;

        public PointControllerUC() : this(new ViewModel.PathEditor("New path"))
        {}
        public PointControllerUC(ViewModel.PathEditor pathEditor)
        {
            InitializeComponent();

            PathEditor = pathEditor;
            PathColor.SelectedColor = pathEditor.Color.Color;

            DataContext = PathEditor;
        }

        public ViewModel.PathEditor PathEditor
        {
            get { return m_pathEditor; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_pathEditor = value;
                DataContext = value;
            }
        }

        private void PathColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            if (e.NewValue.HasValue)
            {
                m_pathEditor.Color.Color = e.NewValue.Value;
            }
        }

        private void ClearMapButton_Click(object sender, RoutedEventArgs e)
        {
            m_pathEditor.Clear();
        }
    }
}
