using System;
using System.Threading.Tasks;


namespace _JoykadeGames.Runtime.SaveSystem
{
    public interface IWriterReader : IDisposable
    {
        public IDirectorySystemProvider Directory { get; }
        public IFileSystemProvider File { get; }
        public void SerializeData(StorableCollection buffer, string path);
        void TryDeserializeGameStateAsync(string folderName);
        public StorableCollection LoadFromSaveFile(string path);
        public Task<SavedGameInfo[]> ReadAllSaves();
        public Task RemoveAllSaves();
        
    }
    
    /*public interface IReader
    {
        public StorableCollection LoadFromSaveFile(string path);
        public Task<SavedGameInfo[]> ReadAllSaves();
    }*/
}