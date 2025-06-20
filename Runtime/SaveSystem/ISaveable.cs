namespace _JoykadeGames.Runtime.SaveSystem
{
    /*Folositi interfata doar pe obiectele din scena nu si pe prefab*/
    public interface ISaveable
    {
        void OnLoad(StorableCollection members);
        StorableCollection OnSave();
    }

    public interface IRuntimeSaveable
    {
        
    }
    
}