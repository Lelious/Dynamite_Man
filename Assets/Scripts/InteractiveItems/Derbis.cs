using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Derbis : MonoBehaviour
{
    [SerializeField] private List<Rigidbody> _derbisList = new List<Rigidbody>();
    [SerializeField] private float _explosionForce;

    private void OnEnable()
    {
        ExplodeDerbis();
    }

    private void ExplodeDerbis()
    {
        foreach (var item in _derbisList)
        {
            item.AddExplosionForce(_explosionForce, transform.position, 5f);
        }

        StartCoroutine(FadingRoutine());
    }

    private IEnumerator FadingRoutine()
    {
        yield return new WaitForSeconds(3f);
        float timer = 1f;
        while (timer > 0)
        {
            timer -= 0.05f;

            foreach (var derbis in _derbisList)
            {
                derbis.transform.localScale = Vector3.one * Mathf.Clamp(timer, 0f, 1f);
            }

            yield return new WaitForSeconds(0.05f);
        }

    }
}
