using _JoykadeGames.Runtime.SaveSystem;

namespace _JoykadeGames.Tests.EditMode.Save
{
    public class MockSavable : ISaveable
    {
        public int level;
        private float experience;
        
        public UniqueID UniqueID { get; set; }
        public float Experience
        {
            get => experience;
            set => experience = value;
        }

        public MockSavable(int level, float experience)
        {
            this.level = level;
            this.experience = experience;
            UniqueID = new UniqueID();
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
                { nameof(experience), experience }
            };
            return members;
        }
        
    }
}