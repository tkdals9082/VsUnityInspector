namespace VsUnityDebugHelper.VsUnityInspector
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using System.ComponentModel.Design;
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
    [Guid("b9988982-94b7-40d4-9439-71e635223420")]
    public class VsUnityInspector : ToolWindowPane
    {

        public VsUnityInspectorControl control;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsUnityInspector"/> class.
        /// </summary>
        public VsUnityInspector() : base(null)
        {
            this.Caption = "Unity Inspector";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            control = new VsUnityInspectorControl();
            this.Content = control;

            this.ToolBar = new CommandID(VsUnityInspectorCommand.CommandSet, VsUnityInspectorCommand.inspectorToolbarID);
            this.ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }
    }
}
