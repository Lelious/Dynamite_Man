using System.Collections.Generic;
using UnityEngine;

public class PoolParent : MonoBehaviour
{
    [SerializeField] private Pool _pool;
    [SerializeField] private List<PoolledObject> _objectsToInitialize = new List<PoolledObject>();

    public void Construct(Pool pool)
    {
        _pool = pool;
        _pool.SetupPool(_objectsToInitialize, transform);
    }
}
