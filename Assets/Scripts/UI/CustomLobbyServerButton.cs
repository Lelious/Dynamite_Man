using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror.Discovery;
using System;

public class CustomLobbyServerButton : MonoBehaviour
{
    public static event Action<ServerResponse> OnServerButtonClicked;
    public Button Button;

    [SerializeField] private TextMeshProUGUI _serverName;

    private ServerResponse _response;

    private void Awake()
    {
        Button.onClick.AddListener(ServerButtonClick);
    }

    public void SetServer(ServerResponse response)
    {
        _response = response;
        _serverName.text = response.GameName;
    }

    public void ServerButtonClick()
    {
        OnServerButtonClicked?.Invoke(_response);
    }
}
