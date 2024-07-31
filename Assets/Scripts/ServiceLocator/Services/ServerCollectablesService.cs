using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ServerCollectablesService : NetworkBehaviour, IService
{
    [SerializeField] private List<Collectable> _collectables = new List<Collectable>();
    [SerializeField] private float _chance;

    private List<Collectable> _spawnedCollectables;

    [ServerCallback]
    private void Start()
    {
        ServiceLocator<IService>.Instance.Register(this);
        _spawnedCollectables = new List<Collectable>();
    }

    [ServerCallback]
    public void SpawnCollectable(Vector2Int pos)
    {
        if (Random.Range(0f, 100f) <= _chance)
        {
            var collectable = Instantiate(GetRandomCollectable(), new Vector3(pos.x, 0f, pos.y), Quaternion.identity);
            _spawnedCollectables.Add(collectable);
            NetworkServer.Spawn(collectable.gameObject, connectionToServer);
        }
    }

    [ServerCallback]
    public void UnspawnCollectable(Collectable collectable)
    {
        _spawnedCollectables.Remove(collectable);
        NetworkServer.Destroy(collectable.gameObject);
    }

    public void UnspawnAllCollectables()
    {
        foreach (var item in _spawnedCollectables)
        {
            NetworkServer.Destroy(item.gameObject);
        }
        _spawnedCollectables.Clear();
    }

    private Collectable GetRandomCollectable()
    {
        return _collectables[Random.Range(0, _collectables.Count)];
    }
}
