using Mirror;
using UnityEngine;

public class ClientLogger : MonoBehaviour
{
    private void Start()
    {
        if (Application.isBatchMode) return;
            NetworkManager.singleton.StartClient();
    }
}
