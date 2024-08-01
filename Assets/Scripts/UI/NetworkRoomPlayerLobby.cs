using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror.Discovery;

public class NetworkRoomPlayerLobby : NetworkBehaviour
{
    [SerializeField] private GameObject _lobbyUI;
    [SerializeField] private TextMeshProUGUI[] _playerNameTexts = new TextMeshProUGUI[4];
    [SerializeField] private Image[] _playerSlot = new Image[4];
    [SerializeField] private GameObject[] _playerReadyTexts = new GameObject[4];
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Sprite _disabledSlot, _enabledSlot;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    private bool _isLeader;

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerPrefs.GetString("PlayerName"));
        _lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        Room.AddRoomPlayer(this);
        UpdateDisplay();
    }

    public override void OnStopClient()
    {
        Room.RemoveRoomPlayer(this);
        UpdateDisplay();
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayersList[0].connectionToClient != connectionToClient) return;

        Room.StartGame();
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateDisplay();
    public void HandleDisplayNameChanged(string oldValue, string newValue) => UpdateDisplay();

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!_isLeader) return;

        _startGameButton.interactable = readyToStart;  
    }

    private void UpdateDisplay()
    {
        if (!isLocalPlayer)
        {
            foreach (var player in Room.RoomPlayersList)
            {
                if (player.isLocalPlayer)
                {
                    player.UpdateDisplay();
                    break;
                }
            }

            return;
        }

        for (int i = 0; i < _playerNameTexts.Length; i++)
        {
            _playerNameTexts[i].text = "Waiting For Player...";
            _playerReadyTexts[i].SetActive(false);
            _playerSlot[i].sprite = _disabledSlot;
        }

        for (int i = 0; i < Room.RoomPlayersList.Count; i++)
        {
            _playerNameTexts[i].text = Room.RoomPlayersList[i].DisplayName;
            _playerReadyTexts[i].SetActive(Room.RoomPlayersList[i].IsReady);
            _playerSlot[i].sprite = _enabledSlot;
        }
    }

    public bool IsLeader
    {
        set 
        {
            _isLeader = value;
            _startGameButton.gameObject.SetActive(value);
        }
    }

    private CustomNetwork _room;
    private CustomNetwork Room
    {
        get
        {
            if (_room != null) return _room;
            return _room = NetworkManager.singleton as CustomNetwork;
        }
    }
}
