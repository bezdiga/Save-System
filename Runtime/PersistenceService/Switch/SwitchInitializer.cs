using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem.Switch
{
    public class SwitchInitializer : PlatformInitializer
    {
        private nn.account.Uid userId;
        private nn.hid.NpadState npadState;
        


        protected override void InitializeCoreServices()
        {
            UnityEngine.Switch.Notification.EnterExitRequestHandlingSection();
            UnityEngine.Switch.Notification.notificationMessageReceived += NotificationMessageReceived;
            // Initialize the Nintendo Switch core services
        }

        void NotificationMessageReceived(UnityEngine.Switch.Notification.Message message)
        {
             Debug.Log("! Notification message received: " + message.ToString()); 
             switch (message)
             {
                 case UnityEngine.Switch.Notification.Message.ExitRequest:
                     ExitRequest(); 
                     return; 
                 // Player returned to the game from the Home screen.
                 case UnityEngine.Switch.Notification.Message.Resume: 
                        //Resume();
                     return; 
             }
       }
        
        #region Notification Handlers
        
        
        void ExitRequest()
        {
            Storage.Dispose();
            UnityEngine.Switch.Notification.LeaveExitRequestHandlingSection();
        }

#endregion
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
            UnityEngine.Debug.Log("Storage for Switch initialized.");
        }
        
        protected override void InitializeTrophies()
        {
            // Implement trophy initialization logic for Switch if applicable
            UnityEngine.Debug.Log("Trophy system not implemented for Switch.");
        }
    }
}