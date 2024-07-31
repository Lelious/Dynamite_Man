using UnityEngine;

public interface IMapObject
{
    public Vector2Int GetRoundedCoords();
    public MapObjectType GetMapObjectType();
    public GameObject GetObject();
}

public enum MapObjectType
{
    Player,
    Concrete,
    Wood,
    Bomb
}
