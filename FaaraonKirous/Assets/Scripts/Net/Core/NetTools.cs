using System;
using System.Collections.Generic;
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
}
