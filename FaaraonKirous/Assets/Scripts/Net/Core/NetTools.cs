using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class NetTools
{
    public static string GetMasterServerIP()
    {
        try
        {
            IPHostEntry hosts = Dns.GetHostEntry(Constants.masterServerDNS);
            if (hosts.AddressList.Length > 0) return hosts.AddressList[0].ToString();
        }
        catch
        {
            Debug.Log("Could not get IP of master server");
        }
        return null;
    }

    public static IPEndPoint CreateIPEndPoint(string endPoint)
    {
        string[] ep = endPoint.Split(':');
        if (ep.Length != 2) throw new FormatException("Invalid endpoint format");
        IPAddress ip;
        if (!IPAddress.TryParse(ep[0], out ip))
        {
            throw new FormatException("Invalid ip-adress");
        }
        int port;
        if (!int.TryParse(ep[1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port))
        {
            throw new FormatException("Invalid port");
        }
        return new IPEndPoint(ip, port);
    }
}
