using _JoykadeGames.Runtime.SaveSystem;
using UnityEngine;
public class ExperienceManager : MonoBehaviour,ISaveable
{
    
    public int level;
    public float experience;
    

    public void OnLoad(StorableCollection members)
    {
        level = members.GetT<int>(nameof(level));
        experience = members.GetT<float>(nameof(experience));
    }

    public StorableCollection OnSave()
    {
        StorableCollection members = new StorableCollection()
        {
            { nameof(level), level },
            { nameof(experience), experience}
        };
        return members;
    }
    
    
}
