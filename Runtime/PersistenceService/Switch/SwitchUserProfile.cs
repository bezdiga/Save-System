using nn.account;

namespace _JoykadeGames.Runtime.SaveSystem.Switch
{
    public class SwitchUserProfile : IUserProfile
    {
        public IUserId UserId { get; }
        public string DisplayName { get; }
        
        public SwitchUserProfile(Uid switchUid)
        {
            UserId = new SwitchUserId(switchUid);
        }
    }
}