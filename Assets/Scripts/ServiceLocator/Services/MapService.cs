using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.IO;
using System;

public class MapService : IDisposable
{
    private Guid _matchId;
    private List<Box> _allBoxList = new List<Box>();
    private Vector2Int _lowerLimit = new Vector2Int(-12, -5);
    private Vector2Int _upperLimit = new Vector2Int(10, 5);
    private Dictionary<Vector2Int, IMapObject> _mapObjects;
    private HashSet<IMapObject> _mapObjectsHash;
    private HashSet<IMapObject> _players;
    private CollectablesService _collectablesService;
    private GameplayService _gameplayService;
    private Box _woodBoxPrefab;
    private string _woodBoxSchemePath;

    public MapService(Guid id, Box woodBoxPrefab, List<Box> concreteCommonBoxList, GameplayService gameplayService)
    {
        _matchId = id;
        _woodBoxPrefab = woodBoxPrefab;
        _woodBoxSchemePath = Path.Combine(Application.streamingAssetsPath, "wood_positions.json");
        _mapObjects = new Dictionary<Vector2Int, IMapObject>();
        _players = new HashSet<IMapObject>();
        _mapObjectsHash = new HashSet<IMapObject>();
        _collectablesService = ServiceLocator<IService>.Instance.Get<CollectablesService>();
        _gameplayService = gameplayService;

        foreach (var item in concreteCommonBoxList)
        {
            _allBoxList.Add(item);
        }
    }

    [ServerCallback]
    public void InitializeService()
    {
        string json = File.ReadAllText(_woodBoxSchemePath);
        PositionList woodlist = JsonUtility.FromJson<PositionList>(json);
        Debug.Log($"Inited wood list, count is {woodlist.objects.Count}");

        if (_woodBoxPrefab == null)
            Debug.Log("Wood prefab is null");

        foreach (var obj in woodlist.objects)
        {
            var woodBox = GameObject.Instantiate(_woodBoxPrefab, new Vector3(obj.PosX, 0.5f, obj.PosZ), Quaternion.identity);
            woodBox.InitializeBox(this, _matchId);
            woodBox.IsDestroyed = true;
            _allBoxList.Add(woodBox);
        }
        ResetMapObjects();
    }

    [Server]
    public void ResetMapObjects()
    {
        _mapObjects.Clear();

        foreach (var item in _allBoxList)
        {
            var pos = new Vector2Int((int)item.transform.position.x, (int)item.transform.position.z);

            _mapObjects.Add(pos, item.GetComponent<IMapObject>());

            if (!item.GetMapObjectType().Equals(MapObjectType.Concrete))
            {
                if (item.IsDestroyed)
                {
                    item.IsDestroyed = false;
                    NetworkServer.Spawn(item.gameObject);
                }
            }
        }
    }

    [Server]
    public void RegisterPlayer(IMapObject player)
    {
        _players.Add(player);
    }

    [Server]
    public void UnregisterPlayer(IMapObject player)
    {
        if (_players.Contains(player))
            _players.Remove(player);
    }

    [ServerCallback]
    public void DestroyGameObjects()
    {
        foreach (var item in _allBoxList)
        {
            if(item.GetMapObjectType().Equals(MapObjectType.Wood))
                NetworkServer.Destroy(item.gameObject);
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

        foreach (var player in _players)
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

    [ServerCallback]
    public void RegisterMapObject(IMapObject mapObject, Vector2Int coords)
    {
        _mapObjects.Add(coords, mapObject);
    }

    [ServerCallback]
    public void UnregisterMapObject(IMapObject obj, Vector2Int coords)
    {
        if(obj.GetMapObjectType().Equals(MapObjectType.Wood))
        {
            _collectablesService.SpawnCollectable(coords, obj.GetMatchGuid(), _gameplayService);
        }
        if (_mapObjects.TryGetValue(coords, out _))
        {
            _mapObjects.Remove(coords);
        }
    }

    [ServerCallback]
    public void DestroyObject(GameObject obj)
    {
        NetworkServer.UnSpawn(obj);
    }

    [Server]
    public void Dispose()
    {
        foreach (var item in _allBoxList)
        {
            if(item.GetMapObjectType().Equals(MapObjectType.Wood))
            {
                NetworkServer.Destroy(item.gameObject);
            }
        }

        _allBoxList.Clear();
        _mapObjects.Clear();
        _mapObjectsHash.Clear();
        _gameplayService = null;
        _players.Clear();
    }
}
public enum CellStatus
{
    Free,
    Occupied,
    NotExist
}
