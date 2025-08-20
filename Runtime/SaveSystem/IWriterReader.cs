using System;
using System.Threading.Tasks;


namespace _JoykadeGames.Runtime.SaveSystem
{
    public interface IWriterReader : IDisposable
    {
        public void SerializeData(StorableCollection buffer, string path);
        void TryDeserializeGameStateAsync(string folderName);
        public StorableCollection LoadFromSaveFile(string path);
        public Task<SavedGameInfo[]> ReadAllSaves();
        public Task RemoveAllSaves();
        
        void StartSaveOperation();
        void EndSaveOperation();
    }
    
    /*public interface IReader
    {
        public StorableCollection LoadFromSaveFile(string path);
        public Task<SavedGameInfo[]> ReadAllSaves();
    }*/
}