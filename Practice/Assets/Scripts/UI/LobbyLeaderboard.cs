using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class LobbyLeaderboard : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform contentParent; // Where names get spawned
    [SerializeField] private GameObject playerEntryPrefab; // TMP text prefab

    private List<GameObject> spawnedEntries = new List<GameObject>();

    void Start()
    {
        RefreshLobbyList();
    }

    void RefreshLobbyList()
    {
        // Clear old entries
        foreach (GameObject obj in spawnedEntries)
        {
            Destroy(obj);
        }

        spawnedEntries.Clear();

        // Add each current player
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject entry = Instantiate(playerEntryPrefab, contentParent);

            TMP_Text text = entry.GetComponent<TMP_Text>();
            text.text = player.NickName;

            spawnedEntries.Add(entry);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshLobbyList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshLobbyList();
    }
}