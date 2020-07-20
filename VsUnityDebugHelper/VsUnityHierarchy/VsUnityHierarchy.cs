namespace VsUnityDebugHelper.VsUnityHierarchy
{
    using System;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Controls;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("be99bd26-a98d-4030-9caf-8f4a85b1f03a")]
    public class VsUnityHierarchy : ToolWindowPane
    {
        // Enable search
        //public override bool SearchEnabled => true;

        public VsUnityHierarchyControl control;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsUnityHierarchy"/> class.
        /// </summary>
        public VsUnityHierarchy() : base(null)
        {
            this.Caption = "Unity Hierarchy";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.

            control = new VsUnityHierarchyControl();
            this.Content = control;

            //TestTreeView();
            this.ToolBar = new CommandID(VsUnityHierarchyCommand.CommandSet, VsUnityHierarchyCommand.hierarchyToolbarID);
            this.ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }

        #region Test Functions
        private void TestTreeView()
        {
            TreeViewItem parentItem = new TreeViewItem();
            parentItem.Header = "Parent Object";

            control.HierarchyView.Items.Add(parentItem);

            TreeViewItem childItem1 = new TreeViewItem();
            childItem1.Header = "Child One";
            parentItem.Items.Add(childItem1);

            TreeViewItem childItem2 = new TreeViewItem();
            childItem2.Header = "Child Two";
            parentItem.Items.Add(childItem2);
        }
        #endregion
    }
}
