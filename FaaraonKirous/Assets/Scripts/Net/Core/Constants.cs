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
    public const int DefaultConnectionId = 0;
    public const int maxPlayers = 50;
    public const int defaultPing = 1000;  // ms
    //public const int resendDelay = 30;  // ms
    public const double resendMultiplier = 1.2;
    public const int maxResends = 30;
}
