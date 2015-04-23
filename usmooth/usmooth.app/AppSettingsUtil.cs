using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace usmooth.app
{
    public class AppSettingsUtil
    {
        public static string Localhost = "127.0.0.1";

        public static bool AppendAsRecentlyConnected(string remoteAddr)
        {
            if (Properties.Settings.Default.RecentAddrList == null)
            {
                Properties.Settings.Default.RecentAddrList = new System.Collections.Specialized.StringCollection();
            }
            else
            {
                if (remoteAddr == Localhost || Properties.Settings.Default.RecentAddrList.Contains(remoteAddr))
                    return false;
            }

            Properties.Settings.Default.RecentAddrList.Add(remoteAddr);
            Properties.Settings.Default.Save();
            return true;
        }
    }
}
