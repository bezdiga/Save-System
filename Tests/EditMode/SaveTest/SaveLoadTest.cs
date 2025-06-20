
using _JoykadeGames.Runtime.SaveSystem;
using NUnit.Framework;
using UnityEngine;


namespace _JoykadeGames.Tests.EditMode.Save
{

    [TestFixture]
    public class SaveLoadTest
    {
        private MockSavable _mockSavable;
        

        [Test]
        public void SaveData_IsSaveCorrectly()
        {
            StorableCollection members = new StorableCollection()
            {
                { "level", 5 },
                { "experience", 200.5 }
            };
            _mockSavable = new MockSavable(5,200.5f);
            
            StorableCollection saveData = _mockSavable.OnSave();
            Assert.IsTrue(SaveTestUtility.DataAreEqual(saveData,members),"Failed: members != savedData");
        }
        
        [Test]
        public void LoadData_IsLoadCorrectly()
        {
            StorableCollection members = new StorableCollection()
            {
                { "level", 5 },
                { "experience", 200.5f }
            };
            _mockSavable = new MockSavable(0,0);
            
            _mockSavable.OnLoad(members);
            
            Assert.IsTrue(_mockSavable.level == (int)members["level"],"Failed: The uploaded level is different");
            Assert.IsTrue(_mockSavable.Experience == (float)members["experience"],"Failed: The uploaded experience is different");
        }
    }
}
