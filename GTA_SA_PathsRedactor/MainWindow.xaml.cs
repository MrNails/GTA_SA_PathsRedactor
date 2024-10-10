using GTA_SA_PathsRedactor.Core.Models;
using GTA_SA_PathsRedactor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.Input;
using GTA_SA_PathsRedactor.Services.SaversAndLoaders;

namespace GTA_SA_PathsRedactor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool m_pointMoveMode;
        private bool m_mouseDown;
        private bool m_gSettingPropChanged;

        private Rectangle m_selectionRectangle;
        private Key m_pressedKey;
        private Point m_oldMousePos;
        private Point m_oldConainerMousePos;
        private Point m_selectionRectangleOldMousePos;

        private ViewModel.PathVM m_pathVM;
        private WorldPoint? m_oldPoint;

        private Dictionary<ViewModel.PathEditor, Services.HistoryController> m_pathHistory;

        private UserControl[] m_userControls;

        private ContextMenu m_lineContextMenu;
        private ContextMenu m_dotContextMenu;

        public MainWindow()
        {
            m_pathVM = new ViewModel.PathVM();
            m_pathHistory = new Dictionary<ViewModel.PathEditor, Services.HistoryController>();

            InitializeComponent();
            LoadImage();
            InitializeAdditionalComponent();

            m_pointMoveMode = false;
            m_mouseDown = false;

            MapContainer.Focusable = true;

            this.DataContext = m_pathVM;
        }

        private void InitializeAdditionalComponent()
        {
            var mainUC = new View.PointControllerUC(m_pathVM);
            var pathSettingUc = new View.PointTransformationUC();

            mainUC.VerticalAlignment = VerticalAlignment.Top;
            pathSettingUc.VerticalAlignment = VerticalAlignment.Top;
            pathSettingUc.AddGoToHomeCommand(new RelayCommand(() =>
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

                m_pathHistory[arg] = new Services.HistoryController();

                MainField.Children.Add(arg.WorkField);
            };
            m_pathVM.PathRemoved += (s, arg) =>
            {
                arg.DotsMouseDown -= DotClicked_MouseDown;
                arg.LinesMouseDown -= LinesMouseDown;

                m_pathHistory.Remove(arg);

                MainField.Children.Remove(arg.WorkField);
            };
            m_pathVM.PathSelected += PathSelected;
            m_pathVM.MapCleared += (s, arg) =>
            {

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

            // GlobalSettings.GetInstance().PropertyChanged += MainWindow_PropertyChanged;

            m_gSettingPropChanged = true;
            // SetNewResolution(GlobalSettings.GetInstance().Resolution);
            m_gSettingPropChanged = false;
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

        private void PathSelected(ViewModel.PathVM pathVM, Services.PathSelectionArgs e)
        {
            if (e.Path == null)
                return;

            var currentHistory = m_pathHistory[e.Path];
            var newHistoryBinding = new Binding
            {
                Source = currentHistory,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                Mode = BindingMode.OneWay,
                Path = new PropertyPath("HasChagned"),
                Converter = new Services.Converters.HistoryHasChangedConverter()
            };

            //CurrentPathInfo.Content = currentHistory.HasChagned ? "Current path have changes" : string.Empty;
            CurrentPathInfo.SetBinding(StatusBarItem.ContentProperty, newHistoryBinding);
        }

        private void ResetSelectionRectangle()
        {
            m_selectionRectangle.Width = 0;
            m_selectionRectangle.Height = 0;
            m_selectionRectangle.Visibility = Visibility.Collapsed;
        }

        private void RemoveSelectedPoints()
        {
            var currentPath = m_pathVM.CurrentPath;

            if (currentPath != null && currentPath.SelectedDots.Count != 0)
            {
                var voStates = currentPath.SelectedDots.Select(dot => new VOState(dot, null, currentPath.IndexOf(dot), Services.State.Deleted))
                                                       .OrderBy(voState => voState.VOIndex)
                                                       .ToList();

                currentPath.RemoveSelectedPoints();

                m_pathHistory[currentPath].AddNew(new VOGroupState(voStates, Services.State.Deleted));
            }
        }

        private void SetNewResolution(Resolution resolution)
        {
            switch (resolution)
            {
                case Resolution._1080x850:
                    SetNewResolution(1080, 850);
                    break;
                case Resolution._1280x1024:
                    SetNewResolution(1280, 1024);
                    break;
                case Resolution._1680x1050:
                    SetNewResolution(1680, 1050);
                    break;
                case Resolution._1920x1080:
                    SetNewResolution(1920, 1080);
                    break;
                default:
                    break;
            }
        }

        private void SetNewResolution(double width, double height)
        {
            // this.Width = width;
            // this.Height = height;
            //
            // var gTransform = ((TransformGroup)m_userControls[0].LayoutTransform);
            // var gSettings = GlobalSettings.GetInstance();
            //
            // if (m_gSettingPropChanged)
            // {
            //     ((ScaleTransform)gTransform.Children[0]).ScaleY = 1;
            //
            //     if (gSettings.Resolution == Resolution._1080x850)
            //         ((ScaleTransform)gTransform.Children[0]).ScaleY = 0.9;
            //
            //     return;
            // }
            //
            // ((ScaleTransform)gTransform.Children[0]).ScaleY = 1;
            //
            // switch (width)
            // {
            //     case 1080:
            //         gSettings.Resolution = Resolution._1080x850;
            //
            //         ((ScaleTransform)gTransform.Children[0]).ScaleY = 0.9;
            //         break;
            //     case 1280:
            //         gSettings.Resolution = Resolution._1280x1024;
            //         break;
            //     case 1680:
            //         gSettings.Resolution = Resolution._1680x1050;
            //         break;
            //     case 1920:
            //         gSettings.Resolution = Resolution._1920x1080;
            //         break;
            //     default:
            //         break;
            // }
        }

        private void MainWindow_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var gSettings = (GlobalSettings)sender;

            m_gSettingPropChanged = true;

            if (e.PropertyName == "Resolution")
            {
                SetNewResolution(gSettings.Resolution);
            }

            m_gSettingPropChanged = false;
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
            if (e.Key == Key.LeftShift || e.Key == Key.LeftCtrl)
            {
                ResetSelectionRectangle();
            }

            if (m_pointMoveMode && m_oldPoint != null && m_pathVM.CurrentPath.CurrentObject != null)
            {
                m_pathHistory[m_pathVM.CurrentPath].AddNew(new VOState(m_pathVM.CurrentPath.CurrentObject, m_oldPoint));
                m_oldPoint = null;
            }

            m_pressedKey = Key.None;
            m_pointMoveMode = false;
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                MainField.Focus();

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
                if (m_oldPoint == null)
                    m_oldPoint = (WorldPoint)m_pathVM.CurrentPath.CurrentObject?.Point.Clone();

                currentPath.CurrentObject.Point.X = (float)currentPos.X;
                currentPath.CurrentObject.Point.Y = (float)currentPos.Y;
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
            m_mouseDown = false;

            MapContainer.Focus();

            var mainFieldSTransform = (ScaleTransform)((TransformGroup)MainField.RenderTransform).Children[0];
            var currentPath = m_pathVM.CurrentPath;

            if (m_pointMoveMode && m_oldPoint != null && m_pathVM.CurrentPath.CurrentObject != null)
            {
                m_pathHistory[m_pathVM.CurrentPath].AddNew(new VOState(m_pathVM.CurrentPath.CurrentObject, m_oldPoint));
                m_oldPoint = null;
            }

            if (currentPath != null && e.ChangedButton == MouseButton.Left &&
                (m_selectionRectangle.Width == 0 || m_selectionRectangle.Height == 0))
                currentPath.MultipleSelectionMode = false;

            if (m_pressedKey == Key.LeftShift && currentPath.SelectedDots.Count != 0 &&
                m_selectionRectangle.Width != 0 && m_selectionRectangle.Height != 0)
            {
                double offsetX = m_selectionRectangleOldMousePos.X - m_oldConainerMousePos.X;
                double offsetY = m_selectionRectangleOldMousePos.Y - m_oldConainerMousePos.Y;

                if (offsetX > 0.01 || offsetX < -0.01 && offsetY > 0.01 || offsetY < 0.01)
                {
                    var oldPoints = currentPath.SelectedDots.Select(dot => (WorldPoint)dot.Point.Clone()).ToList();

                    currentPath.MoveSelectedPoints((float)(offsetX / mainFieldSTransform.ScaleX), (float)(offsetY / mainFieldSTransform.ScaleY));

                    var oldPointsEnumerator = oldPoints.GetEnumerator();
                    var newPointsEnumerator = currentPath.SelectedDots.GetEnumerator();
                    var voStates = new List<VOState>();

                    while (oldPointsEnumerator.MoveNext() && newPointsEnumerator.MoveNext())
                    {
                        voStates.Add(new VOState(newPointsEnumerator.Current, oldPointsEnumerator.Current));
                    }

                    m_pathHistory[currentPath].AddNew(new VOGroupState(voStates, Services.State.Moved));
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
                m_selectionRectangle.Visibility = Visibility.Collapsed;

            if (currentPath?.SelectedDots.Count == 0)
                ResetSelectionRectangle();
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
            var point = new WorldPoint((float)m_oldMousePos.X, (float)m_oldMousePos.Y, 0, false);
            var dot = new DotVisual(point);
            var currentPath = m_pathVM.CurrentPath;

            // var currentPTD = GlobalSettings.GetInstance().GetCurrentTranfromationData();

            // dot.TransformBack(currentPTD);
            // dot.Transform(currentPTD);

            m_pathHistory[currentPath].AddNew(new VOState(dot, null, currentPath.Dots.Count, Services.State.Added));

            currentPath.AddPoint(dot);
        }
        private void InsertPoint_Click(object sender, RoutedEventArgs e)
        {
            var point = new WorldPoint((float)m_oldMousePos.X, (float)m_oldMousePos.Y, 0, false);
            var line = m_lineContextMenu.PlacementTarget as LineVisual;
            var currentPath = m_pathVM.CurrentPath;
            var dot = new DotVisual(point);
            // var currentPTD = GlobalSettings.GetInstance().GetCurrentTranfromationData();

            var insertIndex = currentPath.IndexOf(dot => dot.Point == line?.Start);

            // dot.TransformBack(currentPTD);
            // dot.Transform(currentPTD);

            if (insertIndex == currentPath.Dots.Count - 1)
                insertIndex = 0;
            else
                insertIndex++;

            if (insertIndex != -1)
            {
                m_pathHistory[m_pathVM.CurrentPath].AddNew(new VOState(dot, null, insertIndex, Services.State.Added));

                DebugTextBlock.Text = insertIndex.ToString();

                currentPath.InsertPoint(insertIndex, dot);
            }
        }
        private void RemovePoint_Click(object sender, RoutedEventArgs e)
        {
            var dot = m_dotContextMenu.PlacementTarget as VisualObject;
            var currentPath = m_pathVM.CurrentPath;

            m_pathHistory[currentPath].AddNew(new VOState(dot, null, currentPath.IndexOf(dot), Services.State.Deleted));

            currentPath.RemovePoint(dot);
        }
        private void RemoveSelectedPoint_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedPoints();
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

        private void PointStoreSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // var globalSettings = GlobalSettings.GetInstance();
            // var saversAndLoadersSettingWindow = new View.SaversAndLoadersSettingWindow();
            // saversAndLoadersSettingWindow.SetStartLoader(globalSettings.CurrentLoader);
            // saversAndLoadersSettingWindow.SetStartSaver(globalSettings.CurrentSaver);
            //
            // if (saversAndLoadersSettingWindow.ShowDialog() == true)
            // {
            //     var saver = saversAndLoadersSettingWindow.SelectedSaver;
            //     var loader = saversAndLoadersSettingWindow.SelectedLoader;
            //
            //     var topmostSaver = saversAndLoadersSettingWindow.GetTopmostNode(saver);
            //     var topmostLoader = saversAndLoadersSettingWindow.GetTopmostNode(loader);
            //
            //     var currentTopmostLoaderElem = topmostLoader.Element as AssemblyInfo;
            //     var currentTopmostSaverElem = topmostSaver.Element as AssemblyInfo;
            //
            //     if (currentTopmostLoaderElem.Title == "Default")
            //         globalSettings.CurrentLoader = new DefaultPointLoader();
            //     else
            //         globalSettings.CurrentLoader = Services.ProxyController.CreateInsanceFromAssembly<Core.IPointLoader>(topmostLoader.Value.ToString(), loader.Value.ToString());
            //
            //     if (currentTopmostSaverElem.Title == "Default")
            //         globalSettings.CurrentSaver = new DefaultPointSaver();
            //     else
            //         globalSettings.CurrentSaver = Services.ProxyController.CreateInsanceFromAssembly<Core.IPointSaver>(topmostSaver.Value.ToString(), saver.Value.ToString());
            // }
            //
            // saversAndLoadersSettingWindow = null;
        }

        private void Undo(object sender, ExecutedRoutedEventArgs e)
        {
            var currentPath = m_pathVM.CurrentPath;

            if (currentPath != null)
            {
                var currentPH = m_pathHistory[currentPath];

                if (currentPH.IsPositionOnStart)
                    return;

                var storable = currentPH.CurrentElement;

                if (storable is VOState voState)
                {
                    switch (voState.State)
                    {
                        case Services.State.Added:
                            currentPath.RemovePoint(voState.VisualObject);
                            break;
                        case Services.State.Moved:
                            voState.OldPoint.CopyTo(voState.VisualObject.Point);
                            break;
                        case Services.State.Deleted:
                            currentPath.InsertPoint(voState.VOIndex, voState.VisualObject);
                            break;
                        default:
                            break;
                    }

                }
                else if (storable is VOGroupState voGroupState)
                {
                    switch (voGroupState.State)
                    {
                        case Services.State.Moved:
                            foreach (var _voState in voGroupState.VisualObjects)
                            {
                                _voState.OldPoint.CopyTo(_voState.VisualObject.Point);
                            }
                            break;
                        case Services.State.Deleted:
                            foreach (var _voState in voGroupState.VisualObjects)
                            {
                                currentPath.InsertPoint(_voState.VOIndex, _voState.VisualObject);
                            }
                            break;
                        default:
                            break;
                    }
                }

                currentPH.MoveLeft();
            }
        }
        private void Redo(object sender, ExecutedRoutedEventArgs e)
        {
            var currentPath = m_pathVM.CurrentPath;

            if (currentPath != null)
            {
                var currentPH = m_pathHistory[currentPath];

                if (currentPH.IsPositionOnEnd)
                    return;

                currentPH.MoveRight();

                var storable = currentPH.CurrentElement;

                if (storable is VOState voState)
                {
                    switch (voState.State)
                    {
                        case Services.State.Added:
                            currentPath.InsertPoint(voState.VOIndex, voState.VisualObject);
                            break;
                        case Services.State.Moved:
                            voState.NewPoint.CopyTo(voState.VisualObject.Point);
                            break;
                        case Services.State.Deleted:
                            currentPath.RemovePoint(voState.VisualObject);
                            break;
                        default:
                            break;
                    }

                }
                else if (storable is VOGroupState voGroupState)
                {
                    switch (voGroupState.State)
                    {
                        case Services.State.Moved:
                            foreach (var _voState in voGroupState.VisualObjects)
                            {
                                _voState.NewPoint.CopyTo(_voState.VisualObject.Point);
                            }
                            break;
                        case Services.State.Deleted:
                            foreach (var _voState in voGroupState.VisualObjects)
                            {
                                currentPath.RemovePoint(_voState.VisualObject);
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        private void DeleteSelectedPoints(object sender, ExecutedRoutedEventArgs e)
        {
            RemoveSelectedPoints();
        }

        private void SaveCurrentPath(object sender, ExecutedRoutedEventArgs e)
        {
            if (!m_pathVM.SaveCurrentPath.CanExecute(null))
                return;

            m_pathVM.SaveCurrentPath.Execute(null);

            var history = m_pathHistory[m_pathVM.CurrentPath];

            if (history.CurrentPosition != -1)
                history.SetNewOverloadThresholdElem(history.CurrentPosition);
        }
        private void SaveCurrentPathAs(object sender, ExecutedRoutedEventArgs e)
        {
            if (!m_pathVM.SaveCurrentPath.CanExecute(null))
                return;

            m_pathVM.SaveCurrentPathAs.Execute(null);

            var history = m_pathHistory[m_pathVM.CurrentPath];

            if (history.CurrentPosition != -1)
                history.SetNewOverloadThresholdElem(history.CurrentPosition);
        }

        private void Help(object sender, ExecutedRoutedEventArgs e)
        {
            if (View.HelpWindow.ExistWindow == null)
                new View.HelpWindow().Show();
            else
                View.HelpWindow.ExistWindow.Activate();
        }
        private void About(object sender, RoutedEventArgs e)
        {
            if (View.AboutWindow.ExistWindow == null)
                new View.AboutWindow().Show();
            else
                View.AboutWindow.ExistWindow.Activate();
        }
    }
}
