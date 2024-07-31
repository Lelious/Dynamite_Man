using System;
using System.Collections.Generic;
using UnityEngine;

public class Pool
{
    private List<PoolledObject> _objectsToInitialize;
    private Dictionary<PoolObjectType, Queue<IPoollable>> _pool;
    private Transform _parent;
    public void SetupPool(List<PoolledObject> objects, Transform parent)
    {
        _pool = new Dictionary<PoolObjectType, Queue<IPoollable>>();
        _objectsToInitialize = objects;
        _parent = parent;

        foreach (var item in _objectsToInitialize)
        {
            var queue = new Queue<IPoollable>();

            for (int i = 0; i < item.Count; i++)
            {
                queue.Enqueue(PrecreateObject(item));
            }

            _pool.Add(item.Type, queue);
        }
    }

    public IPoollable GetPoollable(PoolObjectType type)
    {
        if (_pool.TryGetValue(type, out Queue<IPoollable> queue))
        {
            if (queue.Count > 0)
            {
                return queue.Dequeue();
            }
            else
            {
                return PrecreateObject(type);
            }
        }
        else
        {
            return null;
        }
    }

    public void ReturnToPool(IPoollable poollable)
    {
        poollable.SetInnactive();

        if (_pool.TryGetValue(poollable.GetPoollableType(), out Queue<IPoollable> queue))
        {
            queue.Enqueue(poollable);
        }
    }

    private IPoollable PrecreateObject(PoolledObject obj)
    {
        var poollable = GameObject.Instantiate(obj.Prefab, _parent).GetComponent<IPoollable>();
        poollable.SetPool(this);
        poollable.SetInnactive();
        return poollable;
    }

    private IPoollable PrecreateObject(PoolObjectType type)
    {
        var poolObject = _objectsToInitialize.Find(x => x.Type.Equals(type));
        var poollable = GameObject.Instantiate(poolObject.Prefab, _parent).GetComponent<IPoollable>();
        poollable.SetPool(this);
        poollable.SetInnactive();
        return poollable;
    }
}
