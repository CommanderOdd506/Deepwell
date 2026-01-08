using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    private bool isJoining;

    void Start()
    {
        Debug.Log($"[LobbyManager] IsConnected: {PhotonNetwork.IsConnected}");
        Debug.Log($"[LobbyManager] InLobby: {PhotonNetwork.InLobby}");
        Debug.Log($"[LobbyManager] Client State: {PhotonNetwork.NetworkClientState}");

        if (PhotonNetwork.InLobby)
        {
            Debug.Log("[LobbyManager] Already in lobby.");
        }
        else if (PhotonNetwork.IsConnected)
        {
            TryJoinLobby();
        }
        else
        {
            Debug.LogError("[LobbyManager] Not connected to Photon!");
        }
    }

    void Update()
    {
        if (!isJoining && !PhotonNetwork.InLobby && PhotonNetwork.IsConnectedAndReady)
        {
            TryJoinLobby();
        }
    }

    private void TryJoinLobby()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMasterServer && !isJoining)
        {
            Debug.Log("[LobbyManager] Connected to master, joining lobby...");
            isJoining = true;
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[LobbyManager] Connected to master server.");
        TryJoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[LobbyManager] ? Successfully joined lobby!");
        isJoining = false;
        enabled = false;
    }
}
