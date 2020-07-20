using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace VsUnityDebugHelper.Define
{
    class VsGameObject : VsUnityObjectBase
    {
        private const string gameObjectTypeName = "UnityEngine.GameObject";

        // Variables for hierarchy
        public int childCount;
        public bool hasChild => childCount != 0;
        public bool isInit = false; // To remove dummy object when expanded.

        // Variables for Inspector
        public bool activeSelf;

        // both
        public string name;

        public VsGameObject() { }

        public VsGameObject(string objInfo)
        {
            (string objName, string objType) parseGameObjectHeader(string input)
            {
                input = input.Trim('"');

                int left = input.LastIndexOf('(');
                int right = input.LastIndexOf(')');

                if(left == -1 || right == -1)
                {
                    return ("", "");
                }

                string _name = input.Substring(0, left - 1);
                string _type = input.Substring(left + 1, right - left - 1);

                return (_name, _type);
            }

            string[] infos = objInfo.SplitAndTrim("\r\n", StringSplitOptions.RemoveEmptyEntries);

            (string objName, string objType) = parseGameObjectHeader(infos[0]);
            this.name = objName;

            if (objType != gameObjectTypeName)
            {
                throw new ArgumentException($"Unexpected Type name is printed\n\tExpected:{gameObjectTypeName}\n\tReal:{infos[0]}");
            }

            string name, value;
            // last element is always ">"
            for (int i = 1; i < infos.Length - 1; ++i)
            {
                bool isSuccess;
                (name, value) = infos[i].TryGetFieldInfo(out isSuccess);
                if (!isSuccess)
                {
                    throw new ArgumentException($"Unexpected format of value is printed\n\tExpected:(name): (value)\n\tRead:{infos[i]}");
                }

                switch (name)
                {
                    case nameof(activeSelf):
                        activeSelf = bool.Parse(value);
                        break;
                }
            }
        }

        protected override string[] Compare(object lastSyncObj)
        {
            VsGameObject comp = lastSyncObj as VsGameObject;
            if (comp == null) return null;

            List<string> queries = new List<string>();

            if(this.activeSelf != comp.activeSelf)
            {
                queries.Add($"{variableName}.SetActive({this.activeSelf.ToString().ToLower()});");
            }
            // add other changes below

            return queries.ToArray();
        }

        public override object Clone()
        {
            VsGameObject go = new VsGameObject();
            go.activeSelf = this.activeSelf;
            return go;
        }

        protected override StackPanel GetDisplayPanel()
        {
            StackPanel panel = new StackPanel();
            panel.Margin = new System.Windows.Thickness(16);

            // Checkbox for isActive
            CheckBox checkBox = new CheckBox();
            checkBox.IsChecked = this.activeSelf;
            checkBox.Checked += OnChecked;
            checkBox.Unchecked += OnUnChecked;
            checkBox.Content = this.name;

            panel.Children.Add(checkBox);

            return panel;
        }

        private void OnChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox == null) return;

            this.activeSelf = true;
        }

        private void OnUnChecked(object sender, System.Windows.RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            if (checkBox == null) return;

            this.activeSelf = false;
        }
    }
}
