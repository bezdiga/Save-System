using System;
using _JoykadeGames.Runtime.SaveSystem.Standlone;

#if UNITY_SWITCH && !UNITY_EDITOR
using _JoykadeGames.Runtime.SaveSystem.Switch;
#endif

namespace _JoykadeGames.Runtime.SaveSystem
{
    public class PlatformInitializerFactory
    {
        public static PlatformInitializer Create()
        {
#if UNITY_SWITCH && !UNITY_EDITOR
            return new SwitchInitializer();
#elif UNITY_PS4 && !UNITY_EDITOR
            throw new Exception("PlayStation 4 platform is not supported in this implementation.");
#elif UNITY_STANDALONE || UNITY_EDITOR
            return new StandaloneInitializer();
#else

        UnityEngine.Debug.LogWarning("Platform not supported, using StandaloneInitializer as fallback.");
        return new StandaloneInitializer();
#endif
        }
    }
}