using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class CollectablesService : NetworkBehaviour, IService
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
    public void SpawnCollectable(Vector2Int pos, Guid matchId, GameplayService gameplayService)
    {
        if (UnityEngine.Random.Range(0f, 100f) <= _chance)
        {
            var collectable = Instantiate(GetRandomCollectable(), new Vector3(pos.x, 0f, pos.y), Quaternion.identity);
            collectable.InitializeCollectable(matchId, gameplayService);
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

    [Server]
    public void UnspawnAllCollectables(Guid matchId)
    {
        var matchCollectables = new List<Collectable>();

        foreach (var item in _spawnedCollectables)
        {
            if(item.GetMatchId().Equals(matchId))
            {
                matchCollectables.Add(item);
            }
        }

        for (int i = 0; i < matchCollectables.Count; i++)
        {
            _spawnedCollectables.Remove(matchCollectables[i]);
            NetworkServer.Destroy(matchCollectables[i].gameObject);
        }
    }

    [Server]
    private Collectable GetRandomCollectable()
    {
        return _collectables[UnityEngine.Random.Range(0, _collectables.Count)];
    }
}
