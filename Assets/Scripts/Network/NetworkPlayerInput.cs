using PRN;
using Unity.Netcode;

namespace TBS.Network
{
    public struct NetworkPlayerInput : IInput, INetworkSerializable
    {
        public int tick;
        public float forward;
        public float right;
        public float cameraYAngle;
        public bool jump;

        public void SetTick(int tick) => this.tick = tick;
        public int GetTick() => tick;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref cameraYAngle);
            serializer.SerializeValue(ref tick);
            serializer.SerializeValue(ref forward);
            serializer.SerializeValue(ref right);
            serializer.SerializeValue(ref jump);
        }
    }
}
