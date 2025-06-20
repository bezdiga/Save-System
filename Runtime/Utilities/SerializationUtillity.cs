
using _JoykadeGames.Code.Runtime.Scriptables;
using UnityEngine;

public static class SerializationUtillity
{
    private static SerializationAsset serializationAsset;
    private static ObjectDataBase serializationObjectDatabase;
    public static SerializationAsset SerializationAsset
    {
        get
        {
            if (serializationAsset == null)
                serializationAsset = Resources.Load<SerializationAsset>("Data/SerializationAsset");
            
            return serializationAsset;
        }
    }
    
    public static ObjectDataBase SerializationObjectDatabase
    {
        get
        {
            if (serializationObjectDatabase == null)
                serializationObjectDatabase = Resources.Load<ObjectDataBase>("Data/ObjectDatabase");
            
            return serializationObjectDatabase;
        }
    }
}
