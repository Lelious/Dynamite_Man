using System;
using System.Collections.Generic;

public class LobbyRoom
{
    public Guid id;

    public string name;

    public NetworkRoomPlayer leader;

    public List<NetworkRoomPlayer> Players = new();

    public int maxPlayers = 4;

    public bool AllReady()
    {
        foreach (var p in Players)
            if (!p.IsReady)
                return false;

        return true;
    }
}


public struct LobbyPlayerInfo
{
    public string Name;
    public bool Ready;

    public LobbyPlayerInfo(string name, bool ready)
    {
        Name = name;
        Ready = ready;
    }
}
