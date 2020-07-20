
using System;
using System.Windows.Controls;

namespace VsUnityDebugHelper.Define
{
    class MyTreeViewItem : TreeViewItem
    {
        /// <summary>
        /// Dummy Object를 없애주기 위해서 사용
        /// </summary>
        public bool hasDummy = false;

        public Action doubleClicked = null;

        public void CreateDummy()
        {
            Items.Add(new TreeViewItem());
            hasDummy = true;
        }
    }
}
