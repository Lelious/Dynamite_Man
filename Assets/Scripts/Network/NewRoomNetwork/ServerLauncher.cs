using Mirror;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class ServerLauncher : MonoBehaviour
{
    public NetworkManager networkManager;
    public bool IsLobby = false;
    [SerializeField] private kcp2k.KcpTransport _transport;


    private void Start()
    {
        if(Application.isBatchMode)
        {
            string[] args = System.Environment.GetCommandLineArgs();

            bool isGame = false;

            foreach (var arg in args)
            {
                if (arg == "-lobby") IsLobby = true;
                if (arg == "-game") isGame = true;
            }

            if (IsLobby)
            {
                _transport.port = 7777;
                networkManager.onlineScene = "LobbyScene";
                networkManager.StartServer();

                string gameServerBatPath = Path.Combine(Application.dataPath, "../game.bat");
                ProcessStartInfo startInfo = new ProcessStartInfo();

                startInfo.FileName = gameServerBatPath;
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = true;

                Process.Start(startInfo);
            }

            if (isGame)
            {
                _transport.port = 7778;
                networkManager.onlineScene = "MainScene";
                networkManager.StartServer();
            }
        }       
    }
}
