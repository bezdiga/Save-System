using PersistenceService.Standalone;
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem.Standlone
{
    public class StandaloneInitializer : PlatformInitializer
    {
        protected override void InitializeCoreServices()
        {
            Debug.LogWarning("Core services initialization is not implemented for Standalone platform.");
        }

        protected override void InitializeUser()
        {
            Debug.LogError("Initialize Standalone user profile.");
            CurrentUser = new StandaloneProfile();
        }

        protected override void InitializeSave()
        {
            var parameters = new StandaloneParams(SerializationUtillity.SerializationAsset);
            Storage = new FileWriteRead(parameters);
            UnityEngine.Debug.Log("Storage for Standalone initialized.");
        }
    }
}