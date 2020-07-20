using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VsUnityDebugHelper.Define
{
    internal abstract class VsUnityObjectBase : ICloneable
    {
        public const string variableName = "inspectObj";

        /// <summary>
        /// Last syncronized object with UnityPlayer
        /// </summary>
        private object lastSyncObject = null;

        /// <summary>
        /// Apply changes through debugger.ExecuteStatement
        /// </summary>
        /// <param name="debugger"></param>
        public void ApplyChanges(EnvDTE.Debugger debugger)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string[] queries = this.Compare(lastSyncObject);

            if (queries == null || queries.Length == 0) return;

            foreach(var query in queries)
            {
                debugger.ExecuteStatement(query);
            }

            lastSyncObject = this.Clone();
        }

        /// <summary>
        /// Compare and return apply query string array
        /// </summary>
        /// <param name="lastSyncObj">passed in base</param>
        /// <returns></returns>
        protected abstract string[] Compare(object lastSyncObj);

        /// <summary>
        /// Deep copy for variables that will be compared.
        /// </summary>
        /// <returns></returns>
        public abstract object Clone();

        public void Inspect(VsUnityInspector.VsUnityInspector inspector)
        {
            lastSyncObject = this.Clone();

            inspector.control.ParentPanel.Children.Add(GetDisplayPanel());
        }

        protected abstract StackPanel GetDisplayPanel();
    }
}
