using UnityEngine;

public interface IPoollable
{
    public PoolObjectType GetPoollableType();
    public void SetPosition(Vector3 pos);
    public void SetPool(Pool pool);
    public void SetInnactive();
    public void ReturnToPool();
    public void SetActive();
}
