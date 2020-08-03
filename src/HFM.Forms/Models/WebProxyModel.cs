﻿
using System;
using System.ComponentModel;
using System.Linq;

using HFM.Core.Net;
using HFM.Preferences;

namespace HFM.Forms.Models
{
    public class WebProxyModel : ViewModelBase, IDataErrorInfo
    {
        public IPreferenceSet Preferences { get; }

        public WebProxyModel(IPreferenceSet preferences)
        {
            Preferences = preferences;
        }

        public override void Load()
        {
            ProxyServer = Preferences.Get<string>(Preference.ProxyServer);
            ProxyPort = Preferences.Get<int>(Preference.ProxyPort);
            UseProxy = Preferences.Get<bool>(Preference.UseProxy);
            ProxyUser = Preferences.Get<string>(Preference.ProxyUser);
            ProxyPass = Preferences.Get<string>(Preference.ProxyPass);
            UseProxyAuth = Preferences.Get<bool>(Preference.UseProxyAuth);
        }

        public override void Save()
        {
            Preferences.Set(Preference.ProxyServer, ProxyServer);
            Preferences.Set(Preference.ProxyPort, ProxyPort);
            Preferences.Set(Preference.UseProxy, UseProxy);
            Preferences.Set(Preference.ProxyUser, ProxyUser);
            Preferences.Set(Preference.ProxyPass, ProxyPass);
            Preferences.Set(Preference.UseProxyAuth, UseProxyAuth);
        }

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(ProxyServer):
                    case nameof(ProxyPort):
                        return ValidateProxyServerPort() ? null : ProxyServerPortError;
                    case nameof(ProxyUser):
                    case nameof(ProxyPass):
                        return ValidateProxyUserPass() ? null : ProxyUserPassError;
                    default:
                        return null;
                }
            }
        }

        public override string Error
        {
            get
            {
                var names = new[]
                {
                    nameof(ProxyServer),
                    nameof(ProxyUser)
                };
                var errors = names.Select(x => this[x]).Where(x => x != null);
                return String.Join(Environment.NewLine, errors);
            }
        }

        #region Web Proxy Settings

        private string _proxyServer;

        public string ProxyServer
        {
            get { return _proxyServer; }
            set
            {
                if (ProxyServer != value)
                {
                    string newValue = value == null ? String.Empty : value.Trim();
                    _proxyServer = newValue;
                    OnPropertyChanged(nameof(ProxyPort));
                    OnPropertyChanged();
                }
            }
        }

        private int _proxyPort;

        public int ProxyPort
        {
            get { return _proxyPort; }
            set
            {
                if (ProxyPort != value)
                {
                    _proxyPort = value;
                    OnPropertyChanged(nameof(ProxyServer));
                    OnPropertyChanged();
                }
            }
        }

        public string ProxyServerPortError { get; private set; }

        private bool ValidateProxyServerPort()
        {
            if (UseProxy == false) return true;

            var result = HostName.ValidateNameAndPort(ProxyServer, ProxyPort, out var message);
            ProxyServerPortError = result ? String.Empty : message;
            return result;
        }

        private bool _useProxy;

        public bool UseProxy
        {
            get { return _useProxy; }
            set
            {
                if (UseProxy != value)
                {
                    _useProxy = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProxyAuthEnabled));
                }
            }
        }

        private string _proxyUser;

        public string ProxyUser
        {
            get { return _proxyUser; }
            set
            {
                if (ProxyUser != value)
                {
                    string newValue = value == null ? String.Empty : value.Trim();
                    _proxyUser = newValue;
                    OnPropertyChanged(nameof(ProxyPass));
                    OnPropertyChanged();
                }
            }
        }

        private string _proxyPass;

        public string ProxyPass
        {
            get { return _proxyPass; }
            set
            {
                if (ProxyPass != value)
                {
                    string newValue = value == null ? String.Empty : value.Trim();
                    _proxyPass = newValue;
                    OnPropertyChanged(nameof(ProxyUser));
                    OnPropertyChanged();
                }
            }
        }

        public string ProxyUserPassError { get; private set; }

        private bool ValidateProxyUserPass()
        {
            if (ProxyAuthEnabled == false) return true;

            var result = NetworkCredentialFactory.ValidateRequired(ProxyUser, ProxyPass, out var message);
            ProxyUserPassError = result ? String.Empty : message;
            return result;
        }

        private bool _useProxyAuth;

        public bool UseProxyAuth
        {
            get { return _useProxyAuth; }
            set
            {
                if (UseProxyAuth != value)
                {
                    _useProxyAuth = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProxyAuthEnabled));
                }
            }
        }

        public bool ProxyAuthEnabled
        {
            get { return UseProxy && UseProxyAuth; }
        }

        #endregion
    }
}