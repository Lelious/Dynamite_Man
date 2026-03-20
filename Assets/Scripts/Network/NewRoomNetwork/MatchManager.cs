using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager instance;

    private Dictionary<Guid, Match> matches = new();

    private void Awake()
    {
        instance = this;
    }
    public Match CreateMatch(string name)
    {
        Guid matchId = Guid.NewGuid();

        Match match = new Match
        {
            matchId = matchId,
            name = name,
            players = new List<NetworkConnectionToClient>()
        };

        matches.Add(matchId, match);

        return match;
    }
    public Match GetMatch(Guid id)
    {
        matches.TryGetValue(id, out Match match);
        return match;
    }
}

public class Match
{
    public Guid matchId;
    public string name;
    public List<NetworkConnectionToClient> players;
}
