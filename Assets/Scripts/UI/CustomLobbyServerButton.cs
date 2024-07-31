using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror.Discovery;

public class CustomLobbyServerButton : MonoBehaviour
{
    public Button Button;

    [SerializeField] private TextMeshProUGUI _serverName;
    private ServerResponse _response;
    private CustomLobbyHUD _hud;

    public void SetHUD(CustomLobbyHUD hud)
    {
        _hud = hud;
        Button.onClick.AddListener(ConnectToServer);
    }

    public void SetServer(ServerResponse response)
    {
        _response = response;
        _serverName.text = response.GameName;
    }

    public void ConnectToServer()
    {
        _hud.Connect(_response);
    }
}
