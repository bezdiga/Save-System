
using VContainer.Unity;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public abstract class PlatformInitializer : IInitializable
    {
        public IWriterReader Storage { get; protected set; }
        public IUserProfile CurrentUser { get; protected set; }
        //public ITrophyService TrophyService { get; protected set; }
        
        /// <summary>
        /// Metoda șablon. Definește ordinea operațiunilor de inițializare.
        /// Aceasta este singura metodă pe care o va apela codul de start al jocului.
        /// </summary>
        public void Initialize()
        {
            InitializeCoreServices();
            InitializeUser();
            InitializeSave();
            InitializeTrophies();
        }
        
        /// <summary>
        /// Inițializează serviciile de bază ale platformei.
        /// </summary>
        protected abstract void InitializeCoreServices();

        /// <summary>
        /// Gestionează logica de obținere și setare a utilizatorului activ.
        /// </summary>
        protected abstract void InitializeUser();

        /// <summary>
        /// Creează și configurează sistemul de citire/scriere (folosind factory-ul anterior).
        /// </summary>
        protected abstract void InitializeSave();

        /// <summary>
        /// Inițializează sistemul de trofee/achievements, dacă există.
        /// </summary>
        protected virtual void InitializeTrophies(){}

        /*public void Start()
        {
            Initialize();
        }*/
    }
}