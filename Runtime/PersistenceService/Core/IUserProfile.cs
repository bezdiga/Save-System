namespace _JoykadeGames.Runtime.SaveSystem
{
    public interface IUserProfile
    {
        /// <summary>
        /// Gets the unique identifier for the user profile.
        /// </summary>
        IUserId UserId { get; }

        /// <summary>
        /// Gets the display name of the user profile.
        /// </summary>
        string DisplayName { get; }
        
    }
}