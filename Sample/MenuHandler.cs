using _JoykadeGames.Runtime.SaveSystem;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public void SaveGame()
    {
        SaveGameManager.SaveTest();
    }
    public void LaodGame()
    {
        SaveGameManager.LoadInfo();
    }
}
