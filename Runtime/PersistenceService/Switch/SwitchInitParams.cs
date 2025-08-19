namespace _JoykadeGames.Runtime.SaveSystem.Switch
{
    public class SwitchInitParams : WriterReaderParams
    {
        public nn.account.Uid UserId { get; private set; }
        
        public SwitchInitParams(IUserId userId,SerializationAsset serializationAsset) : base(serializationAsset)
        {
            if (userId is SwitchUserId switchId)
            {
                nn.account.Uid nativeUid = switchId.NativeId;
                UserId = nativeUid;
            }
        }
    }
}