namespace _JoykadeGames.Runtime.SaveSystem
{
    public interface IDirectorySystemProvider
    {
        void CreateDirectory(string path);
        void Delete(string path);
        bool Exists(string path);
        string[] GetDirectories(string path, string searchPattern);
    }
}