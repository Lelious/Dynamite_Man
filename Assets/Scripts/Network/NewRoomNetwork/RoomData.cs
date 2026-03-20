using System;

[Serializable]
public class RoomData
{
    public Guid matchId;
    public string roomName;
    public int maxPlayers;

    public RoomData() { }

    public RoomData(Guid guid, string roomName, int maxPlayers)
    {
        matchId = guid;
        this.roomName = roomName;
        this.maxPlayers = maxPlayers;
    }
}
