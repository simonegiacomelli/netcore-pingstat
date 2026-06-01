using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;

namespace PingStat
{
    internal class Hosts
    {
        private static readonly string _verboseLog = "verbose.tab";
        private static readonly string sep = "\t";
        private readonly List<Host> _hosts;
        private bool? _onLinePrev;
        private DateTime? _prevChange;

        public Hosts(IniFile ini)
        {
            _hosts = new List<Host>();

            var hostsConf = new NameValueCollection();
            ini.ReadSection("hosts", hostsConf);
            foreach (var hostKey in hostsConf.AllKeys)
            {
                var name = hostsConf.Get(hostKey);
                if (string.IsNullOrEmpty(name))
                    name = hostKey;

                _hosts.Add(new Host {Name = name});
            }
        }

        public bool OnLineStatusChanged { get; private set; }

        public bool OnLine { get; private set; }

        public TimeSpan LastStateSpan { get; private set; }

        public void RefreshPing()
        {
            foreach (var host in _hosts)
                host.Ping();

            OnLine = false;
            foreach (var host in _hosts)
                OnLine |= host.PingSuccess;

            if (!_onLinePrev.HasValue)
            {
                OnLineStatusChanged = true;
                _prevChange = DateTime.Now;
            }
            else
            {
                OnLineStatusChanged = _onLinePrev.Value != OnLine;
                if (OnLineStatusChanged)
                {
                    LastStateSpan = DateTime.Now.Subtract(_prevChange.Value).Duration();
                    _prevChange = DateTime.Now;
                }
            }

            _onLinePrev = OnLine;
        }

        public void WriteVerboseLog()
        {
            if (!File.Exists(_verboseLog))
            {
                var header = "time" + sep + string.Join(sep, _hosts.Select(s => s.Name).ToArray())
                             + Environment.NewLine;
                File.AppendAllText(_verboseLog, header);
            }

            var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) + sep
                + string.Join(sep, HostsToValues()) + Environment.NewLine;
            File.AppendAllText(_verboseLog, line);
        }

        private string[] HostsToValues()
        {
            return _hosts.Select(h => Convert.ToString(h.PingSuccess ? h.Time : -1)).ToArray();
        }

        public void OnLineStatusClear()
        {
            OnLineStatusChanged = false;
        }
    }
}