using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsUnityDebugHelper
{
    public static class SelectionUtility
    {
        public static void SaveLinePosition(this TextSelection selection, out int lineNum)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            lineNum = selection.BottomLine;
        }

        public static string GetTextBetweenCurPosToLine(this TextSelection selection, int to_lineNum, bool moveToEndOfDoc = true)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            selection.MoveToLineAndOffset(to_lineNum, 1, Extend:true);
            string result = selection.Text;
            if(moveToEndOfDoc)
                selection.EndOfDocument();

            return result;
        }
    }
}
