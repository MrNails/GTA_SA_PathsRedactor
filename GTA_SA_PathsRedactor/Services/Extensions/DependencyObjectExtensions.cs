using System.Windows;
using System.Windows.Media;

namespace GTA_SA_PathsRedactor.Services
{
    public static class DependencyObjectExtensions
    {
        public static childItem? FindVisualChild<childItem>(this DependencyObject? obj)
                      where childItem : DependencyObject
        {
            if (obj == null)
            {
                return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem? childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
    }
}
