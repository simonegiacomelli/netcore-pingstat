using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Text;
using System.Linq;

namespace PingStat
{
    internal class Hosts
    {
        private List<Host> _hosts;
        private static string _verboseLog = "verbose.tab";
        private bool _onLineStatusChanged;
        private bool _onLine;
        private bool? _onLinePrev;
        private DateTime? _prevChange;
        private TimeSpan _lastStateSpan;
        private static string sep = "\t";
        public Hosts(IniFile ini)
        {
            _hosts = new List<Host>();

            NameValueCollection hostsConf = new NameValueCollection();
            ini.ReadSection("hosts", hostsConf);
            foreach (var hostKey in hostsConf.AllKeys)
            {

                string name = hostsConf.Get(hostKey);
                if (string.IsNullOrEmpty(name))
                    name = hostKey;

                _hosts.Add(new Host { Name = name });
            }
        }

        public bool OnLineStatusChanged
        {
            get
            {
                return _onLineStatusChanged;
            }
        }

        public bool OnLine
        {
            get
            {
                return _onLine;
            }
        }

        public TimeSpan LastStateSpan
        {
            get { return _lastStateSpan; }
        }

        public void RefreshPing()
        {
            foreach (var host in _hosts)
                host.Ping();

            _onLine = false;
            foreach (var host in _hosts)
                _onLine |= host.PingSuccess;

            if (!_onLinePrev.HasValue)
            {
                _onLineStatusChanged = true;
                _prevChange = DateTime.Now;
            }
            else
            {
                _onLineStatusChanged = _onLinePrev.Value != _onLine;
                if (_onLineStatusChanged)
                {
                    _lastStateSpan = DateTime.Now.Subtract(_prevChange.Value).Duration();
                    _prevChange = DateTime.Now;
                }
            }

            _onLinePrev = _onLine;

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
            _onLineStatusChanged = false;
        }
    }
}