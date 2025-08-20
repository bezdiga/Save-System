namespace _JoykadeGames.Runtime.SaveSystem
{
    public enum FileSystemErrorType
    {
        None, // Operația a avut succes
        PathNotFound,
        UsableSpaceNotEnough,
        TargetLocked,
        Unknown // O altă eroare neașteptată
    }
}