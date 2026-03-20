using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointsService : NetworkBehaviour, IService
{
    [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();

    [ServerCallback]
    private void Start()
    {
        ServiceLocator<IService>.Instance.Register(this);
    }

    [ServerCallback]
    public Transform GetPoint(int index) => _spawnPoints[index];
}
