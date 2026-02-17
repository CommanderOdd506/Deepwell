using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Linq;

public enum GameMode
{
    GunGame,
    FFA
}

public class GameModeManager : MonoBehaviourPunCallbacks
{
    public static GameModeManager Instance { get; private set; }

    [SerializeField] private GameMode startingMode;
    public static GameMode CurrentMode { get; private set; }

    private Dictionary<int, int> scoreByActor = new Dictionary<int, int>();
    private Dictionary<int, string> nameByActor = new Dictionary<int, string>();

    [SerializeField] private WeaponData[] gunGameOrder;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentMode = startingMode;
    }

    // ==============================
    // PLAYER REGISTRATION
    // ==============================

    public void RegisterPlayer(int actorNumber)
    {
        scoreByActor.TryAdd(actorNumber, 0);

        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);

        if (player != null)
        {
            nameByActor[actorNumber] = player.NickName;

            Hashtable props = new Hashtable();
            props["Score"] = 0;
            player.SetCustomProperties(props);
        }

        Debug.Log($"[GameModeManager] Player {actorNumber} registered.");
    }

    public void CleanupActor(int actorNumber)
    {
        scoreByActor.Remove(actorNumber);
        nameByActor.Remove(actorNumber);
    }

    // ==============================
    // SCORE HANDLING
    // ==============================

    public void AddScore(int killerActor, int victimActor)
    {
        switch (CurrentMode)
        {
            case GameMode.FFA:
                HandleFFA(killerActor, victimActor);
                break;

            case GameMode.GunGame:
                HandleGunGame(killerActor, victimActor);
                break;
        }
    }

    private void WinRound(int actorNumber)
    {
        Hashtable props = new Hashtable();
        props["GameStarted"] = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        
    }

    void HandleFFA(int killerActor, int victimActor)
    {
        if (killerActor <= -1)
            return;

        scoreByActor.TryAdd(killerActor, 0);
        scoreByActor[killerActor]++;

        SyncScoreToProperties(killerActor);

        Debug.Log($"[FFA] {killerActor} killed {victimActor}. Score: {scoreByActor[killerActor]}");
    }

    void HandleGunGame(int killerActor, int victimActor)
    {
        if (killerActor <= -1)
            return;

        scoreByActor.TryAdd(killerActor, 0);
        scoreByActor[killerActor]++;

        SyncScoreToProperties(killerActor);

        PromoteWeapon(killerActor);

        Debug.Log($"[GunGame] {killerActor} advanced to tier {scoreByActor[killerActor]}");
    }

    private void SyncScoreToProperties(int actorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(actorNumber);
        if (player == null)
            return;

        Hashtable props = new Hashtable();
        props["Score"] = scoreByActor[actorNumber];

        player.SetCustomProperties(props);
    }

    // ==============================
    // LEADERBOARD
    // ==============================

    public (string name, int score)[] GetLeaderboard()
    {
        return PhotonNetwork.PlayerList
            .Select(p =>
            {
                int score = p.CustomProperties.TryGetValue("Score", out object s)
                    ? (int)s
                    : 0;

                return (p.NickName, score);
            })
            .OrderByDescending(entry => entry.score)
            .ToArray();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Score"))
        {
            Debug.Log("Leaderboard updated.");
            // Hook your UI refresh here if needed
        }
    }

    // ==============================
    // GAME STATE
    // ==============================

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameStarted", out object started))
            {
                if ((bool)started)
                {
                    FindObjectOfType<SpawnPlayers>().SpawnPlayerLate();
                }
            }
            return;
        }

        Hashtable props = new Hashtable();
        props["GameStarted"] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        SpawnPlayers spawnPlayers = FindObjectOfType<SpawnPlayers>();
        spawnPlayers.photonView.RPC("SpawnPlayerRPC", RpcTarget.All);
    }

    public bool InGame()
    {
        if (PhotonNetwork.CurrentRoom == null)
            return false;

        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("GameStarted", out object started))
        {
            return (bool)started;
        }

        return false;
    }

    // ==============================
    // GUNGAME PROMOTION
    // ==============================

    public void PromoteWeapon(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!scoreByActor.ContainsKey(actorNumber))
            return;

        int tier = scoreByActor[actorNumber];

        if (tier >= gunGameOrder.Length)
        {
            Debug.Log($"[GunGame] Player {actorNumber} wins!");
            WinRound(actorNumber);
            return;
        }

        int newWeaponId = gunGameOrder[tier].weaponId;

        foreach (PlayerCombatController player in FindObjectsOfType<PlayerCombatController>())
        {
            if (player.photonView.OwnerActorNr == actorNumber)
            {
                player.photonView.RPC(
                    "RPC_PromotePlayer",
                    player.photonView.Owner,
                    newWeaponId
                );
                break;
            }
        }
    }
}