
using System.IO;
using _JoykadeGames.Runtime.SaveSystem;
using _JoykadeGames.Tests.EditMode;
using NUnit.Framework;

namespace _JoykadeGames.Tests.EditMode.Save
{
    [TestFixture]
    public class FileWriteReadTest
    {
        private SerializationAsset _serializationAsset => SerializationUtillity.SerializationAsset;
        private FileWriteRead _writeRead;

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
            _writeRead = new FileWriteRead(SerializationUtillity.SerializationAsset);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_writeRead.GetSaveFolderPath))
            {
                Directory.Delete(_writeRead.GetSaveFolderPath, true);
            }
        }

        [Test]
        public void SerializeData_WritesToFileCorrectly()
        {
            // Arrange
            string saveInfoFileName = _serializationAsset.SaveInfoName + _serializationAsset.SaveExtension;
            SaveDataToFile(saveInfoFileName, out StorableCollection saveBuffer);

            //Act
            _writeRead.SerializeData(saveBuffer, saveInfoFileName);

            //Assert
            Assert.IsTrue(File.Exists(Path.Combine(_writeRead.GetSaveFolderPath, saveInfoFileName)));
            var fileContent = File.ReadAllBytes(Path.Combine(_writeRead.GetSaveFolderPath, saveInfoFileName));
            Assert.IsNotEmpty(fileContent);
        }

        [Test]
        public void LoadData_DeserializeCorrectly()
        {
            // Arrange
            string saveInfoFileName = _serializationAsset.SaveInfoName + _serializationAsset.SaveExtension;
            SaveDataToFile(saveInfoFileName, out StorableCollection saveBuffer);

            //Act
            _writeRead.SerializeData(saveBuffer, saveInfoFileName);
            StorableCollection loadBuffer = _writeRead.LoadFromSaveFile(saveInfoFileName);
            //Assert
            Assert.IsTrue(SaveTestUtility.DataAreEqual(saveBuffer, loadBuffer),
                "Failed: The loaded data do not coincide with the saved data");
        }


        private void SaveDataToFile(string saveInfoFileName, out StorableCollection saveBuffer)
        {
            saveBuffer = new StorableCollection()
            {
                { "key1", 4 },
                { "key2", "String" },
                { "key3", 0.2f },
                { "key", new int[] { 1, 2, 3, 4 } }
            };

            _writeRead.SerializeData(saveBuffer, saveInfoFileName);
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
