using UnityEngine;
using Mirror;

public class WoodBox : Box, IDamagable
{
    [SerializeField] private GameObject _cube;
    [SerializeField] private BoxCollider _collider;

    public void TakeDamage(Vector3 pos)
    {
        UnregisterBox();
        NetworkServer.UnSpawn(gameObject);
    }
}
