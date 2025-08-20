using _JoykadeGames.Runtime.SaveSystem;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Sample
{
    public class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private SaveGameManager saveGameManager;
        [SerializeField] private ExperienceManager experienceManager;
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(saveGameManager);
            builder.RegisterComponent(experienceManager);
        }
    }
}