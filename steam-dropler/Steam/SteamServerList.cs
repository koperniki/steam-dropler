using System.Collections.Generic;
using System.Linq;
using SteamKit2;
using SteamKit2.Discovery;

namespace steam_dropler.Steam
{
    public static class SteamServerList
    {
        private const int CountForChange = 12;

        private static Dictionary<ServerRecord, int> _servers;

        private static readonly List<ServerRecord> BadRecords = new List<ServerRecord>();

        static SteamServerList()
        {
            var serverList = new SmartCMServerList(SteamConfiguration.Create(b => b.WithProtocolTypes(ProtocolTypes.Tcp)));
            _servers = serverList.GetAllEndPoints().Where(t => t.ProtocolTypes.HasFlag(ProtocolTypes.Tcp)).ToDictionary(t => t, t => 0);
        }

        public static ServerRecord GetServerRecord()
        {

            var validRecord = _servers
                .FirstOrDefault(t => t.Value < CountForChange && !BadRecords.Contains(t.Key)).Key;
            if (validRecord == null)
            {
                var serverList = new SmartCMServerList(SteamConfiguration.Create(b => b.WithProtocolTypes(ProtocolTypes.Tcp)));
                _servers = serverList.GetAllEndPoints()
                    .Where(t => t.ProtocolTypes.HasFlag(ProtocolTypes.Tcp))
                    .ToDictionary(t => t, t => 0);
                BadRecords.Clear();
                return GetServerRecord();
            }
            _servers[validRecord] = _servers[validRecord] + 1;

            return validRecord;
        }


        public static void ReleasServerRecord(ServerRecord serverRecord)
        {
            if (_servers.ContainsKey(serverRecord))
            {
                var uses = _servers[serverRecord];
                uses -= 1;
                if (uses < 0)
                {
                    uses = 0;
                }
                _servers[serverRecord] = uses;

            }
        }


        public static void SetBadServer(ServerRecord record)
        {
            BadRecords.Add(record);
        }
    }
}
