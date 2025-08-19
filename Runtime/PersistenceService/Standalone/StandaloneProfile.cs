using _JoykadeGames.Runtime.SaveSystem;
using _JoykadeGames.Runtime.SaveSystem.Standlone;

namespace PersistenceService.Standalone
{
    public class StandaloneProfile : IUserProfile
    {
        public IUserId UserId { get; }
        public string DisplayName { get; }
        
        public StandaloneProfile()
        {
            UserId = new StandaloneUser();
        }
    }
}