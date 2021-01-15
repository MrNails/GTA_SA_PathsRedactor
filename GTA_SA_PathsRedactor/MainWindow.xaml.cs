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

namespace GTA_SA_PathsRedactor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool addmode;
        private bool movemode;
        private bool isLoaded;
        private bool m_mouseDown;
        private bool m_mouseMoved;

        private Point m_oldMousePos;
        private PathVisualizer.Path path;
        private ViewModel.PathEditor pathEditor;

        private UserControl[] userControls;

        public MainWindow()
        {
            InitializeComponent();
            LoadImage();
            InitializeAdditionalComponent();

            addmode = false;
            movemode = false;
            isLoaded = false;
            m_mouseDown = false;
            m_mouseMoved = false;

            path = new PathVisualizer.Path("First path");
            MainField.Children.Add(path);
            pathEditor = new ViewModel.PathEditor("TestPath");
            pathEditor.Path = path;
        }

        private void InitializeAdditionalComponent()
        {
            var mainUC = new View.PointControllerUC();
            var pathSettingUc = new View.PointTransformationUC();
            mainUC.VerticalAlignment = VerticalAlignment.Top;
            pathSettingUc.VerticalAlignment = VerticalAlignment.Top;

            userControls = new UserControl[] { mainUC, pathSettingUc };

            TransformGroup tGroup = new TransformGroup();

            tGroup.Children.Add(new ScaleTransform());
            tGroup.Children.Add(new TranslateTransform());

            MainField.RenderTransform = tGroup;
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
        private void AddToMainFieldTranslate(double newX, double newY)
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            addmode = !addmode;
            //AddModeTextBlock.Text = addmode.ToString();
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

                if (e.Delta > 0)
                {
                    tTransform.X = -pos.X * (sTransform.ScaleX - 1);
                    tTransform.Y = -pos.Y * (sTransform.ScaleY - 1);

                    AddToMainFieldTranslate(0, 0);
                }
                else
                {
                    AddToMainFieldTranslate(-(pos.X * (sTransform.ScaleX - 1) + tTransform.X), 
                                            -(pos.Y * (sTransform.ScaleY - 1) + tTransform.Y));
                }



                if (sTransform.ScaleX < 1 || sTransform.ScaleY < 1)
                {
                    sTransform.ScaleX = 1;
                    sTransform.ScaleY = 1;
                    ResetTranslate(tTransform);
                }
            }
        }

        private void MainField_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                m_mouseDown = true;
                m_oldMousePos = e.GetPosition((IInputElement)sender);
            }
            //MoveMode.Text = m_oldMousePos.ToString();
        }
        private void MainField_MouseMove(object sender, MouseEventArgs e)
        {
            if (!m_mouseDown)
            {
                return;
            }

            Point currentPos = e.GetPosition((IInputElement)sender);

            if (movemode && path.CurrentObject != null)
            {
                path.CurrentObject.Point.X = currentPos.X;
                path.CurrentObject.Point.Y = currentPos.Y;
            }
            else 
            {
                double offsetX = currentPos.X - m_oldMousePos.X;
                double offsetY = currentPos.Y - m_oldMousePos.Y;

                if (Math.Abs(offsetX) > 25 || Math.Abs(offsetY) > 25)
                {
                    m_oldMousePos = currentPos;
                }

                AddToMainFieldTranslate(offsetX, offsetY);
            }
        }
        private void MainField_MouseUp(object sender, MouseButtonEventArgs e)
        {
            m_mouseDown = false;

            if (m_mouseMoved || !addmode)
            {
                return;
            }

            PathVisualizer.DotVisual dotVisual = new PathVisualizer.DotVisual(new Models.GTA_SA_Point(e.GetPosition(MainField), 0, false));

            path.AddPoint(dotVisual);
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

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            movemode = !movemode;
            //MoveMode.Text = movemode.ToString();

            if (!isLoaded)
            {
                await pathEditor.LoadPointAsync();
                isLoaded = true;
            }
        }

        private void OffsetXTBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;

            var transformObj = Services.PointTransformationData.TransformatorForResolution1280x1024;

            switch (textBox.Tag.ToString())
            {
                case "1":
                    if (double.TryParse(textBox.Text, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowTrailingSign, System.Globalization.CultureInfo.InvariantCulture, out double res))
                    {
                        if (transformObj.OffsetX == res)
                        {
                            return;
                        }

                        transformObj.OffsetX = res;
                    }
                    else
                    {
                        return;
                    }
                    break;
                case "2":
                    if (double.TryParse(textBox.Text, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowTrailingSign, System.Globalization.CultureInfo.InvariantCulture, out double res1))
                    {
                        if (transformObj.OffsetY == res1)
                        {
                            return;
                        }

                        transformObj.OffsetY = res1;
                    }
                    else
                    {
                        return;
                    }
                    break;
                case "3":
                    if (double.TryParse(textBox.Text, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowTrailingSign, System.Globalization.CultureInfo.InvariantCulture, out double res2))
                    {
                        if (transformObj.PointScaleX == res2)
                        {
                            return;
                        }

                        transformObj.PointScaleX = res2;
                    }
                    else
                    {
                        return;
                    }
                    break;
                case "4":
                    if (double.TryParse(textBox.Text, System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowTrailingSign, System.Globalization.CultureInfo.InvariantCulture, out double res3))
                    {
                        if (transformObj.PointScaleY == res3)
                        {
                            return;
                        }

                        transformObj.PointScaleY = res3;
                    }
                    else
                    {
                        return;
                    }
                    break;
                default:
                    break;
            }

            path.DrawScale(transformObj.OffsetX, transformObj.OffsetY, transformObj.PointScaleX, transformObj.PointScaleY);
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
                }
            }
        }

        private void PathTransorm_Click(object sender, RoutedEventArgs e)
        {
            UserContentContainer.Child = userControls[1];
        }
    }
}
