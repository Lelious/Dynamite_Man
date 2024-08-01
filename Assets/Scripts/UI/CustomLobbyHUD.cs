using Mirror;
using Mirror.Discovery;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomLobbyHUD : MonoBehaviour
{
    [SerializeField] private CustomNetwork _networkManager;
    [SerializeField] private NetworkDiscovery _networkDiscovery;
    [SerializeField] private GameObject _landingPanel;
    [SerializeField] private List<CustomLobbyServerButton> _buttonToConnect = new List<CustomLobbyServerButton>();
    [SerializeField] private PlayerNameInput _inputPlayerName;
    [SerializeField] private GameNameInput _inputGameName;

    private readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();
    private ServerResponse _server;
    private void Start()
    {
        _networkDiscovery.OnServerFound.AddListener(OnDiscoveredServer);

        foreach (var item in _buttonToConnect)
        {
            item.SetHUD(this);
        }
    }

    public void StartHost()
    {
        discoveredServers.Clear();
        NetworkManager.singleton.StartHost();
        _landingPanel.SetActive(false);
        _networkDiscovery.AdvertiseServer(_inputGameName.GetName());
    }

    public void Refresh()
    {
        discoveredServers.Clear();
        _networkDiscovery.StartDiscovery();
    }

    private void RedrawFindedServers()
    {
        foreach (var item in _buttonToConnect)
        {
            item.gameObject.SetActive(false);
        }

        int iterator = 0;

        foreach (var item in discoveredServers)
        {
            _buttonToConnect[iterator].gameObject.SetActive(true);
            _buttonToConnect[iterator].SetServer(item.Value);
        }
    }

    public void Connect()
    {
        _networkDiscovery.StopDiscovery();
        _landingPanel.SetActive(false);
        NetworkManager.singleton.StartClient(_server.uri);
    }

    public void SetServerToConnect(ServerResponse response) => _server = response;

    public void Quit()
    {
        Application.Quit();
    }

    private void OnDiscoveredServer(ServerResponse info)
    {
        if (discoveredServers.TryAdd(info.serverId, info))
        {
            RedrawFindedServers();
        }
    }
}
