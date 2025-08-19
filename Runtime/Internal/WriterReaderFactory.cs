using _JoykadeGames.Runtime.SaveSystem.Standlone;
using _JoykadeGames.Runtime.SaveSystem.Switch;
using UnityEngine;

namespace _JoykadeGames.Runtime.SaveSystem
{
    public static class WriterReaderFactory
    {
        private static IWriterReader _instance;

        public static IWriterReader GetWriter(WriterReaderParams readerParams)
        {
            if (_instance == null)
            {
                if(readerParams is SwitchInitParams switchParams)
                {
                    _instance = new SwitchFileWriteReader(switchParams);
                }
                else if(readerParams is StandaloneParams standloneParams)
                {
                    _instance = new FileWriteRead(standloneParams);
                }
                else Debug.LogError("SaveSystem Error: WriterReaderFactory - Unsupported platform or parameters provided.");
            }
            return _instance;
        }
    }
}