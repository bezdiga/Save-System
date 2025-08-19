namespace _JoykadeGames.Runtime.SaveSystem
{
    public abstract class WriterReaderParams
    {
        public SerializationAsset SerializationAsset { get; private set; }
        
        public WriterReaderParams(SerializationAsset serializationAsset)
        {
            SerializationAsset = serializationAsset;
        }
    }
}