using System.Threading.Tasks;


namespace _JoykadeGames.Runtime.SaveSystem
{
    public interface IWriterReader
    {
        public void SerializeData(StorableCollection buffer, string path);
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