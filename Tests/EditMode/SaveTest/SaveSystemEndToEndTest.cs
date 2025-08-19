using System.IO;
using _JoykadeGames.Runtime.SaveSystem;
using _JoykadeGames.Runtime.SaveSystem.Standlone;
using NUnit.Framework;

namespace _JoykadeGames.Tests.EditMode.Save
{
    [TestFixture]
    public class SaveSystemEndToEndTest
    {
        private SerializationAsset _serializationAsset => SerializationUtillity.SerializationAsset;
        private FileWriteRead _writeRead;
        private MockSavable _savable;
        
        private string SavedGamePath
        {
            get
            {
                string savesPath = _serializationAsset.GetSavesPath();
                if (!Directory.Exists(savesPath))
                    Directory.CreateDirectory(savesPath);

                return savesPath;
            }
        }

        [SetUp]
        public void SetUp()
        {
            /*GetSavedFolder(out string saveFolderName, 0);
            string saveFolderPath = Path.Combine(SavedGamePath, saveFolderName);
            Directory.CreateDirectory(saveFolderPath);*/
            _writeRead = new FileWriteRead(new StandaloneParams(SerializationUtillity.SerializationAsset));
            _savable = new MockSavable(0, 0);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_writeRead.GetSaveFolderPath))
            {
                Directory.Delete(_writeRead.GetSaveFolderPath,true);
            }
        }

        [Test]
        [TestCase(4,101.1f)]
        public void SaveSystem_EndToEnd(int level, float experience)
        {
            string saveInfoFileName = _serializationAsset.SaveInfoName + _serializationAsset.SaveExtension;
            _savable.level = level;
            _savable.Experience = experience;
            
            //save
            var saveBuffer = _savable.OnSave();
            _writeRead.SerializeData(saveBuffer,saveInfoFileName);
            
            //load
            var loadBuffer = _writeRead.LoadFromSaveFile(saveInfoFileName);
            
            Assert.IsTrue(SaveTestUtility.DataAreEqual(saveBuffer,loadBuffer),"Failed: The saved data are differend from load data");
        }
        
        private void GetSavedFolder(out string saveFolderName, int savedId)
        {
            saveFolderName = _serializationAsset.SaveFolderPrefix;

            if (!_serializationAsset.SingleSave)
            {
                string[] directories = Directory.GetDirectories(SavedGamePath, $"{saveFolderName}*");
                saveFolderName += directories.Length.ToString("D3");
            }
            else
            {
                saveFolderName += savedId.ToString("D3");
            }
        }
    }
}