using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants
{
    public const int byteLengthInBytes = 1;
    public const int shortLengthInBytes = 2;
    public const int intLengthInBytes = 4;
    public const int longLengthInBytes = 8;
    public const int floatLengthInBytes = 4;
    public const int boolLengthInBytes = 1;

    public const int windowSize = 512;
    public const int maxSequenceNumber = 10000;
    public const float timeout = 1f;
    public const int updateFrequency = 10;  // ms
    public const int heartbeatFrequency = 1000;  // ms
    public const int defaultConnectionId = 0;
    public const int serverConnectionId = defaultConnectionId;
    public const int noConnectionId = -1;
    public const int maxPlayers = 50;
    public const int defaultPing = 1000;  // ms
    //public const int resendDelay = 30;  // ms
    public const double resendMultiplier = 1.2;
    public const int maxResends = 30;

    public const int port = 26950;
    public const string ip = "127.0.0.1";

    public const float maxSceneLoadProgress = .9f;

    public const int sioUdpConnectionReset = -1744830452;
}
