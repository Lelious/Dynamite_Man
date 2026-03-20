using Mirror;
using System;
using UnityEngine;

public abstract class Box : NetworkBehaviour, IMapObject
{
    [SerializeField] protected MapObjectType _type;
    [SerializeField] protected NetworkMatch _match;

    private MapService _mapService;

    public bool IsDestroyed;
    public MapObjectType GetMapObjectType() => _type;
    public GameObject GetObject() => gameObject;
    public Vector2Int GetRoundedCoords() => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

    [Server]
    public void InitializeBox(MapService mapService, Guid id)
    {
        _match.matchId = id;
        _mapService = mapService;
    }

    [Server]
    public void DestroyBox()
    {
        IsDestroyed = true;
        _mapService.UnregisterMapObject(this, GetRoundedCoords());
        _mapService.DestroyObject(gameObject);
    }

    [Server]
    public Guid GetMatchId() => _match.matchId;

    [Server]
    public Guid GetMatchGuid() => _match.matchId;
}
