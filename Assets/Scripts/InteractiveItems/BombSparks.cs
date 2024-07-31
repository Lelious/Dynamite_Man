using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombSparks : MonoBehaviour
{
    [SerializeField] private MeshRenderer _renderer;
    [SerializeField] private float _timeToScaleSpark = 0.3f, _timeToUnscaleSpark = 0.3f;
    [SerializeField] private List<Transform> _sparks = new List<Transform>();

    private Vector3 _centerScale = Vector3.zero;

    public void CreateSparks(ref Explosion info)
    {
        if (_centerScale == Vector3.zero)
        {
            _centerScale = _renderer.transform.localScale;
        }

        _renderer.transform.localScale = Vector3.zero;

        StartCoroutine(SparksRoutine(new List<int> { info.Up, info.Right, info.Down, info.Left }));
    }

    private IEnumerator SparksRoutine(List<int> sparks)
    {
        _renderer.enabled = true;
        float timer = _timeToScaleSpark;
        float scaler;

        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            scaler = 1 - (timer < 0 ? 0 : timer / _timeToScaleSpark);

            _renderer.transform.localScale = _centerScale * scaler;

            yield return new WaitForEndOfFrame();
        }

        timer = _timeToScaleSpark;

        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            scaler = 1 - (timer < 0 ? 0 : timer / _timeToScaleSpark);

            for (int i = 0; i < _sparks.Count; i++)
            {
                _sparks[i].localScale = new Vector3(sparks[i] * scaler, 1f, 1f);
            }

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(0.5f);

        timer = _timeToScaleSpark;

        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            scaler = (timer < 0 ? 0 : timer / _timeToScaleSpark);

            for (int i = 0; i < _sparks.Count; i++)
            {
                _sparks[i].localScale = new Vector3(sparks[i] * scaler, 1f, 1f);
            }

            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i < _sparks.Count; i++)
        {
            _sparks[i].localScale = new Vector3(0f, 1f, 1f);
        }

        timer = _timeToScaleSpark;

        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            scaler = (timer < 0 ? 0 : timer / _timeToScaleSpark);

            _renderer.transform.localScale = _centerScale * scaler;

            yield return new WaitForEndOfFrame();
        }

        _renderer.enabled = false;
    }
}

public struct SparkInfo
{
    public readonly List<int> SparksScaleList;

    public SparkInfo(List<int> scales)
    {
        SparksScaleList = scales;
    }
}
