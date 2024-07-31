using Mirror;
using UnityEngine;

public class Collectable : NetworkBehaviour
{
    [SerializeField] private StatType _statType;
    [SerializeField] private float _amount;

    private ServerPlayersService _playersService;
    private StatImprovement _statImprovement;

    [ServerCallback]
    private void Start()
    {
        _statImprovement = new StatImprovement(_statType, _amount);
        _playersService = ServiceLocator<IService>.Instance.Get<ServerPlayersService>();
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            _playersService.ImprovePlayerStats(player, ref _statImprovement);
            ServiceLocator<IService>.Instance.Get<ServerCollectablesService>().UnspawnCollectable(this);
        }
    }
}


