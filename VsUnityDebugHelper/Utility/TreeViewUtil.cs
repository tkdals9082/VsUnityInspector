using System.Collections.Generic;
using System.Windows.Controls;

namespace VsUnityDebugHelper
{
    public static class TreeViewUtil
    {
        /// <summary>
        /// return index from parent to target item.
        /// a - b
        ///   \ c - d
        ///   => index of d = [1, 0]
        /// </summary>
        /// <param name="item">array of index</param>
        /// <returns></returns>
        public static int[] GetIndexOfItem(this TreeViewItem item)
        {
            if (item == null) return null;

            TreeViewItem parent = item.Parent as TreeViewItem;
            if(parent == null)
            {
                return new int[] { GetSiblingIndexOfItem(item) };
            }

            List<int> indexToRoot = new List<int>();

            while(parent != null)
            {
                indexToRoot.Add(GetSiblingIndexOfItem(item));
                item = parent;
                parent = parent.Parent as TreeViewItem;
            }

            indexToRoot.Reverse();

            return indexToRoot.ToArray();
        }

        public static int GetSiblingIndexOfItem(this TreeViewItem item)
        {
            if (item != null)
            {
                TreeViewItem parent = item.Parent as TreeViewItem;
                if (parent != null)
                {
                    return parent.Items.IndexOf(item); //=1 when you select "0-0-1"
                }
            }
            return -1;
        }
    }
}
