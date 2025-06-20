using System.Linq;
using UnityEngine;

namespace _JoykadeGames
{
    public class Manager<T> : ManagerBase where T : Manager<T>
    {
#if UNITY_EDITOR
        protected static T Instance { get; private set; }
#else
        protected static T Instance;
#endif
        private const string k_DefaultManagerPath = "Managers/";

        protected static void SetInstance(string path = k_DefaultManagerPath)
        {
            if (path == null)
                path = k_DefaultManagerPath;
            var managers = Resources.LoadAll<T>(path);

            Instance = managers.FirstOrDefault();

            if (Instance == null)
                Instance = CreateInstance<T>();
            Instance.OnInitialized();
        }

        protected static void CreateInstance()
        {
            Instance = CreateInstance<T>();
            Instance.OnInitialized();
        }

        protected U CreateRuntimeObject<U>(string objName = "RuntimeObject") where U : MonoBehaviour
        {
            var managersRoot = GetManagersRoot();
            var runtimeObject = new GameObject(objName).AddComponent<U>();
            runtimeObject.transform.SetParent(managersRoot.transform);
            runtimeObject.gameObject.name = objName;

            return runtimeObject;
        }
        protected virtual void OnInitialized() {}
    }
}
