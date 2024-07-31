using Mirror;
using UnityEngine;

public abstract class Box : NetworkBehaviour, IMapObject
{
    [SerializeField] protected MapObjectType _type;

    public MapObjectType GetMapObjectType() => _type;
    public GameObject GetObject() => gameObject;
    public Vector2Int GetRoundedCoords() => new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.z));

    [ServerCallback]
    public void UnregisterBox()
    {
        ServiceLocator<IService>.Instance.Get<ServerCollectablesService>().SpawnCollectable(GetRoundedCoords());
        ServiceLocator<IService>.Instance.Get<MapService>().UnregisterMapObject(GetRoundedCoords());
    }
}
