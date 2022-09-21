﻿namespace Townsharp.Infra.Alta.Api
{
    public record class ConnectionInfo(long ServerId, string Address, string LocalAddress, string PodName, int GamePort, int ConsolePort, int LoggingPort, int WebsocketPort, int WebserverPort);
}