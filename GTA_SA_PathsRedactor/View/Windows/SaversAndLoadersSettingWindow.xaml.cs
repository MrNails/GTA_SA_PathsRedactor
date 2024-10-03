using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Shapes;
using GTA_SA_PathsRedactor.Models;
using GTA_SA_PathsRedactor.Services;
using GTA_SA_PathsRedactor.ViewModel;

namespace GTA_SA_PathsRedactor.View
{
    /// <summary>
    /// Логика взаимодействия для SaversAndLoadersSettingWindow.xaml
    /// </summary>
    public partial class SaversAndLoadersSettingWindow : Window
    {
        private static PointStoreSettingsVM m_settingsVM;

        private bool m_isUIChange;

        static SaversAndLoadersSettingWindow()
        {
            m_settingsVM = new PointStoreSettingsVM();
        }

        public SaversAndLoadersSettingWindow()
        {
            InitializeComponent();

            m_settingsVM.PropertyChanged += PointStoreSettingsPropertyChanged;

            DataContext = m_settingsVM;
        }

        public TreeNodeWithItem SelectedSaver => m_settingsVM.CurrentSaver;
        public TreeNodeWithItem SelectedLoader => m_settingsVM.CurrentLoader;

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
            m_settingsVM.CurrentSaver = GetExistNode(m_settingsVM.Savers, saver.GetType());
        }
        
        public void SetStartLoader(Core.IPointLoader loader)
        {
            m_settingsVM.CurrentLoader = GetExistNode(m_settingsVM.Loaders, loader.GetType());
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
            SelectItemInTreeView(SaversTreeView, m_settingsVM.CurrentSaver);
            SelectItemInTreeView(LoadersTreeView, m_settingsVM.CurrentLoader);
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeView treeView = sender as TreeView;

            var node = (TreeNodeWithItem)e.NewValue;
            var topmostNode = GetTopmostNode(node);

            if (node.Parent == null)
            {
                LoaderAndSaverInfoGB.DataContext = node;

                if (GetTopmostNode(m_settingsVM.CurrentSaver)?.Equals(topmostNode) != true)
                    SaverGroupBox.DataContext = null;
                if (GetTopmostNode(m_settingsVM.CurrentLoader)?.Equals(topmostNode) != true)
                    LoaderGroupBox.DataContext = null;

                return;
            }

            switch (treeView.Tag)
            {
                case "1":
                    m_settingsVM.CurrentLoader = node;
                    LoaderGroupBox.DataContext = node.Element;

                    if (GetTopmostNode(m_settingsVM.CurrentSaver)?.Equals(topmostNode) != true)
                        LoaderAndSaverInfoGB.DataContext = null;
                    else
                        LoaderAndSaverInfoGB.DataContext = GetTopmostNode(node);
                    break;
                case "2":
                    m_settingsVM.CurrentSaver = node;
                    SaverGroupBox.DataContext = node.Element;

                    if (GetTopmostNode(m_settingsVM.CurrentLoader)?.Equals(topmostNode) != true)
                        LoaderAndSaverInfoGB.DataContext = null;
                    else
                        LoaderAndSaverInfoGB.DataContext = GetTopmostNode(node);
                    break;
                default:
                    break;
            }

            m_isUIChange = false;

            //LoaderAndSaverInfoGB.DataContext = GetTopmostNode(node);
        }

        private void PointStoreSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (m_isUIChange)
            {
                return;
            }

            var setting = (PointStoreSettingsVM)sender;

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
            m_isUIChange = true;
        }
    }
}
