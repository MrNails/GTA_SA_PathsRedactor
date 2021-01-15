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
        private ViewModel.PointTransformVM pointTransform;

        public PointTransformationUC()
        {
            InitializeComponent();

            pointTransform = new ViewModel.PointTransformVM();

            var pointsTransfromData = new Services.PointTransformationData[SettingsForResolutionCB.Items.Count];

            for (int i = 0; i < SettingsForResolutionCB.Items.Count; i++)
            {
                var comboBoxItem = SettingsForResolutionCB.Items[i] as ComboBoxItem;

                if (comboBoxItem != null)
                {
                    pointsTransfromData[i] = new Services.PointTransformationData(0, 0, 0, 0, 0, 0,
                                                                                  comboBoxItem.Content.ToString());
                }
                else
                {
                    pointsTransfromData[i] = new Services.PointTransformationData();
                }
                
            }

            pointTransform.AddNewPointTransformationData(pointsTransfromData);

            this.DataContext = pointTransform;
        }
    }
}
