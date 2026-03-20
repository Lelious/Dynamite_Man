using System;

[Serializable]
public class MapSchemeObject
{
    public string Type;
    public int PosX;
    public int PosZ;

    public MapSchemeObject(int x, int z, MapObjectType type)
    {
        PosX = x;
        PosZ = z;
        Type = type.ToString();
    }
}
