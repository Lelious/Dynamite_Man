using Mirror;
using UnityEngine;

public sealed class BombService : NetworkBehaviour, IService
{
    [SerializeField] private Bomb _bomb;
    [SerializeField] private float _bombResetTime = 2f;

    private MapService _mapService;

    [Command]
    public void CmdPlaceBomb(Player player)
    {
        if (_mapService == null)
        {
            _mapService = ServiceLocator<IService>.Instance.Get<MapService>();
        }

        if (player.GetBombCount() > 0)
        {
            Vector2Int pos = new Vector2Int(Mathf.RoundToInt(player.transform.position.x), Mathf.RoundToInt(player.transform.position.z));

            if (_mapService.CheckFree(pos))
            {
                player.ReduseBombCount();
                Bomb bomb = GetOrCreate(player);
                bomb.transform.position = new Vector3(pos.x, 0.5f, pos.y);
                _mapService.RegisterMapObject(bomb, pos);
                bomb.InitializeBomb(pos, player);
                NetworkServer.Spawn(bomb.gameObject, connectionToClient);
            }
        }
    }

    [ServerCallback]
    public void Unregister(Bomb bomb)
    {
        _mapService.UnregisterMapObject(new Vector2Int((int)bomb.transform.position.x, (int)bomb.transform.position.z));
    }

    [ServerCallback]
    public void ReturnToServicePool(Bomb bomb)
    {
        NetworkServer.Destroy(bomb.gameObject);
    }

    private Bomb GetOrCreate(Player player)
    {
        var newBomb = Instantiate(_bomb);
        newBomb.SetPlayer(player);
        newBomb.SetBombService(this);
        return newBomb;
    }
}
