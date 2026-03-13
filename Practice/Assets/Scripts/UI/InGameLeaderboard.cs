using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Linq;

public class InGameLeaderboard : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject playerEntryPrefab;
    [SerializeField] private GameObject leaderboardPanel;

    private List<GameObject> spawnedEntries = new List<GameObject>();

    void Update()
    {
        // Hold TAB to show leaderboard
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            leaderboardPanel.SetActive(true);
            RefreshLeaderboard();
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            leaderboardPanel.SetActive(false);
        }
    }

    void RefreshLeaderboard()
    {
        // Clear old entries
        foreach (GameObject obj in spawnedEntries)
        {
            Destroy(obj);
        }

        spawnedEntries.Clear();

        // Build sorted leaderboard
        var leaderboard = PhotonNetwork.PlayerList
            .Select(p =>
            {
                int score = p.CustomProperties.TryGetValue("Score", out object s)
                    ? (int)s
                    : 0;

                return (p.NickName, score);
            })
            .OrderByDescending(entry => entry.score);

        // Spawn entries
        foreach (var entry in leaderboard)
        {
            GameObject row = Instantiate(playerEntryPrefab, contentParent);

            PlayerEntryUI entryUI = row.GetComponent<PlayerEntryUI>();

            entryUI.nameText.text = entry.NickName;
            entryUI.scoreText.text = entry.score.ToString();

            spawnedEntries.Add(row);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        RefreshLeaderboard();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RefreshLeaderboard();
    }
}
