using _JoykadeGames.Runtime.SaveSystem;
using NUnit.Framework;

namespace _JoykadeGames.Tests.EditMode
{
    public static class SaveTestUtility
    {
        public static bool DataAreEqual(StorableCollection saveBuffer, StorableCollection loadBuffer)
        {
            if (saveBuffer.Count != loadBuffer.Count)
            {
                TestContext.WriteLine($"Number of elements not equals: dict1 = {saveBuffer.Count}, dict2 = {loadBuffer.Count}");
                return false;
            }

            foreach (var kvp in saveBuffer)
            {
                if (!loadBuffer.ContainsKey(kvp.Key))
                {
                    TestContext.WriteLine($"Load Data not contain key: {kvp.Key} ");
                    return false;
                }

                if (!saveBuffer.ContainsKey(kvp.Key))
                {
                    TestContext.WriteLine($"Save Data not contain key: {kvp.Key} ");
                    return false;
                }
            }

            return true;
        }
    }
}