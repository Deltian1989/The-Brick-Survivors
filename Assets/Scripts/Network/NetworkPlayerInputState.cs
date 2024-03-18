using Unity.Netcode;

namespace TBS.Network
{
    public class NetworkPlayerInputState : INetworkSerializable
    {
        public NetworkPlayerInput input;
        public NetworkPlayerState state;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref input);
            serializer.SerializeValue(ref state);
        }
    }
}
