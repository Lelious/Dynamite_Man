using Mirror;
using System;
using UnityEngine;

public class Collectable : NetworkBehaviour
{
    [SerializeField] private StatType _statType;
    [SerializeField] private float _amount;
    [SerializeField] private NetworkMatch _match;

    private GameplayService _gameplayService;
    private StatImprovement _statImprovement;

    [Server]
    public void InitializeCollectable(Guid id, GameplayService gameplayService)
    {
        _match.matchId = id;
        _gameplayService = gameplayService;
    }

    [Server]
    public Guid GetMatchId() => _match.matchId;

    [Server]
    private void Start()
    {
        _statImprovement = new StatImprovement(_statType, _amount);
    }

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            _gameplayService.ImprovePlayerStats(player, ref _statImprovement);
            ServiceLocator<IService>.Instance.Get<CollectablesService>().UnspawnCollectable(this);
        }
    }
}


