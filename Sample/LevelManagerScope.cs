using _JoykadeGames.Runtime.SaveSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class LevelManagerScope : LifetimeScope
{
    [SerializeField] private LevelManager levelManager;
    
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.RegisterComponent(levelManager);
    }
}
