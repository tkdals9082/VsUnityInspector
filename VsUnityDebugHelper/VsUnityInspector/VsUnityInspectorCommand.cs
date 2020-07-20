using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VsUnityDebugHelper.Define;
using VsUnityDebugHelper.VsUnityHierarchy;
using Task = System.Threading.Tasks.Task;

namespace VsUnityDebugHelper.VsUnityInspector
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class VsUnityInspectorCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 4098;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("9fe9f9e7-14d0-4e9b-b09c-6be04cd21b36");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        public const int cmdidApplyInspector = 0x2000;
        public const int inspectorToolbarID = 0x2001;

        public List<VsUnityObjectBase> curInspectedObjects = new List<VsUnityObjectBase>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VsUnityInspectorCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private VsUnityInspectorCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            var applybtnCmdID = new CommandID(CommandSet, cmdidApplyInspector);
            var applyItem = new MenuCommand(new EventHandler(Apply), applybtnCmdID);
            commandService.AddCommand(applyItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static VsUnityInspectorCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in VsUnityInspectorCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new VsUnityInspectorCommand(package, commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(VsUnityInspector), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void Apply(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (VsUnityHierarchyCommand.Instance.CheckNotInBreakMode()) return;

            for (int i = 0; i < curInspectedObjects.Count; i++)
            {
                curInspectedObjects[i].ApplyChanges(VsUnityHierarchyCommand.Instance.dte.Debugger);
            }
        }
    }
}
