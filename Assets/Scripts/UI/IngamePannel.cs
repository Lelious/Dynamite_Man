using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IngamePannel : MonoBehaviour
{
    [SerializeField] private GameObject _leaderBoard;

    public void ShowLeaderBoard()
    {
        if (!_leaderBoard.activeInHierarchy)
        {
            _leaderBoard.SetActive(true);
        }
    }

    public void HideLeaderBoard()
    {
        if (_leaderBoard.activeInHierarchy)
        {
            _leaderBoard.SetActive(false);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
