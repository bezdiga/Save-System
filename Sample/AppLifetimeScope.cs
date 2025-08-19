using System.Threading.Tasks;
using _JoykadeGames.Runtime.SaveSystem;
using UnityEngine.SceneManagement;
using VContainer;
using VContainer.Unity;

namespace Sample
{
    public class AppLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            var initializer = PlatformInitializerFactory.Create();

            initializer.Initialize();
            
            builder.RegisterInstance(initializer).As<PlatformInitializer>();
            builder.RegisterInstance(initializer.Storage).As<IWriterReader>();
            builder.RegisterInstance(initializer.CurrentUser).As<IUserProfile>();
            
            /*builder.Register(resolver =>
            {
                return PlatformInitializerFactory.Create();
            }, Lifetime.Transient).As<PlatformInitializer>();

            builder.Register(resolver =>
            {
                var platformInitializer = resolver.Resolve<PlatformInitializer>();

                platformInitializer.Initialize();

                return platformInitializer.Storage;

            }, Lifetime.Singleton).As<IWriterReader>();;


            builder.Register(resolver =>
            {
                // Similar, putem înregistra și profilul utilizatorului
                var platformInitializer = resolver.Resolve<PlatformInitializer>();
                return platformInitializer.CurrentUser;

            },Lifetime.Singleton).As<IUserProfile>();*/

        }
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            SceneManager.LoadScene(1);
        }
    }
    
}