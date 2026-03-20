using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class CustomLobbyServerButton : MonoBehaviour
{
    public static event Action<Guid> OnRoomButtonClicked;
    public Button Button;

    private Guid _guid;

    [SerializeField] private TextMeshProUGUI _serverName;

    private void Awake()
    {
        Button.onClick.AddListener(ServerButtonClick);
    }

    public void SetServer(Guid id, string name)
    {
        _serverName.text = name;
        _guid = id;
    }

    public void ServerButtonClick()
    {
        OnRoomButtonClicked?.Invoke(_guid);
    }
}
