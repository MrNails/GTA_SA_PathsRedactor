using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace GTA_SA_PathsRedactor.Models
{
    public class TreeNode : Core.Entity
    {
        private string m_name;
        private TreeNode? m_parent;
        private ObservableCollection<TreeNode> m_treeNodes;

        public TreeNode()
        {
            m_treeNodes = new ObservableCollection<TreeNode>();

            m_name = string.Empty;

            m_treeNodes.CollectionChanged += TreeNodes_CollectionChanged;
        }

        public TreeNode? Parent => m_parent;
        public ObservableCollection<TreeNode> Nodes => m_treeNodes;

        public string Name
        {
            get { return m_name; }
            set 
            {
                if (value == string.Empty)
                    m_errors["Name"] = "Node name cannot be empty.";
                else
                    m_errors["Name"] = "";

                m_name = value;
                OnPropertyChanged();
            }
        }

        private void TreeNodes_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (TreeNode node in e.NewItems)
                    {
                        node.m_parent = this;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (TreeNode node in e.OldItems)
                    {
                        node.m_parent = null;
                    }
                    break;
                default:
                    break;
            }
        }

        public static string GetFullPath(TreeNode node, string delimiter)
        {
            StringBuilder path = new StringBuilder();
            delimiter = delimiter == string.Empty ? "\\" : delimiter;

            if (node.Parent == null)
                return node.Name;

            path.Append(GetFullPath(node.Parent, delimiter));

            return path.ToString();
        }
    }

    public class TreeNodeWithItem : TreeNode
    {
        private string m_displayMember;
        private object? m_element;
        
        public TreeNodeWithItem(object element) : this(element, string.Empty)
        {}
        public TreeNodeWithItem(object element, string displayMember)
        {
            Element = element;
            DisplayMember = displayMember;
        }

        public string DisplayMember
        {
            get { return m_displayMember; }
            set
            {
                m_displayMember = value;

                var name = GetPropertyValueAsText();

                Name = name == string.Empty ? Element?.ToString() : name;

                OnPropertyChanged();
            }
        }

        public object? Element
        {
            get { return m_element; }
            set
            {
                m_element = value;
                OnPropertyChanged();
            }
        }

        private string GetPropertyValueAsText()
        {
            if (DisplayMember == string.Empty ||
                Element == null)
                return string.Empty;

            var type = m_element.GetType();
            var propertyInfo = type.GetProperty(DisplayMember);

            return propertyInfo.GetValue(Element)?.ToString()!;
        }
    }
}
