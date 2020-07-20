using System;
using System.ComponentModel.Design;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using EnvDTE;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Documents;
using VsUnityDebugHelper.Define;
using VsUnityDebugHelper.VsUnityInspector;

namespace VsUnityDebugHelper.VsUnityHierarchy
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class VsUnityHierarchyCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("9fe9f9e7-14d0-4e9b-b09c-6be04cd21b36");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        public const int cmdidRefreshHierarchy = 0x132;
        public const int hierarchyToolbarID = 0x1000;

        public const string commandWindowGuid = "{28836128-FC2C-11D2-A433-00C04F72D18A}";
        //public const string immediateWindowGuid = "{ECB7191A-597B-41F5-9843-03A4CF275DDE}";

        private VsUnityHierarchy _hierarchyWindow;
        public VsUnityHierarchy hierarchyWindow
        {
            get
            {
                if(_hierarchyWindow == null)
                {
                    _hierarchyWindow = (VsUnityHierarchy)package.FindToolWindow(typeof(VsUnityHierarchy), 0, true);
                }
                return _hierarchyWindow;
            }
        }

        private VsUnityInspector.VsUnityInspector _inspectorWindow;
        public VsUnityInspector.VsUnityInspector inspectorWindow
        {
            get
            {
                if(_inspectorWindow == null)
                {
                    _inspectorWindow = (VsUnityInspector.VsUnityInspector)package.FindToolWindow(typeof(VsUnityInspector.VsUnityInspector), 0, true);
                }
                return _inspectorWindow;
            }
        }

        private EnvDTE80.DTE2 _dte = null;
        public EnvDTE80.DTE2 dte
        {
            get
            {
                if(_dte == null)
                {
                    _dte = ((IServiceProvider)package).GetService(typeof(SDTE)) as EnvDTE80.DTE2;
                }
                return _dte;
            }
        }

        private System.Windows.Controls.TreeView _hierarchyView = null;
        public System.Windows.Controls.TreeView hierarchyView
        {
            get
            {
                if(_hierarchyView == null)
                {
                    _hierarchyView = hierarchyWindow.control.HierarchyView;
                }
                return _hierarchyView;
            }
        }

        private IVsDebugger _debugger = null;
        public IVsDebugger debugger
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if(_debugger == null)
                {
                    _debugger = ((IServiceProvider)package).GetService(typeof(SVsShellDebugger)) as IVsDebugger;
                }
                return _debugger;
            }
        }

        //private Window _immediateWindow = null;
        //public Window immediateWindow
        //{
        //    get
        //    {
        //        ThreadHelper.ThrowIfNotOnUIThread();

        //        if(_immediateWindow == null)
        //        {
        //            _immediateWindow = GetWindow(dte, immediateWindowGuid);
        //        }
        //        return _immediateWindow;
        //    }
        //}

        private Window _commandWindow = null;
        public Window commandWindow
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                if(_commandWindow == null)
                {
                    _commandWindow = GetWindow(dte, commandWindowGuid);
                }
                return _commandWindow;
            }
        }

        private Window GetWindow(EnvDTE80.DTE2 dte, string windowObjectKind)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach(Window window in dte.Windows)
            {
                try
                {
                    if (window.ObjectKind == windowObjectKind)
                    {
                        return window;
                    }
                }
                catch(Exception)
                {
                    // Some windows don't have ObjectKind.
                }
            }
            return null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VsUnityHierarchyCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private VsUnityHierarchyCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            var refreshbtnCmdID = new CommandID(CommandSet, cmdidRefreshHierarchy);
            var refreshItem = new MenuCommand(new EventHandler(Refresh), refreshbtnCmdID);
            commandService.AddCommand(refreshItem);

            // TODO: AdviseDebuggerEvents.
            //      It might be a thread problem...
            //uint debuggerServiceCookie;
            //debugger.AdviseDebuggerEvents(new DebuggerEventListener(), out debuggerServiceCookie);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static VsUnityHierarchyCommand Instance
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
            // Switch to the main thread - the call to AddCommand in ToolWindow1Command's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new VsUnityHierarchyCommand(package, commandService);
        }

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsWindowFrame windowFrame = (IVsWindowFrame)hierarchyWindow.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private bool isInitSceneAndRootObj = false;
        /// <summary>
        /// Refresh Unity Hierarchy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Refresh(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (CheckNotInBreakMode()) return;

            EnvDTE.TextSelection selection = commandWindow?.Selection as EnvDTE.TextSelection;

            if(selection == null)
            {
                ShowMessage(
                    "Error!", 
                    "Cannot find command window automatically\nPlease manually activate it and try again.",
                    OLEMSGICON.OLEMSGICON_CRITICAL);
                return;
            }


            if (!isInitSceneAndRootObj)
            {
                dte.Debugger.ExecuteStatement(Statement: "UnityEngine.SceneManagement.Scene scene;");
                dte.Debugger.ExecuteStatement(Statement: "UnityEngine.GameObject[] objs;");
                isInitSceneAndRootObj = true;
            }

            // Get Active Scene
            string sceneInfoStr = GetDebuggerExecutionOutput(Statement: "scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();", TreatAsExpression: true, selection);
            
            VsScene scene = new VsScene(sceneInfoStr);

            // Get Root GameObjects
            string objInfoStr = GetDebuggerExecutionOutput(Statement: "objs = scene.GetRootGameObjects();", TreatAsExpression: true, selection);
            
            string[] gameObjectNames = OuputParseUtil.GetGameObjectNamesFromInfo(objInfoStr);

            // Get ChildCount of each GameObjects
            int saveLineNum;
            selection.SaveLinePosition(out saveLineNum);
            for (int i = 0; i < scene.rootCount; ++i)
            {
                dte.Debugger.ExecuteStatement(Statement: $"objs[{i}].transform.childCount;", TreatAsExpression: true);
            }
            string childCountInfoStr = selection.GetTextBetweenCurPosToLine(saveLineNum);
            string[] childCountsStr = childCountInfoStr.SplitAndTrim("\r\n", StringSplitOptions.RemoveEmptyEntries);
            
            var objs = new System.Collections.Generic.List<VsGameObject>();

            for(int i = 0; i < gameObjectNames.Length; ++i)
            {
                VsGameObject go = new VsGameObject();
                go.name = gameObjectNames[i];
                go.childCount = int.Parse(childCountsStr[i]);

                objs.Add(go);
            }

            InitHierarchyView(scene, objs);

            selection.EndOfDocument();
        }

        private void InitHierarchyView(VsScene scene, System.Collections.Generic.List<VsGameObject> objs)
        {
            hierarchyView.Items.Clear();

            TreeViewItem sceneItem = new TreeViewItem();
            sceneItem.Header = scene.name;

            foreach(var obj in objs)
            {
                MyTreeViewItem objItem = new MyTreeViewItem();
                objItem.Header = obj.name;
                objItem.doubleClicked += () => Inspect(objItem);
                if (obj.childCount > 0)
                {
                    objItem.CreateDummy();
                    objItem.Expanded += ObjItem_Expanded;
                }
                sceneItem.Items.Add(objItem);
            }

            hierarchyView.Items.Add(sceneItem);
        }

        private bool isInitObj = false;
        private void ObjItem_Expanded(object sender, System.Windows.RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (CheckNotInBreakMode()) return;

            MyTreeViewItem item = sender as MyTreeViewItem;
            if (item == null) return;

            item.Expanded -= ObjItem_Expanded;

            int[] index = item.GetIndexOfItem();
            if (index == null) return;

            string transformQuery = GetTransformQuery(index);

            if (!isInitObj)
            {
                dte.Debugger.ExecuteStatement(Statement: "UnityEngine.Transform tf;");
                isInitObj = true;
            }

            EnvDTE.TextSelection selection = commandWindow?.Selection as EnvDTE.TextSelection;

            dte.Debugger.ExecuteStatement(Statement: $"tf = objs{transformQuery};");

            string childCountStr = GetDebuggerExecutionOutput(Statement: $"tf.childCount", TreatAsExpression: true, selection);
            
            int childCount = int.Parse(childCountStr.SplitAndTrim("\r\n", StringSplitOptions.RemoveEmptyEntries)[0]);

            if(item.hasDummy)
            {
                item.Items.Clear();

                // Get Name
                int lineNum;
                selection.SaveLinePosition(out lineNum);
                for(int i = 0; i < childCount; ++i)
                {
                    dte.Debugger.ExecuteStatement(Statement: $"tf.GetChild({i}).childCount", TreatAsExpression: true);
                }
                string childCountsInfoStr = selection.GetTextBetweenCurPosToLine(lineNum);
                string[] childCounts = childCountsInfoStr.SplitAndTrim("\r\n", StringSplitOptions.RemoveEmptyEntries);

                // Get childCount
                selection.SaveLinePosition(out lineNum);
                for (int i = 0; i < childCount; ++i)
                {
                    dte.Debugger.ExecuteStatement(Statement: $"tf.GetChild({i}).name", TreatAsExpression: true);
                }
                string childNamesInfoStr = selection.GetTextBetweenCurPosToLine(lineNum);
                string[] childNames = childNamesInfoStr.SplitAndTrim("\r\n", StringSplitOptions.RemoveEmptyEntries);

                for (int i = 0; i < childCount; ++i)
                {
                    MyTreeViewItem objItem = new MyTreeViewItem();
                    objItem.Header = childNames[i].Trim('"');
                    objItem.doubleClicked += () => Inspect(objItem);
                    if (int.Parse(childCounts[i]) > 0)
                    {
                        objItem.CreateDummy();
                        objItem.Expanded += ObjItem_Expanded;
                    }
                    item.Items.Add(objItem);
                }
            }
            else
            {
                // if the item doesn't have dummy item, we don't need to initialize it again. 
            }
        }

        #region Inspect function

        private bool isInitInspectObj = false;

        private void Inspect(MyTreeViewItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (CheckNotInBreakMode()) return;

            inspectorWindow.control.ParentPanel.Children.Clear();
            VsUnityInspectorCommand.Instance.curInspectedObjects.Clear();

            int[] index = item.GetIndexOfItem();
            string tfQuery = GetTransformQuery(index);

            if(!isInitInspectObj)
            {
                dte.Debugger.ExecuteStatement(Statement: "UnityEngine.GameObject inspectObj;");
                isInitInspectObj = true;
            }

            EnvDTE.TextSelection selection = commandWindow?.Selection as EnvDTE.TextSelection;
            string gameObjectInfo = GetDebuggerExecutionOutput(Statement: $"inspectObj = objs{tfQuery}.gameObject;", TreatAsExpression:true, selection);

            VsGameObject obj = new VsGameObject(gameObjectInfo);

            obj.Inspect(inspectorWindow);

            VsUnityInspectorCommand.Instance.curInspectedObjects.Add(obj);
        }

        #endregion

        #region Helper Function

        private void ShowMessage(string title, string message, OLEMSGICON icon)
        {
            VsShellUtilities.ShowMessageBox(
                    package,
                    message,
                    title,
                    icon,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }

        private string GetTransformQuery(int[] indexes, bool isStartFromRootObj = true)
        {
            string query = "";

            if (indexes == null) return query;

            for(int i = 0; i < indexes.Length; ++i)
            {
                if(i == 0)
                {
                    if (isStartFromRootObj)
                    {
                        query += $"[{indexes[i]}].transform";
                    }
                    else
                    {
                        query += $".transform.GetChild({indexes[i]})";
                    }
                }
                else
                {
                    query += $".GetChild({indexes[i]})";
                }
            }

            return query;
        }

        private string GetDebuggerExecutionOutput(string Statement, bool TreatAsExpression, EnvDTE.TextSelection selection)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            int saveLineNum;

            selection.EndOfDocument();
            selection.SaveLinePosition(out saveLineNum);
            dte.Debugger.ExecuteStatement(Statement: Statement, TreatAsExpression: TreatAsExpression);
            return selection.GetTextBetweenCurPosToLine(saveLineNum);
        }

        public bool CheckNotInBreakMode()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (dte.Debugger.CurrentMode != dbgDebugMode.dbgBreakMode)
            {
                ShowMessage(
                    "Error!",
                    "Use only if the debugger is in breakMode.",
                    OLEMSGICON.OLEMSGICON_CRITICAL);
                return true;
            }

            return false;
        }

        #endregion
    }
}
