using System.IO;
using _JoykadeGames.Runtime.SaveSystem;
using UnityEngine;

namespace PersistenceService.Standalone
{
    public class StandloneDirectoryProvider : IDirectorySystemProvider
    {
        public void CreateDirectory(string path) => Directory.CreateDirectory(path);

        public void Delete(string path)
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException e)
            {
                Debug.LogError("Failed to delete directory: " + path + " - " + e.Message);
            }
            catch (System.Exception e)
            {
                Debug.LogError("An error occurred while deleting directory: " + path + " - " + e.Message);
            }
        }

        public bool Exists(string path) => Directory.Exists(path);

        public string[] GetDirectories(string path, string searchPattern) => Directory.GetDirectories(path,searchPattern);
    }
}