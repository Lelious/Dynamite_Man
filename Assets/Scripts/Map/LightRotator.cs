using System.Collections.Generic;
using UnityEngine;

public class LightRotator : MonoBehaviour
{
    [SerializeField] private List<Transform> _objectsToRotate = new();
    [SerializeField] private float _rotationSpeed = 1f;

    private void FixedUpdate()
    {
        foreach (var item in _objectsToRotate)
        {
            item.Rotate(Vector3.forward * Time.fixedDeltaTime, _rotationSpeed);
        }
    }
}
