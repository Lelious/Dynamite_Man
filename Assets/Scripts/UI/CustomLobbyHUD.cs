using Mirror;
using Mirror.Discovery;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomLobbyHUD : MonoBehaviour
{
    [SerializeField] private GameObject _landingPanel;
    [SerializeField] private List<CustomLobbyServerButton> _buttonToConnect = new List<CustomLobbyServerButton>();
    [SerializeField] private PlayerNameInput _inputPlayerName;
    [SerializeField] private GameNameInput _inputGameName;

    private Guid _lastClickedRoomGuid;

    private void Awake()
    {
        CustomLobbyServerButton.OnRoomButtonClicked += SetRoomToJoin;
    }

    public void CreateGame()
    {
        NetworkRoomPlayer player =
        NetworkClient.localPlayer.GetComponent<NetworkRoomPlayer>();
        var name = _inputPlayerName.GetName();
        player.DisplayName = name;
        player.CmdCreateRoom(_inputGameName.GetName());
    }

    public void Refresh()
    {
        NetworkClient.localPlayer.GetComponent<NetworkRoomPlayer>().UpdateRoomList();
    }

    public void UpdateHUDRoomsList(List<LobbyRoom> rooms)
    {
        foreach (var item in _buttonToConnect)
        {
            item.gameObject.SetActive(false);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            _buttonToConnect[i].gameObject.SetActive(true);
            _buttonToConnect[i].SetServer(rooms[i].id, rooms[i].name);
        }
    }

    public void Connect()
    {
        NetworkClient.localPlayer.GetComponent<NetworkRoomPlayer>().CmdJoinRoom(_lastClickedRoomGuid);
    }

    public void Quit()
    {
        Application.Quit();
    }

    private void SetRoomToJoin(Guid id)
    {
        _lastClickedRoomGuid = id;
    }
}
