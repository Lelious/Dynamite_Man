using Mirror;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;

public class NetworkRoomPlayer : NetworkBehaviour
{
    [SerializeField] private GameObject _lobbyUI;
    [SerializeField] private TextMeshProUGUI[] _playerNameTexts = new TextMeshProUGUI[4];
    [SerializeField] private Image[] _playerSlot = new Image[4];
    [SerializeField] private GameObject[] _playerReadyTexts = new GameObject[4];
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Sprite _disabledSlot, _enabledSlot;
    [SerializeField] private CustomLobbyHUD _hudPrefab;

    [SyncVar]
    public Guid MatchId = Guid.Empty;
    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;
    [SerializeField]
    [SyncVar(hook = nameof(HandleHostStatusChanged))]
    public bool IsLeader = false;

    private List<LobbyPlayerInfo> _roomPlayers = new();
    private CustomLobbyHUD _hud;

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerPrefs.GetString("PlayerName"));
        _hud = Instantiate(_hudPrefab, gameObject.transform);
    }

    [TargetRpc]
    public void TargetOpenLobby(NetworkConnection conn)
    {
        _lobbyUI.SetActive(true);
    }

    [TargetRpc]
    public void TargetCloseLobby(NetworkConnection conn)
    {
        _lobbyUI.SetActive(false);
    }

    [Command]
    public void CmdCreateRoom(string roomName)
    {
        LobbyManager.Instance.CreateRoom(this, roomName);
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;
        LobbyManager.Instance.UpdateRoomCheckState(this);
    }

    [Command]
    public void CmdJoinRoom(Guid roomId)
    {
        LobbyManager.Instance.JoinRoom(this, roomId);
        MatchId = roomId;
    }

    [Command]
    public void CmdLeaveRoom()
    {
        LobbyManager.Instance.LeaveRoom(this);
        MatchId = Guid.Empty;
        IsReady = false;
        IsLeader = false;
    }

    [Command]
    public void UpdateRoomList()
    {
        var rooms = LobbyManager.Instance.Rooms.Values.ToList();

        TargetUpdateRoom(rooms);
    }

    [Command]
    public void CmdStartGame()
    {
        if (!IsLeader)
            return;

        LobbyManager.Instance.StartGame(MatchId);        
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue) => UpdateCheckState();
    public void HandleDisplayNameChanged(string oldValue, string newValue)
    { 

    }

    public void HandleHostStatusChanged(bool oldValue, bool newValue) => UpdateHostStatus(newValue);

    [TargetRpc]
    public void TargetUpdateInRoom(NetworkConnection conn, List<LobbyPlayerInfo> playersInRoom)
    {
        _roomPlayers = playersInRoom;
        Debug.Log($"room players {_roomPlayers.Count}");
        UpdateCheckState();
    }

    [TargetRpc]
    public void TargetUpdateRoom(List<LobbyRoom> rooms)
    {
        _hud.UpdateHUDRoomsList(rooms);
    }

    [Client]
    private void UpdateCheckState()
    {
        bool readyToStart = true;

        for (int i = 0; i < _playerNameTexts.Length; i++)
        {
            _playerNameTexts[i].text = "Waiting For Player...";
            _playerReadyTexts[i].SetActive(false);
            _playerSlot[i].sprite = _disabledSlot;
        }
        for (int i = 0; i < _roomPlayers.Count; i++)
        {
            _playerNameTexts[i].text = _roomPlayers[i].Name;
            _playerReadyTexts[i].SetActive(_roomPlayers[i].Ready);
            _playerSlot[i].sprite = _enabledSlot;

            if(!_roomPlayers[i].Ready)
            {
                readyToStart = false;             
            }
        }

        if(IsLeader)
        {
            _startGameButton.interactable = readyToStart && _roomPlayers.Count > 1;
        }
    }

    private void UpdateHostStatus(bool value)
    {
        IsLeader = value;
        _startGameButton.gameObject.SetActive(value);
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!IsLeader) return;

        _startGameButton.interactable = readyToStart;  
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    private CustomNetwork _customNetwork;
    private CustomNetwork CustomNetwork
    {
        get
        {
            if (_customNetwork != null) return _customNetwork;
            return _customNetwork = NetworkManager.singleton as CustomNetwork;
        }
    }
}
