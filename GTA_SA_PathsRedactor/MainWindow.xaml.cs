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
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace GTA_SA_PathsRedactor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool m_pointMoveMode;
        private bool isLoaded;
        private bool m_mouseDown;

        private Rectangle m_selectionRectangle;
        private Key m_pressedKey;
        private Point m_oldMousePos;
        private Point m_oldConainerMousePos;
        private Point m_selectionRectangleOldMousePos;

        private Services.PointLoader m_pointLoader;
        private ViewModel.PathEditor m_pathEditor;

        private UserControl[] userControls;

        public MainWindow()
        {
            m_pathEditor = new ViewModel.PathEditor("TestPath");

            InitializeComponent();
            LoadImage();
            InitializeAdditionalComponent();

            m_pointMoveMode = false;
            isLoaded = false;
            m_mouseDown = false;

            MainField.Children.Add(m_pathEditor.WorkField);
        }

        private void InitializeAdditionalComponent()
        {
            var mainUC = new View.PointControllerUC(m_pathEditor);
            var pathSettingUc = new View.PointTransformationUC();

            mainUC.PathEditor = m_pathEditor;
            mainUC.VerticalAlignment = VerticalAlignment.Top;
            pathSettingUc.VerticalAlignment = VerticalAlignment.Top;
            pathSettingUc.EditablePath = m_pathEditor;
            pathSettingUc.AddGoToHomeCommand(new Services.RelayCommand(obj =>
            {
                UserContentContainer.Child = userControls[0];
            }));

            UserContentContainer.Child = mainUC;

            userControls = new UserControl[] { mainUC, pathSettingUc };

            TransformGroup tGroup = new TransformGroup();

            tGroup.Children.Add(new ScaleTransform());
            tGroup.Children.Add(new TranslateTransform());

            MainField.RenderTransform = tGroup;

            m_pathEditor.DotsMouseUp += ObecjctClicked_MouseDown;

            m_selectionRectangle = new Rectangle();
            m_selectionRectangle.Stroke = new SolidColorBrush(Colors.White);
            m_selectionRectangle.StrokeDashArray.Add(2);
            m_selectionRectangle.StrokeThickness = 1;
            m_selectionRectangle.RenderTransform = new ScaleTransform();
            MapContainer.Children.Add(m_selectionRectangle);
        }
        private void LoadImage()
        {
            string imgPath = Environment.CurrentDirectory + @"\Resource\MapImage.jpg";

            if (System.IO.File.Exists(imgPath))
            {
                MapIcon.Source = new BitmapImage(new Uri(imgPath));
            }
            else
            {
                MapIcon.Source = new BitmapImage(new Uri(@"/Resource/noimage.png", UriKind.Relative));
                MapIcon.Stretch = Stretch.None;
            }
        }

        private void ResetTranslate(TranslateTransform translateTransform)
        {
            translateTransform.X = 0;
            translateTransform.Y = 0;
        }
        private void ChangeMainFieldTranslate(double newX, double newY)
        {
            var tGroup = MainField.RenderTransform as TransformGroup;

            if (tGroup != null && tGroup.Children.Count != 0)
            {
                ScaleTransform sTransform = ((ScaleTransform)tGroup.Children[0]);
                TranslateTransform tTransform = ((TranslateTransform)tGroup.Children[1]);

                if (sTransform.ScaleX == 1 || sTransform.ScaleY == 1)
                {
                    ResetTranslate(tTransform);
                    return;
                }

                tTransform.X += newX;
                tTransform.Y += newY;

                double maxRightTranslate = MainField.ActualWidth - MainField.ActualWidth * sTransform.ScaleX;
                double maxUpTranslate = MainField.ActualHeight - MainField.ActualHeight * sTransform.ScaleY;

                if (tTransform.X <= maxRightTranslate)
                {
                    tTransform.X = maxRightTranslate;
                }
                if (tTransform.X >= 0)
                {
                    tTransform.X = 0;
                }

                if (tTransform.Y <= maxUpTranslate)
                {
                    tTransform.Y = maxUpTranslate;
                }
                if (tTransform.Y >= 0)
                {
                    tTransform.Y = 0;
                }
            }
        }
        
        private void ResetSelectionRectangle()
        {
            m_selectionRectangle.Width = 0;
            m_selectionRectangle.Height = 0;
            m_selectionRectangle.Visibility = Visibility.Collapsed;
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var tGroup = MainField.RenderTransform as TransformGroup;

            if (tGroup != null && tGroup.Children.Count != 0)
            {
                ScaleTransform sTransform = ((ScaleTransform)tGroup.Children[0]);
                TranslateTransform tTransform = ((TranslateTransform)tGroup.Children[1]);

                Point pos = e.GetPosition(MainField);

                sTransform.ScaleX += e.Delta / 1000.0;
                sTransform.ScaleY += e.Delta / 1000.0;

                tTransform.X = -pos.X * (sTransform.ScaleX - 1);
                tTransform.Y = -pos.Y * (sTransform.ScaleY - 1);
                ChangeMainFieldTranslate(0, 0);

                if (sTransform.ScaleX < 1 || sTransform.ScaleY < 1)
                {
                    sTransform.ScaleX = 1;
                    sTransform.ScaleY = 1;
                    ResetTranslate(tTransform);
                }
            }
        }
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftShift)
            {
                ResetSelectionRectangle();
            }

            m_pressedKey = Key.None;
            m_pointMoveMode = false;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            m_pressedKey = e.Key;
        }

        private void MainField_MouseDown(object sender, MouseButtonEventArgs e)
        {
            m_selectionRectangleOldMousePos = e.GetPosition(MapContainer);
            m_oldConainerMousePos = m_selectionRectangleOldMousePos;
            m_oldMousePos = e.GetPosition(MainField);

            if (m_pressedKey == Key.LeftCtrl)
            {
                m_pointMoveMode = true;
            }
            if (m_pressedKey == Key.LeftShift)
            {
                ResetSelectionRectangle();
                Canvas.SetTop(m_selectionRectangle, m_selectionRectangleOldMousePos.Y);
                Canvas.SetLeft(m_selectionRectangle, m_selectionRectangleOldMousePos.X);
                m_selectionRectangle.Visibility = Visibility.Visible;

                m_pathEditor.MultipleSelectionMode = true;
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                m_mouseDown = true;
            }
        }
        private void MainField_MouseMove(object sender, MouseEventArgs e)
        {
            if (!m_mouseDown)
            {
                return;
            }

            Point currentPos = e.GetPosition(MainField);
            Point currentContainerPos = e.GetPosition(MapContainer);

            if (m_pointMoveMode && m_pathEditor.CurrentObject != null)
            {
                m_pathEditor.CurrentObject.Point.X = currentPos.X;
                m_pathEditor.CurrentObject.Point.Y = currentPos.Y;
            }
            else if (!m_pointMoveMode && m_pressedKey == Key.LeftShift)
            {
                var newWidth = currentPos.X - m_oldMousePos.X;
                var newHeight = currentPos.Y - m_oldMousePos.Y;

                var scaleTransform = m_selectionRectangle.RenderTransform as ScaleTransform;

                if (newWidth < 0)
                    scaleTransform.ScaleX = -1;
                else
                    scaleTransform.ScaleX = 1;

                if (newHeight < 0)
                    scaleTransform.ScaleY = -1;
                else
                    scaleTransform.ScaleY = 1;

                var groupTransform = (TransformGroup)MainField.RenderTransform;
                var mainFieldScale = (ScaleTransform)groupTransform.Children[0];

                m_selectionRectangleOldMousePos = currentContainerPos;

                m_selectionRectangle.Width = Math.Abs(newWidth) * mainFieldScale.ScaleX;
                m_selectionRectangle.Height = Math.Abs(newHeight) * mainFieldScale.ScaleY;
            }
            else if (m_pressedKey == Key.LeftCtrl && m_pathEditor.SelectedDots.Count != 0)
            {
                double offsetX = currentContainerPos.X - m_selectionRectangleOldMousePos.X;
                double offsetY = currentContainerPos.Y - m_selectionRectangleOldMousePos.Y;

                m_selectionRectangleOldMousePos = currentContainerPos;

                Canvas.SetTop(m_selectionRectangle, Canvas.GetTop(m_selectionRectangle) + offsetY);
                Canvas.SetLeft(m_selectionRectangle, Canvas.GetLeft(m_selectionRectangle) + offsetX);
            }
            else
            {
                double offsetX = currentPos.X - m_oldMousePos.X;
                double offsetY = currentPos.Y - m_oldMousePos.Y;

                if (Math.Abs(offsetX) > 25 || Math.Abs(offsetY) > 25)
                {
                    m_oldMousePos = currentPos;
                }

                ChangeMainFieldTranslate(offsetX, offsetY);
            }
        }
        private void MainField_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_mouseDown = false;
        }
        private void MainField_MouseLeave(object sender, MouseEventArgs e)
        {
            m_mouseDown = false;
        }
        private void MainField_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                m_mouseDown = true;
            }
        }
        private void MapContainer_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var mainFieldSTransform = (ScaleTransform)((TransformGroup)MainField.RenderTransform).Children[0];

            if (m_pressedKey == Key.LeftCtrl && m_pathEditor.SelectedDots.Count != 0)
            {
                double offsetX = m_selectionRectangleOldMousePos.X - m_oldConainerMousePos.X;
                double offsetY = m_selectionRectangleOldMousePos.Y - m_oldConainerMousePos.Y;

                if (offsetX > 0.01 || offsetX < -0.01 && offsetY > 0.01 || offsetY < 0.01)
                {
                    m_pathEditor.MoveSelectedPoints(offsetX / mainFieldSTransform.ScaleX, offsetY / mainFieldSTransform.ScaleY);
                }
            }

            if (m_selectionRectangle.Width != 0 && m_selectionRectangle.Height != 0)
            {
                m_pathEditor.MultipleSelectionMode = true;

                var currentMainFieldPos = e.GetPosition(MainField);

                var sTransorm = m_selectionRectangle.RenderTransform as ScaleTransform;

                double rectX = sTransorm.ScaleX < 0 ? 
                    currentMainFieldPos.X :
                    m_oldMousePos.X;
                double rectY = sTransorm.ScaleY < 0 ?
                    currentMainFieldPos.Y :
                    m_oldMousePos.Y;

                var location = new Point(rectX, rectY);
                var lenght = new Size(m_selectionRectangle.Width / mainFieldSTransform.ScaleX, 
                                      m_selectionRectangle.Height / mainFieldSTransform.ScaleY);

                m_pathEditor.SelectPoints(new Rect(location, lenght));
            }

            if (m_pressedKey != Key.LeftShift)
            {
                ResetSelectionRectangle();
            }

            if (m_pressedKey != Key.LeftShift && m_pathEditor.MultipleSelectionMode)
            {
                m_pathEditor.MultipleSelectionMode = false;
            }
        }

        private void ObecjctClicked_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is PathVisualizer.VisualObject vOject)
            {
                m_pathEditor.CurrentObject = vOject;
                vOject.IsSelected = !vOject.IsSelected;

                if (!vOject.IsSelected)
                {
                    m_pathEditor.CurrentObject = null;
                }
            }
        }

        private void ResolutionChange_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;

            if (menuItem != null)
            {
                string[] resolution = menuItem.Header.ToString().Split('x', StringSplitOptions.RemoveEmptyEntries);
                int width, height;

                if (int.TryParse(resolution[0], out width) && int.TryParse(resolution[1], out height))
                {
                    this.Width = width;
                    this.Height = height;

                    var pointTranformation = (View.PointTransformationUC)userControls[1];

                    var transormDatas = pointTranformation.PointTransformationDatas;

                    if (transormDatas.Count != 0)
                    {
                        switch (width)
                        {
                            case 800:
                                m_pathEditor.DrawScale(pointTranformation.PointTransformationDatas[0]);
                                break;
                            case 1080:
                                m_pathEditor.DrawScale(pointTranformation.PointTransformationDatas[1]);
                                break;
                            case 1280:
                                m_pathEditor.DrawScale(pointTranformation.PointTransformationDatas[2]);
                                break;
                            case 1680:
                                m_pathEditor.DrawScale(pointTranformation.PointTransformationDatas[3]);
                                break;
                            case 1920:
                                m_pathEditor.DrawScale(pointTranformation.PointTransformationDatas[4]);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
        }

        private void PathTransorm_Click(object sender, RoutedEventArgs e)
        {
            UserContentContainer.Child = userControls[1];
        }

        private async void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!isLoaded)
            {
                if (m_pointLoader == null)
                {
                    OpenFileDialog openFileDialog = new OpenFileDialog();
                    openFileDialog.Filter = "exe files (*.ext) |*.exe";

                    if (openFileDialog.ShowDialog() == true)
                    {
                        if (m_pointLoader == null)
                        {
                            m_pointLoader = new Services.PointLoader(openFileDialog.FileName);
                        }
                    }
                    else
                    {
                        return;
                    }
                }

                m_pathEditor.AddRangePoint(await m_pointLoader.LoadPointsAsync());

                isLoaded = true;
            }
        }

        private void AddPoint_Click(object sender, RoutedEventArgs e)
        {
            var point = new Models.GTA_SA_Point(m_oldMousePos, 0, false);

            m_pathEditor.AddPoint(point);
        }
    }
}
