using System;
using System.IO;
using _JoykadeGames.Runtime.SaveSystem;

namespace PersistenceService.Standalone
{
    public class StandaloneFileSystemProvider : IFileSystemProvider
    {
        private string rootDirectory;
        public StandaloneFileSystemProvider(string mountName)
        {
            rootDirectory = mountName;
        }
        private string GetFullPath(string relativePath)
        {
            return Path.Combine(rootDirectory, relativePath);
        }
        public bool Exists(string path) => File.Exists(path);

        public bool WriteFile(string path, byte[] data)
        {
            try
            {
                File.WriteAllBytes(GetFullPath(path), data);
                return true;
            }
            catch (IOException ex)
            {
                UnityEngine.Debug.LogError($"Failed to write file at {GetFullPath(path)}: {ex.Message}");
                return false;
            }
        }

        public bool ReadFile(string path, out byte[] data)
        {
            try
            {
                data = File.ReadAllBytes(GetFullPath(path));
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to read file at {GetFullPath(path)}: {ex.Message}");
                data = null;
                return false;
            }
        }
    }
}