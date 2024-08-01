using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderBoard : NetworkBehaviour
{
    [SerializeField] private List<LeaderboardCell> _leaderBoardList = new List<LeaderboardCell>();
    private SyncList<LeaderboardCell> _cells = new SyncList<LeaderboardCell>();

    public void RefreshData(List<LeaderboardInfo> info)
    {
        foreach (var item in _cells)
        {
            item.Cell.SetActive(false);
        }

        for (int i = 0; i < info.Count; i++)
        {
            _cells[i].Cell.SetActive(true);
            _cells[i].SetNewData(info[i]);
        }
    }
}

[Serializable]
public class LeaderboardCell
{
    public GameObject Cell;

    [SerializeField] private TextMeshProUGUI _name, _wins, _kills, _deaths;

    public void SetNewData(LeaderboardInfo info)
    {
        _name.text = info.Name;
        _wins.text = info.Wins;
        _kills.text = info.Kills;
        _deaths.text = info.Deaths;
    }
}

public struct LeaderboardInfo
{
    public string Name, Wins, Kills, Deaths;
}
