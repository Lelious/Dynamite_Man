using Mirror;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BombService : NetworkBehaviour, IService
{
    [SerializeField] private Bomb _bomb;
    [SerializeField] private float _bombExplodeTime = 2f;

    private List<Bomb> _allBombs = new();
    private Queue<Bomb> _bombPool = new();

    [ServerCallback]
    private void Start()
    {
        ServiceLocator<IService>.Instance.Register(this);

        for (int i = 0; i < 100; i++)
        {
            var bomb = GetOrCreate();
            _bombPool.Enqueue(bomb);
        }
    }

    [Server]
    public float GetBombExplodeTime() => _bombExplodeTime;

    [Server]
    public void PlaceBomb(Player player, MapService map, GameplayService gameplayService)
    {
        if (player.GetBombCount() > 0)
        {
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.z));

            if (map.CheckFree(pos))
            {
                player.ReduseBombCount();
                Bomb bomb = null;

                if (_bombPool.TryDequeue(out bomb)) { }
                else
                {
                    bomb = GetOrCreate();
                    _allBombs.Add(bomb);
                }

                bomb.transform.position = new Vector3(pos.x, 0.5f, pos.y);
                map.RegisterMapObject(bomb, pos);
                bomb.SetBombMatch(player.GetMatchGuid());
                bomb.InitializeBomb(pos, player, map, gameplayService);
                NetworkServer.Spawn(bomb.gameObject);
                Debug.Log("SpawnBomb");
            }
        }
    }

    [ServerCallback]
    public void ReturnToServicePool(Bomb bomb)
    {
        NetworkServer.UnSpawn(bomb.gameObject);
        _bombPool.Enqueue(bomb);
    }

    [ServerCallback]
    private Bomb GetOrCreate()
    {
        var newBomb = Instantiate(_bomb);
        newBomb.SetBombService(this);
        _allBombs.Add(newBomb);

        return newBomb;
    }
}
