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
    /// Логика взаимодействия для PointTransformationUC.xaml
    /// </summary>
    public partial class PointTransformationUC : UserControl
    {
        private ViewModel.PointTransformVM m_pointTransform;
        private ViewModel.PathEditor m_path;

        public PointTransformationUC()
        {
            InitializeComponent();

            m_pointTransform = new ViewModel.PointTransformVM();

            var pointsTransfromData = new Services.PointTransformationData[SettingsForResolutionCB.Items.Count];

            for (int i = 0; i < SettingsForResolutionCB.Items.Count; i++)
            {
                var comboBoxItem = SettingsForResolutionCB.Items[i] as ComboBoxItem;
                pointsTransfromData[i] = new Services.PointTransformationData();
                pointsTransfromData[i].PropertyChanged += TransformPropertyChanged;

                if (comboBoxItem != null)
                {
                    pointsTransfromData[i].TransformName = comboBoxItem.Content.ToString();
                }
            }

            m_pointTransform.AddNewPointTransformationData(pointsTransfromData);
            m_pointTransform.PropertyChanged += TransformPropertyChanged;

            this.DataContext = m_pointTransform;
        }

        private void TransformPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            m_path?.DrawScale(m_pointTransform.CurrentPointTransformData);
        }

        public System.Collections.ObjectModel.ReadOnlyCollection<Services.PointTransformationData> PointTransformationDatas
        {
            get
            {
                return m_pointTransform.PointTranformationDatas;
            }
        }

        public ViewModel.PathEditor EditablePath
        {
            get { return m_path; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                m_path = value;
                m_path.DrawScale(m_pointTransform.CurrentPointTransformData);
            }
        }

        public void AddGoToHomeCommand(Services.RelayCommand goToMainMenu)
        {
            if (goToMainMenu == null)
            {
                throw new ArgumentNullException("goToMainMenu");
            }

            m_pointTransform.GoToMainMenu = goToMainMenu;
        }
    }
}
