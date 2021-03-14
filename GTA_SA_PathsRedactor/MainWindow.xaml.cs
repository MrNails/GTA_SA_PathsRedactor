using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GTA_SA_PathsRedactor.Core.Models;
using GTA_SA_PathsRedactor.Models;

namespace GTA_SA_PathsRedactor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool m_pointMoveMode;
        private bool m_mouseDown;

        private Rectangle m_selectionRectangle;
        private Key m_pressedKey;
        private Point m_oldMousePos;
        private Point m_oldConainerMousePos;
        private Point m_selectionRectangleOldMousePos;

        private ViewModel.PathVM m_pathVM;

        private UserControl[] m_userControls;

        private ContextMenu m_lineContextMenu;
        private ContextMenu m_dotContextMenu;

        public MainWindow()
        {
            m_pathVM = new ViewModel.PathVM();

            InitializeComponent();
            LoadImage();
            InitializeAdditionalComponent();

            m_pointMoveMode = false;
            m_mouseDown = false;

            this.DataContext = m_pathVM;
        }

        private void InitializeAdditionalComponent()
        {
            var mainUC = new View.PointControllerUC(m_pathVM);
            var pathSettingUc = new View.PointTransformationUC();

            mainUC.VerticalAlignment = VerticalAlignment.Top;
            pathSettingUc.VerticalAlignment = VerticalAlignment.Top;
            pathSettingUc.AddGoToHomeCommand(new Services.RelayCommand(obj =>
            {
                UserContentContainer.Child = m_userControls[0];
            }));

            UserContentContainer.Child = mainUC;

            m_userControls = new UserControl[] { mainUC, pathSettingUc };

            foreach (var uc in m_userControls)
            {
                var gTransform = new TransformGroup();
                gTransform.Children.Add(new ScaleTransform(1, 1));

                uc.LayoutTransform = gTransform;
            }

            TransformGroup tGroup = new TransformGroup();

            tGroup.Children.Add(new ScaleTransform());
            tGroup.Children.Add(new TranslateTransform());

            MainField.RenderTransform = tGroup;

            m_pathVM.PathAdded += (s, arg) =>
            {
                arg.DotsMouseDown += DotClicked_MouseDown;
                arg.LinesMouseDown += LinesMouseDown;

                MainField.Children.Add(arg.WorkField);
            };
            m_pathVM.PathRemoved += (s, arg) =>
            {
                arg.DotsMouseDown -= DotClicked_MouseDown;
                arg.LinesMouseDown -= LinesMouseDown;

                MainField.Children.Remove(arg.WorkField);
            };

            m_selectionRectangle = new Rectangle();
            m_selectionRectangle.Stroke = new SolidColorBrush(Colors.White);
            m_selectionRectangle.StrokeDashArray.Add(2);
            m_selectionRectangle.StrokeThickness = 1;
            m_selectionRectangle.RenderTransform = new ScaleTransform();
            MapContainer.Children.Add(m_selectionRectangle);

            m_lineContextMenu = new ContextMenu();
            m_dotContextMenu = new ContextMenu();
            m_lineContextMenu.Placement = PlacementMode.Mouse;
            m_dotContextMenu.Placement = PlacementMode.Mouse;

            var addPointMenuItem = new MenuItem { Header = "Insert point" };
            var removePointMenuItem = new MenuItem { Header = "Remove point" };
            addPointMenuItem.Click += InsertPoint_Click;
            removePointMenuItem.Click += RemovePoint_Click;

            m_lineContextMenu.Items.Add(addPointMenuItem);
            m_dotContextMenu.Items.Add(removePointMenuItem);

            SetNewResolution(1280, 1024);
        }

        private void LoadImage()
        {
            MapIcon.Source = new BitmapImage(new Uri(@"/Resource/MapImage.jpg", UriKind.Relative));
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

        private void SetNewResolution(double width, double height)
        {
            this.Width = width;
            this.Height = height;

            var gTransform = ((TransformGroup)m_userControls[0].LayoutTransform);

            ((ScaleTransform)gTransform.Children[0]).ScaleY = 1;

            var gSettings = GlobalSettings.GetInstance();

            switch (width)
            {
                case 1080:
                    gSettings.Resolution = Resolution._1080x850;

                    ((ScaleTransform)gTransform.Children[0]).ScaleY = 0.9;
                    break;
                case 1280:
                    gSettings.Resolution = Resolution._1280x1024;
                    break;
                case 1680:
                    gSettings.Resolution = Resolution._1680x1050;
                    break;
                case 1920:
                    gSettings.Resolution = Resolution._1920x1080;
                    break;
                default:
                    break;
            }
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

            if (e.RightButton == MouseButtonState.Pressed)
            {
                CanvasContextMenu.IsOpen = true;
            }

            if (m_pressedKey == Key.LeftCtrl)
            {
                m_pointMoveMode = true;
            }
            if (m_pressedKey != Key.LeftShift && e.LeftButton == MouseButtonState.Pressed)
            {
                ResetSelectionRectangle();
                Canvas.SetTop(m_selectionRectangle, m_selectionRectangleOldMousePos.Y);
                Canvas.SetLeft(m_selectionRectangle, m_selectionRectangleOldMousePos.X);
                m_selectionRectangle.Visibility = Visibility.Visible;

                if (m_pathVM.CurrentPath != null)
                    m_pathVM.CurrentPath.MultipleSelectionMode = true;
            }
            else if (m_pressedKey == Key.LeftShift && e.LeftButton == MouseButtonState.Pressed)
            {
                m_selectionRectangle.Visibility = Visibility.Visible;
            }

            if (e.MiddleButton == MouseButtonState.Pressed ||
                (e.LeftButton == MouseButtonState.Pressed && m_pressedKey == Key.LeftShift))
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

            var currentPath = m_pathVM.CurrentPath;

            if (m_pointMoveMode && currentPath.CurrentObject != null)
            {
                currentPath.CurrentObject.Point.X = currentPos.X;
                currentPath.CurrentObject.Point.Y = currentPos.Y;
            }
            else if (!m_pointMoveMode && m_pressedKey != Key.LeftShift && e.LeftButton == MouseButtonState.Pressed)
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
            else if (m_pressedKey == Key.LeftShift)
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
            var currentPath = m_pathVM.CurrentPath;

            if (m_pressedKey == Key.LeftShift && currentPath.SelectedDots.Count != 0 &&
                m_selectionRectangle.Width != 0 && m_selectionRectangle.Height != 0)
            {
                double offsetX = m_selectionRectangleOldMousePos.X - m_oldConainerMousePos.X;
                double offsetY = m_selectionRectangleOldMousePos.Y - m_oldConainerMousePos.Y;

                if (offsetX > 0.01 || offsetX < -0.01 && offsetY > 0.01 || offsetY < 0.01)
                {
                    currentPath.MoveSelectedPoints(offsetX / mainFieldSTransform.ScaleX, offsetY / mainFieldSTransform.ScaleY);
                }
            }
            else if (m_selectionRectangle.Width != 0 && m_selectionRectangle.Height != 0)
            {
                if (currentPath != null)
                    currentPath.MultipleSelectionMode = true;

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

                currentPath?.SelectPoints(new Rect(location, lenght));
            }

            if (m_pressedKey != Key.LeftShift)
            {
                m_selectionRectangle.Visibility = Visibility.Collapsed;
            }

            if (currentPath?.SelectedDots.Count == 0)
            {
                ResetSelectionRectangle();
            }
        }

        private void DotClicked_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is VisualObject vOject)
            {
                var currentPath = m_pathVM.CurrentPath;

                currentPath.CurrentObject = vOject;
                vOject.IsSelected = !vOject.IsSelected;

                if (!vOject.IsSelected)
                {
                    currentPath.CurrentObject = null;
                }

                if (e.RightButton == MouseButtonState.Pressed)
                {
                    m_dotContextMenu.PlacementTarget = vOject;
                    m_dotContextMenu.IsOpen = true;
                }
            }
        }
        private void LinesMouseDown(object sender, MouseButtonEventArgs e)
        {
            var line = sender as LineVisual;

            if (line == null)
            {
                return;
            }

            if (e.RightButton == MouseButtonState.Pressed)
            {
                m_lineContextMenu.PlacementTarget = line;
                m_lineContextMenu.IsOpen = true;
            }
        }

        private void ResolutionChange_Click(object sender, RoutedEventArgs e)
        {
            MenuItem? menuItem = sender as MenuItem;

            if (menuItem != null)
            {
                string[] resolution = menuItem.Header.ToString().Split('x', StringSplitOptions.RemoveEmptyEntries);
                int width, height;

                if (int.TryParse(resolution[0], out width) && int.TryParse(resolution[1], out height))
                {
                    SetNewResolution(width, height);
                }
            }
        }

        private void PathTransorm_Click(object sender, RoutedEventArgs e)
        {
            UserContentContainer.Child = m_userControls[1];
        }

        private void AddPoint_Click(object sender, RoutedEventArgs e)
        {
            var point = new GTA_SA_Point(m_oldMousePos.X, m_oldMousePos.Y, 0, false);

            m_pathVM.CurrentPath.AddPoint(point);
        }
        private void InsertPoint_Click(object sender, RoutedEventArgs e)
        {
            var point = new GTA_SA_Point(m_oldMousePos.X, m_oldMousePos.Y, 0, false);
            var line = m_lineContextMenu.PlacementTarget as LineVisual;
            var currentPath = m_pathVM.CurrentPath;

            var insertIndex = currentPath.IndexOf(dot => dot.Point == line?.Start);

            if (insertIndex != -1)
            {
                DebugTextBlock.Text = insertIndex.ToString();

                currentPath.InsertPoint(insertIndex, point);
            }
        }
        private void RemovePoint_Click(object sender, RoutedEventArgs e)
        {
            var dot = m_dotContextMenu.PlacementTarget as VisualObject;

            m_pathVM.CurrentPath.RemovePoint(dot);
        }

        private void MapContainer_Drop(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData(DataFormats.FileDrop);

            try
            {
                if (data != null)
                {
                    var files = (string[])data;

                    foreach (var file in files)
                    {
                        m_pathVM.LoadPath.Execute(file);
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MapIcon_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            DebugTextBlock.Text = "+";
        }
    }
}
