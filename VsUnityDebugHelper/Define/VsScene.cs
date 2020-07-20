using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsUnityDebugHelper.Define
{
    class VsScene
    {
        const string sceneTypeName = "{UnityEngine.SceneManagement.Scene}";

        public int buildIndex;
        public int handle;
        public bool isDirty;
        public bool isLoaded;
        public string name;
        public string path;
        public int rootCount;

        public VsGameObject[] rootGameObjects;

        public VsScene(string sceneInfo)
        {
            #region Parse Scene
            string[] sceneInfos = sceneInfo.SplitAndTrim("\r\n", StringSplitOptions.RemoveEmptyEntries);

            if(sceneInfos[0] != sceneTypeName)
            {
                throw new ArgumentException($"Unexpected Type name is printed\n\tExpected:{sceneTypeName}\n\tReal:{sceneInfos[0]}");
            }

            string name, value;
            // last element is always ">"
            for (int i = 1; i < sceneInfos.Length - 1; ++i) // 0 is Scene type checker
            {
                bool isSuccess;
                (name, value) = sceneInfos[i].TryGetFieldInfo(out isSuccess);
                if(!isSuccess)
                {
                    throw new ArgumentException($"Unexpected format of value is printed\n\tExpected:(name): (value)\n\tRead:{sceneInfos[i]}");
                }

                switch(name)
                {
                    case nameof(buildIndex):
                        buildIndex = int.Parse(value);
                        break;
                    case nameof(handle):
                        handle = int.Parse(value);
                        break;
                    case nameof(isDirty):
                        isDirty = bool.Parse(value);
                        break;
                    case nameof(isLoaded):
                        isLoaded = bool.Parse(value);
                        break;
                    case nameof(name):
                        this.name = value.Trim('"');
                        break;
                    case nameof(path):
                        path = value.Trim('"');
                        break;
                    case nameof(rootCount):
                        rootCount = int.Parse(value);
                        break;
                }
            }

            #endregion Parse Scene
        }
    }
}
