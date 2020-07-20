using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using VsUnityDebugHelper.VsUnityHierarchy;

namespace VsUnityDebugHelper
{
    class DebuggerEventListener : IVsDebuggerEvents
    {
        public int OnModeChange(DBGMODE dbgmodeNew)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ThreadHelper.JoinableTaskFactory.RunAsync(() => RefreshAsync(dbgmodeNew));

            return 0;
        }

        private async System.Threading.Tasks.Task RefreshAsync(DBGMODE dbgmodeNew)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            switch (dbgmodeNew)
            {
                case DBGMODE.DBGMODE_Break:
                    VsUnityHierarchyCommand.Instance?.Refresh((object)null, (EventArgs)null);
                    break;
            }
        }
    }
}
