using UnityEngine;

namespace _JoykadeGames
{
    public class ManagerBase : ScriptableObject
    {
        private static GameObject s_ManagersRoot;

        protected static GameObject GetManagersRoot()
        {
            if (s_ManagersRoot == null)
            {
                s_ManagersRoot = new GameObject("Managers")
                {
                    tag = "GameController"
                };
                
                DontDestroyOnLoad(s_ManagersRoot);
            }

            return s_ManagersRoot;
        }
    }
}