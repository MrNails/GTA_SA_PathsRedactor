using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
                    _errors["Name"] = "Node name cannot be empty.";
                else
                    _errors["Name"] = "";

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

    public class TreeNodeWithItem : TreeNode, IEquatable<TreeNodeWithItem>
    {
        private string m_displayMember;
        private string m_valueMember;
        private object? m_element;

        public TreeNodeWithItem(object element) : this(element, string.Empty)
        { }
        public TreeNodeWithItem(object element, string displayMember)
        {
            Element = element;
            DisplayMember = displayMember;
            ValueMember = string.Empty;
        }

        public string DisplayMember
        {
            get { return m_displayMember; }
            set
            {
                m_displayMember = value;

                var name = GetPropertyValue(value)?.ToString();

                Name = name ?? Element.ToString();

                OnPropertyChanged();
            }
        }
        public string ValueMember
        {
            get { return m_valueMember; }
            set
            {
                m_valueMember = value;

                OnPropertyChanged();
                OnPropertyChanged("Value");
            }
        }

        public object Value => string.IsNullOrEmpty(ValueMember) ? null : GetPropertyValue(ValueMember);

        public object? Element
        {
            get { return m_element; }
            set
            {
                m_element = value;
                OnPropertyChanged();
            }
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as TreeNodeWithItem);
        }

        public bool Equals(TreeNodeWithItem? other)
        {
            return other != null &&
                   other.Name == other.Name &&
                   Element.Equals(other.Element);
        }

        private object GetPropertyValue(string property)
        {
            if (DisplayMember == string.Empty ||
                Element == null)
                return null;

            var type = m_element.GetType();
            var propertyInfo = type.GetProperty(property);

            return propertyInfo.GetValue(Element);
        }
    }

    public class TreeNodeComparer : IEqualityComparer<TreeNode>
    {
        public bool Equals(TreeNode? x, TreeNode? y)
        {
            if (x == null)
                return false;

            return x.Equals(y);
        }

        public int GetHashCode([DisallowNull] TreeNode obj)
        {
            return obj.GetHashCode();
        }
    }

}
