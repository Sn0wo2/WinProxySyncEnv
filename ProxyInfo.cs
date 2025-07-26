namespace WinProxyEnvSync
{
    public class ProxyInfo
    {
        public ProxyInfo(bool proxyEnable, string proxyServer, string proxyOverride)
        {
            ProxyEnable = proxyEnable;
            ProxyServer = proxyServer;
            ProxyOverride = proxyOverride;
        }

        public bool ProxyEnable { get; }
        public string ProxyServer { get; }
        public string ProxyOverride { get; }

        public override bool Equals(object obj)
        {
            if (obj is ProxyInfo other)
                return ProxyEnable == other.ProxyEnable &&
                       ProxyServer == other.ProxyServer &&
                       ProxyOverride == other.ProxyOverride;

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = 17;
                hashCode = hashCode * 23 + ProxyEnable.GetHashCode();
                hashCode = hashCode * 23 + (ProxyServer?.GetHashCode() ?? 0);
                hashCode = hashCode * 23 + (ProxyOverride?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}