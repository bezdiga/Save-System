using System.IO;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public interface IFileSystemProvider
    {

        /// <summary>
        /// Checks if a file exists at the specified path.
        /// </summary>
        /// <param name="path">The path of the file to check.</param>
        /// <returns>True if the file exists, otherwise false.</returns>
        bool Exists(string path);
        
        /// <summary>
        /// Scrie datele și returnează true dacă a reușit.
        /// Complexitatea (FileStream vs nn.Result) este ascunsă în implementare.
        /// </summary>
        bool WriteFile(string path, byte[] data);

        /// <summary>
        /// Citește datele și returnează true dacă a reușit.
        /// </summary>
        bool ReadFile(string path, out byte[] data);
    }
}