using BrilliantSkies.Core;
using BrilliantSkies.Ftd.Multiplayer.NetworkCommunication;
using BrilliantSkies.Ui.Special.InfoStore;
using NetInfrastructure;

namespace AdventurePatch
{
    public static class NetworkedInfoStore
    {
        private static INetworkIdentity _identity;

        static NetworkedInfoStore()
        {
            if (Net.IsConnected)
            {
                _identity = NetSys.System.NetworkIdentities.MakeOrGetUniqueIdentity(0u);
            }
        }
        public static void Add(string message, float duration = 4.5f)
        {
            int hash = message.GetHashCode();
            Add(hash, message, duration);
        }
        public static void Add(int hash, string message, float duration = 4.5f)
        {
            InfoStore.Add(hash, message);
            if (Net.IsConnected && _identity != null)
            {
                ServerOutgoingRpcs.SendHUDInfoMessage(_identity, message);
            } 
        }
        public static void Add(int hash, string str1, string str2, float duration = 4.5f)
        {
            string message = $"{str1}{str2}";
            Add(hash, message, duration);
        }

        public static bool IsMultiplayer => Net.IsConnected;
    }
}