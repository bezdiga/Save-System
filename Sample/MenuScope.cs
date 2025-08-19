using _JoykadeGames.Runtime.SaveSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MenuScope : LifetimeScope
{
    [SerializeField] private SaveLoader saveLoader;
    protected override void Configure(IContainerBuilder builder)
    {
        base.Configure(builder);
        builder.RegisterComponent(saveLoader);
    }
}
