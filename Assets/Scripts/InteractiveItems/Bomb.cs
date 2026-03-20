using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Bomb : NetworkBehaviour, IMapObject
{
    [SerializeField] private LayerMask _mask;
    [SerializeField] private float _timeToExplode = 2f;
    [SerializeField] private Transform _bombTransform;
    [SerializeField] private CapsuleCollider _triggercollider, _collisionCollider;
    [SerializeField] private List<Collider> _collisionsList = new List<Collider>();
    [SerializeField] private GameObject _explosion, _derbis, _bombVisual;
    [SerializeField] private BombSparks _sparks;
    [SerializeField] private NetworkMatch _match;

    private List<IMapObject> _damagables = new();
    private Vector2Int _position;
    private Player _player;
    private GameplayService _gameplayService;
    private BombService _bombService;
    private MapService _mapService;
    private float _timer;
    private float _initTime;
    private bool _isInited;
    private int _bombPower;

    [ServerCallback]
    public void InitializeBomb(Vector2Int pos, Player player, MapService map, GameplayService gameplayService)
    {
        _damagables.Clear();
        _player = player;
        _position = pos;
        _timer = _timeToExplode;
        _collisionCollider.enabled = false;
        _initTime = 0.2f;
        _isInited = false;
        _bombPower = player.GetBombPower();
        _mapService = map;
        _gameplayService = gameplayService;

        StartCoroutine(BombRoutine());
    }

    [Server]
    public void SetBombMatch(Guid id) => _match.matchId = id;
    [Server]
    public void SetPlayer(Player player) => _player = player;
    [Server]
    public Player GetPlayer() => _player;
    [Server]
    public float GetExplodeTime() => _timeToExplode;

    private IEnumerator BombRoutine()
    {
        while (_timer > 0)
        {
            yield return null;
            _timer -= Time.deltaTime;
            _initTime -= 0.02f;

            if (_initTime <= 0f && !_isInited)
            {
                _isInited = true;
                _collisionCollider.enabled = true;
            }
        }

        BombExplode();
    }

    [ServerCallback]
    private void BombExplode()
    {
        _damagables.Clear();
        _collisionCollider.enabled = false;
        _mapService.UnregisterMapObject(this, new Vector2Int((int)transform.position.x, (int)transform.position.z));

        var mapObj = _mapService.GetObjects(_position);

        if (mapObj.Count != 0)
        {
            foreach (var obj in mapObj)
            {
                _damagables.Add(obj);          
            }
        }

        int up = CalculateExplosion(Vector2Int.up, _position, _bombPower);
        int right = CalculateExplosion(Vector2Int.right, _position, _bombPower);
        int down = CalculateExplosion(Vector2Int.down, _position, _bombPower);
        int left = CalculateExplosion(Vector2Int.left, _position, _bombPower);

        _gameplayService.ProcessBombDamage(_player, _damagables);
        Explosion explosionScheme = new Explosion(up, right, down, left);
        _player.IncreaceBombCount();

        foreach (var item in _damagables)
        {
            Debug.Log($"Bomb at {_position} hitted target {item.GetObject().name} at {item.GetRoundedCoords()}");
        }

        StartCoroutine(DelayedDespawnRoutine());
        RpcBombExplodeVisual();
        RpcSetExplosionCheme(explosionScheme);
    }

    [ServerCallback]
    private int CalculateExplosion(Vector2Int vector, Vector2Int pos, int power)
    {
        int length = 0;

        for (int i = 1; i <= power; i++)
        {
            var newPosition = vector * i + pos;

            if (_mapService.GetExistStatus(newPosition))
            {                
                var mapObj = _mapService.GetObjects(newPosition);

                if (mapObj.Count == 0)
                {
                    length++;
                }
                else
                {
                    foreach (var obj in mapObj)
                    {
                        switch (obj.GetMapObjectType())
                        {
                            case MapObjectType.Concrete:
                                return length;
                            case MapObjectType.Bomb:
                                length++;
                                break;
                            case MapObjectType.Player:
                                _damagables.Add(obj);
                                length++;
                                break;                           
                            case MapObjectType.Wood:
                                _damagables.Add(obj);
                                return ++length;
                        }
                    }
                }
            }
            else
            {
                return length;
            }
        }
        return length;
    }

    private IEnumerator DelayedDespawnRoutine()
    {
        yield return new WaitForSeconds(3f);
        DespawnBomb();
    }

    [ServerCallback]
    private void DespawnBomb()
    {
        StopAllCoroutines();
        _bombService.ReturnToServicePool(this);
    }

    [ClientRpc]
    private void RpcBombExplodeVisual()
    {
        _bombVisual.SetActive(false);
        _explosion.SetActive(true);
        _derbis.SetActive(true);
    }

    [ClientRpc]
    private void RpcSetExplosionCheme(Explosion scheme)
    {
        _sparks.CreateSparks(ref scheme);
    }

    [ServerCallback]
    public void SetBombService(BombService service) => _bombService = service;

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (!_isInited)
        {           
            if (other.TryGetComponent(out PlayerController controller))
            {
                _collisionsList.Add(other);
                Physics.IgnoreCollision(other, _collisionCollider, true);
            }
        }
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        if (_collisionsList.Count == 0) return;
        foreach (var item in _collisionsList)
        {
            if (item == null)
            {
                _collisionsList.Remove(item);
                return;
            }

            if (Vector3.Distance(new Vector3(item.transform.position.x, 0f, item.transform.position.z), new Vector3(transform.position.x, 0f, transform.position.z)) > _collisionCollider.radius + 0.2f)
            {
                Physics.IgnoreCollision(item, _collisionCollider, false);
                _collisionsList.Remove(item);
                return;
            }
        }
    }

    public Vector2Int GetRoundedCoords() => _position;
    public MapObjectType GetMapObjectType() => MapObjectType.Bomb;
    public GameObject GetObject() => gameObject;

    [Server]
    public Guid GetMatchGuid() => _match.matchId;
}
