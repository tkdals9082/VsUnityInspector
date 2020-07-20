namespace VsUnityDebugHelper.VsUnityHierarchy
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using VsUnityDebugHelper.Define;

    /// <summary>
    /// Interaction logic for ToolWindow1Control.
    /// </summary>
    public partial class VsUnityHierarchyControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VsUnityHierarchyControl"/> class.
        /// </summary>
        public VsUnityHierarchyControl()
        {
            this.InitializeComponent();
        }

        private void HierarchyView_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var hit = e.OriginalSource as DependencyObject;
            while (hit != null && !(hit is TreeViewItem))
                hit = VisualTreeHelper.GetParent(hit);

            MyTreeViewItem item = hit as MyTreeViewItem;

            if (item == null) return;

            e.Handled = true;
            item.doubleClicked?.Invoke();
        }
    }
}