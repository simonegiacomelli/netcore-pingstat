using System;
using System.Net.NetworkInformation;

namespace PingStat
{
    internal class Host
    {
        internal string Name;
        internal bool PingSuccess;
        internal long Time { get; set; }
        
        private static byte[] _buffer = new byte[32];
        static readonly PingOptions Options = new PingOptions();

        Ping _pingSender;

        /// <summary>
        /// 
        /// </summary>
        static Host()
        {
            for (int idx = 0; idx < _buffer.Length; idx++)
                _buffer[idx] = (byte) 'a';
        }

        public void Ping()
        {
            if (_pingSender == null)
            {
                _pingSender = new Ping();
                Options.DontFragment = true;
            }
            
            try
            {
                PingReply reply = _pingSender.Send(Name, 1500, _buffer, Options);
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