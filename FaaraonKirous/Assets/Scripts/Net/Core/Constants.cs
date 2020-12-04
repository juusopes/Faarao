﻿using System.Collections;
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
    public const int noId = -1;
    public const int maxPlayers = 2;
    public const int defaultPing = 1000;  // ms
    //public const int resendDelay = 30;  // ms
    public const double resendMultiplier = 1.2;
    public const int maxResends = 30;

    public const int port = 26950;
    public const string ip = "127.0.0.1";

    public const float maxSceneLoadProgress = .9f;

    public const int sioUdpConnectionReset = -1744830452;

    public const float messageLifetimeInSeconds = 10;
    public const float messageStartFadeoutAfterSeconds = 5;
    public const float messageFadeoutTimespan = messageLifetimeInSeconds - messageStartFadeoutAfterSeconds;


    public static readonly Color messageColorNetworking = Color.blue;

    public const int masterServerPort = 26915;
    public const int maxServers = 512;
    public const string masterServerDNS = "faaraonkirous.ddns.net";
    public const float masterServerHeartbeatFrequency = 10f;
    public const int masterServerId = masterServerPort;
    public const string apiAddress = "http://faaraonkirous.ddns.net:5000/";
}
