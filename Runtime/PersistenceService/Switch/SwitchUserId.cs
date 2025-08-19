using nn.account;

namespace _JoykadeGames.Runtime.SaveSystem.Switch
{
    public class SwitchUserId : IUserId
    {
        public Uid NativeId { get; }

        public SwitchUserId(Uid nativeId)
        {
            NativeId = nativeId;
        }

        public override string ToString()
        {
            return NativeId.ToString();
        }
    }
}