using UnityEngine;

public interface IPoollableObject
{
    public void SetActive();
    public void SetInnactive();
    public void ReturnToPool();
    public void SetPosition(Vector3 pos);
    public GameObject GetGameObject();
}
