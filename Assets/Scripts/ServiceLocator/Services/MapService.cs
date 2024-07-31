using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

public sealed class MapService : NetworkBehaviour, IService
{
    [SerializeField] private List<Box> _allBoxList = new List<Box>();
    [SerializeField] private Vector2Int _lowerLimit, _upperLimit;

    private Dictionary<Vector2Int, IMapObject> _mapObjects;
    private HashSet<IMapObject> _mapObjectsHash;
    private HashSet<IMapObject> _players;
    private ServerPlayersService _playersService;

    [ServerCallback]
    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        ServiceLocator<IService>.Instance.Register(this);
        _mapObjects = new Dictionary<Vector2Int, IMapObject>();
        _players = new HashSet<IMapObject>();
        _mapObjectsHash = new HashSet<IMapObject>();
        _playersService = ServiceLocator<IService>.Instance.Get<ServerPlayersService>();

        InitAllBoxes();
    }

    [ServerCallback]
    public void InitAllBoxes()
    {
        _mapObjects.Clear();

        foreach (var item in _allBoxList)
        {
            _mapObjects.Add(new Vector2Int((int)item.transform.position.x, (int)item.transform.position.z), item.GetComponent<IMapObject>());
        }
    }

    [ServerCallback]
    public HashSet<IMapObject> GetObjects(Vector2Int coords)
    {
        _mapObjectsHash.Clear();

        if (_mapObjects.TryGetValue(coords, out var mapObject))
        {
            _mapObjectsHash.Add(mapObject);
        }

        foreach (var player in _playersService.GetMapPlayers())
        {
            if (player.GetRoundedCoords() == coords)
            {
                _mapObjectsHash.Add(player);
            }
        }
        return _mapObjectsHash;
    }

    [ServerCallback]
    public bool CheckFree(Vector2Int coords)
    {
        if (_mapObjects.TryGetValue(coords, out var mapObject))
        {
            if (mapObject.GetMapObjectType() == MapObjectType.Player)
            {
                return true;
            }
            else return false;
        }
        else
        {
            return true;
        }
    }

    [ServerCallback]
    public bool GetExistStatus(Vector2Int coords)
    {
        if (coords.x < _lowerLimit.x || coords.x > _upperLimit.x || coords.y < _lowerLimit.y || coords.y > _upperLimit.y)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public Vector2Int GetLowerLimit() => _lowerLimit;
    public Vector2Int GetUpperLimit() => _upperLimit;

    [ServerCallback]
    public void RegisterMapObject(IMapObject mapObject, Vector2Int coords)
    {
        _mapObjects.Add(coords, mapObject);
    }

    [ServerCallback]
    public void UnregisterMapObject(Vector2Int coords)
    {
        if (_mapObjects.TryGetValue(coords, out _))
        {
            _mapObjects.Remove(coords);
        }
    }

}
public enum CellStatus
{
    Free,
    Occupied,
    NotExist
}
