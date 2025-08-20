using System;
using System.IO;
using _JoykadeGames.Runtime.SaveSystem;

namespace PersistenceService.Standalone
{
    public class StandaloneFileSystemProvider : IFileSystemProvider
    {
        public bool Exists(string path) => File.Exists(path);

        public bool WriteFile(string path, byte[] data)
        {
            try
            {
                File.WriteAllBytes(path, data);
                return true;
            }
            catch (IOException ex)
            {
                UnityEngine.Debug.LogError($"Failed to write file at {path}: {ex.Message}");
                return false;
            }
        }

        public bool ReadFile(string path, out byte[] data)
        {
            try
            {
                data = File.ReadAllBytes(path);
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to read file at {path}: {ex.Message}");
                data = null;
                return false;
            }
        }
    }
}