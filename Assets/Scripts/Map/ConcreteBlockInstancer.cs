using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConcreteBlockInstancer : MonoBehaviour
{
    [SerializeField] private GameObject _concreteBoxPrefab;
    [SerializeField] private List<Vector2Int> _concretePositions;

    private void Start()
    {
        if (Application.isBatchMode) return;

        foreach (var obj in _concretePositions)
        {
            var block = Instantiate(_concreteBoxPrefab, new Vector3(obj.x, 0.5f, obj.y), Quaternion.identity, transform);
        }
    }
}
