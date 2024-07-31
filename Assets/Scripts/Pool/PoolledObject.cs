using System;
using UnityEngine;

[Serializable]
public class PoolledObject
{
    public GameObject Prefab;
    public PoolObjectType Type;
    public int Count;
}
