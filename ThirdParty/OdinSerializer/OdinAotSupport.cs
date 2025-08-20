using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OdinSerializer;
using UnityEngine;
using UnityEngine.Scripting;

public class OdinAotSupport
{
    [Preserve]
    private static void AotStubs()
    {
        Assembly assembly = typeof(WeakDictionaryFormatter).Assembly;
        var types = assembly.GetTypes()
            .Where(t => t != null
                        && typeof(IFormatter).IsAssignableFrom(t)
                        && t.IsClass
                        && !t.IsAbstract)
            .ToList();
        for (int i = 0; i < types.Count; i++)
        {
            _ = types[i];
            //Debug.Log("Registering AOT stub for formatter: " + types[i].FullName);
        }
        
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        AotStubs();
    }
}
