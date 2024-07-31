using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBombVisualService : MonoBehaviour, IService
{
    [SerializeField] private List<Image> _bombImages = new List<Image>();
    [SerializeField] private float _maxReplenishTime = 2f;
    [SerializeField] private List<VisualBomb> _bombs = new List<VisualBomb>();
    private int _currentBombs = 0;

    private void Awake()
    {
        ServiceLocator<IService>.Instance.Register(this);
        InitializeBombs();
    }

    private void InitializeBombs()
    {
        for (int i = 0; i < 3; i++)
        {
            _bombs.Add(new VisualBomb { Image = _bombImages[i], TimeToRestore = 0f });
        }
    }

    public void ValidateBombs(int count)
    {
        if (_currentBombs != count)
        {
            _currentBombs = count;
        }

        foreach (var item in _bombImages)
        {
            item.enabled = false;
        }

        for (int i = 0; i < count; i++)
        {
            _bombImages[i].enabled = true;
        }
    }

    public void StartResettingBomb(float time)
    {
        for (int i = _currentBombs - 1; i >= 0; i--)
        {
            if (_bombs[i].TimeToRestore <= 0f)
            {
                _bombs[i].TimeToRestore = time;
                return;
            }
        }
    }

    public void SetBombReplenishTime(float time)
    {
        _maxReplenishTime = time;
    }

    private void FixedUpdate()
    {
        foreach (var item in _bombs)
        {
            if (item.TimeToRestore > 0f)
            {
                item.TimeToRestore -= Time.fixedDeltaTime;
                item.Image.fillAmount = 1f - (item.TimeToRestore / _maxReplenishTime);
            }
        }
    }
}

[System.Serializable]
public class VisualBomb
{
    public Image Image;
    public float TimeToRestore;
}
