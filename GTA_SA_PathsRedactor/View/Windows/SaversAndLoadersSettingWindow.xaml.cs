using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GTA_SA_PathsRedactor.Models;
using GTA_SA_PathsRedactor.Services;
using GTA_SA_PathsRedactor.ViewModel;
using Microsoft.Extensions.DependencyInjection;

namespace GTA_SA_PathsRedactor.View
{
    /// <summary>
    /// Логика взаимодействия для SaversAndLoadersSettingWindow.xaml
    /// </summary>
    public partial class SaversAndLoadersSettingWindow : Window
    {
        private readonly PointStoreSettingsViewModel _settingsViewModel;

        private bool _isUiChange;

        public SaversAndLoadersSettingWindow()
        {
            InitializeComponent();

            _settingsViewModel = new PointStoreSettingsViewModel(((App)App.Current).ServiceProvider.GetService<ProxyController>()!);
            _settingsViewModel.PropertyChanged += PointStoreSettingsPropertyChanged;

            DataContext = _settingsViewModel;
        }

        public TreeNodeWithItem SelectedSaver => _settingsViewModel.CurrentSaver;
        public TreeNodeWithItem SelectedLoader => _settingsViewModel.CurrentLoader;

        public T? GetTopmostNode<T>(T? treeNode)
            where T : TreeNode
        {
            if (treeNode == null)
                return null;

            if (treeNode.Parent != null)
                return GetTopmostNode((T)treeNode.Parent);
            else
                return treeNode;
        }

        public void SetStartSaver(Core.IPointSaver saver)
        {
            _settingsViewModel.CurrentSaver = GetExistNode(_settingsViewModel.Savers, saver.GetType());
        }
        
        public void SetStartLoader(Core.IPointLoader loader)
        {
            _settingsViewModel.CurrentLoader = GetExistNode(_settingsViewModel.Loaders, loader.GetType());
        }

        private TreeNodeWithItem GetExistNode(ObservableCollection<TreeNodeWithItem> collection, Type objectType)
        {
            var objFullName = objectType.FullName;
            var assemblyInfo = objectType.Assembly.GetAssemblyInfo();
            var foundNode = collection.Where(node => node.Element.Equals(assemblyInfo)).FirstOrDefault();
            var firstElem = (TreeNodeWithItem)collection[0].Nodes[0];

            if (foundNode != null)
            {
                var innerCollection = foundNode.Nodes;
                var innerElem = innerCollection.Where(innerNode => innerNode.Name == objFullName).FirstOrDefault();

                if (innerElem != null)
                    return (TreeNodeWithItem)innerElem;
                else
                    return firstElem;
            }
            else
            {
                return firstElem;
            }
        }

        private void SelectItemInTreeView(TreeView treeView, TreeNodeWithItem node)
        {
            //TODO Research container generator for node selection when property changed occured 
            var item = treeView.ItemContainerGenerator.ContainerFromItem(GetTopmostNode(node)) as TreeViewItem;

            if (item != null)
                item.IsSelected = true;
        }

        private void SaverAndLoaderSettingWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SelectItemInTreeView(SaversTreeView, _settingsViewModel.CurrentSaver);
            SelectItemInTreeView(LoadersTreeView, _settingsViewModel.CurrentLoader);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView treeView = sender as TreeView;

            var node = (TreeNodeWithItem)e.NewValue;
            var topmostNode = GetTopmostNode(node);

            if (node.Parent == null)
            {
                LoaderAndSaverInfoGB.DataContext = node;

                if (GetTopmostNode(_settingsViewModel.CurrentSaver)?.Equals(topmostNode) != true)
                    SaverGroupBox.DataContext = null;
                if (GetTopmostNode(_settingsViewModel.CurrentLoader)?.Equals(topmostNode) != true)
                    LoaderGroupBox.DataContext = null;

                return;
            }

            switch (treeView.Tag)
            {
                case "1":
                    _settingsViewModel.CurrentLoader = node;
                    LoaderGroupBox.DataContext = node.Element;

                    if (GetTopmostNode(_settingsViewModel.CurrentSaver)?.Equals(topmostNode) != true)
                        LoaderAndSaverInfoGB.DataContext = null;
                    else
                        LoaderAndSaverInfoGB.DataContext = GetTopmostNode(node);
                    break;
                case "2":
                    _settingsViewModel.CurrentSaver = node;
                    SaverGroupBox.DataContext = node.Element;

                    if (GetTopmostNode(_settingsViewModel.CurrentLoader)?.Equals(topmostNode) != true)
                        LoaderAndSaverInfoGB.DataContext = null;
                    else
                        LoaderAndSaverInfoGB.DataContext = GetTopmostNode(node);
                    break;
                default:
                    break;
            }

            _isUiChange = false;

            //LoaderAndSaverInfoGB.DataContext = GetTopmostNode(node);
        }

        private void PointStoreSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_isUiChange)
            {
                return;
            }

            var setting = (PointStoreSettingsViewModel)sender;

            switch (e.PropertyName)
            {
                case "CurrentSaver":
                    SelectItemInTreeView(SaversTreeView, setting.CurrentSaver);
                    break;
                case "CurrentLoader":
                    SelectItemInTreeView(LoadersTreeView, setting.CurrentLoader);
                    break;
                default:
                    break;
            }
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void TreeViewClick(object sender, MouseButtonEventArgs e)
        {
            _isUiChange = true;
        }
    }
}
