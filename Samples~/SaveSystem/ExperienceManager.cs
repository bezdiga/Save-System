using System;
using _JoykadeGames.Runtime.SaveSystem;

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using VContainer;

public class ExperienceManager : MonoBehaviour,ISaveable
{
    public int level;
    public float experience;
    private GUIStyle customStyle;
    private GUIStyle buttonStyle;

    private IWriterReader _writerReader;
    private void OnGUI()
    {
        if (customStyle == null)
        {
            customStyle = new GUIStyle(GUI.skin.label);
            customStyle.fontSize = 42;
            customStyle.normal.textColor = Color.white;
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 36;
        }
        int width = 400;
        int height = 150;
        int x = (Screen.width - width) / 2;
        int y = (Screen.height - height) / 2;

        GUI.Label(new Rect(x, y, width, height), $"Level: {level} \n Experience: {experience}",customStyle);
        
        if(GUI.Button(new Rect(x - 255, y + 150, 300, 80), "Add (10 exp)",buttonStyle))
        {
            GainExperience(10);
        }
        
        if(GUI.Button(new Rect(x + 255, y +150, 250, 80), "Save",buttonStyle))
        {
            SaveGameManager.SaveGame();
        }
        
        if(GUI.Button(new Rect(x + 510, y +150, 250, 80), "Menu",buttonStyle))
        {
            LoadGame();
        }
        if(GUI.Button(new Rect(x + 765, y +150, 250, 80), "Unmount",buttonStyle))
        {
            Unmount();
        }
    }

    [Inject]
    public void Construct(IWriterReader writeRead)
    {
        _writerReader = writeRead;
    }
    private void Update()
    {
        if (Gamepad.current != null) 
        {
            if (Gamepad.current.dpad.up.wasPressedThisFrame)
            {
                GainExperience(10);
            }
            
            if(Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                SaveGameManager.SaveGame();
                Debug.LogError("Save GAme");
            }
        }
    }

    private void GainExperience(int exp)
    {
        experience += exp;
        if (experience >= 50) // Assuming 50 experience points are needed to level up
        {
            level++;
            experience -= 50; // Reset experience after leveling up
        }
    }

    public void LoadGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Unmount()
    {
        _writerReader.Dispose();
    }
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
