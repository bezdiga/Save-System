using _JoykadeGames.Runtime.SaveSystem.Nitendo;
using PersistenceService.Switch;
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem.Switch
{
    public class SwitchInitializer : PlatformInitializer
    {
        private nn.account.Uid userId;
        private nn.hid.NpadState npadState;
        


        protected override void InitializeCoreServices()
        {
            // Initialize the Nintendo Switch core services
        }

        protected override void InitializeUser()
        {
            nn.account.Account.Initialize();
            nn.account.UserHandle userHandle = new nn.account.UserHandle();

            Debug.LogError("Initialize User");
            if (!nn.account.Account.TryOpenPreselectedUser(ref userHandle))
            {
                nn.Nn.Abort("Failed to open preselected user.");
            }
            nn.Result result = nn.account.Account.GetUserId(ref userId, userHandle);
            result.abortUnlessSuccess();
            CurrentUser = new SwitchUserProfile(userId);
            npadState = new nn.hid.NpadState();
        }

        protected override void InitializeSave()
        {
            var userId = (CurrentUser as SwitchUserProfile)?.UserId; // Presupunând că ai o clasă SwitchUserProfile
            var parameters = new SwitchInitParams(userId,SerializationUtillity.SerializationAsset);
            Storage = new SwitchFileWriteReader(parameters);
            DirectorySystemProvider = new SwitchDirectoryProvider();
            FileSystemProvider = new SwitchFileSystem();
            UnityEngine.Debug.Log("Storage for Switch initialized.");
        }
        
        protected override void InitializeTrophies()
        {
            // Implement trophy initialization logic for Switch if applicable
            UnityEngine.Debug.Log("Trophy system not implemented for Switch.");
        }
    }
}