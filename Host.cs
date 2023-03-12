using System;
using System.Net.NetworkInformation;

namespace PingStat
{
    internal class Host
    {
        internal string Name;
        internal bool PingSuccess;
        internal long Time { get; set; }
        
        Ping _pingSender;

        public void Ping()
        {
            _pingSender ??= new Ping();

            try
            {
                PingReply reply = _pingSender.Send(Name, 1500);
                PingSuccess = reply.Status == IPStatus.Success;
                Time = reply.RoundtripTime;
            }
            catch (Exception ex)
            {
                PingSuccess = false;
            }
        }

    }
}