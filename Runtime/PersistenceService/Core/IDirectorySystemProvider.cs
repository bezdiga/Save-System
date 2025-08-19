namespace _JoykadeGames.Runtime.SaveSystem
{
    public interface IDirectorySystemProvider
    {
        void CreateDirectory(string path);
        void DeleteDirectory(string path);
        bool DirectoryExists(string path);
    }
}