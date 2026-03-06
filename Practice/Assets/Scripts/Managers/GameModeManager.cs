using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;

using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

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

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        // If you're NOT the new master, it means the old host left
        // and Photon migrated master to someone else.

        Debug.Log("Master client switched. Ending room.");

        SceneTransitionManager.ReturnToLobby();
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

            PhotonHashtable props = new PhotonHashtable();
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
        if (!PhotonNetwork.IsMasterClient)
            return;

        Debug.Log($"Player {actorNumber} wins the match!");


        StartCoroutine(RestartMatchRoutine());
    }

    private IEnumerator RestartMatchRoutine()
    {
        // 1️⃣ End the match
        PhotonHashtable endProps = new PhotonHashtable();
        endProps["GameStarted"] = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(endProps);
        DamageSystem.Instance.SetRound(false);

        Debug.Log("Match ended. Restarting in 5 seconds...");

        // 2️⃣ Wait (win screen time)
        yield return new WaitForSeconds(5f);

        // 3️⃣ Reset all scores
        ResetAllScores();
        ResetWeapons();
        Debug.Log("Scores reset.");

        // 4️⃣ Start the match again
        DamageSystem.Instance.SetRound(false);

        // start next round countdown
        StartRoundCountdown(6);
        // 5️⃣ Respawn everyone
        SpawnPlayers spawnPlayers = SpawnPlayers.Instance;
        foreach (var player in FindObjectsOfType<PlayerHealth>())
        {
            Vector3 spawn = spawnPlayers.GetRandomSpawn();

            player.photonView.RPC(
                "RPC_Respawn",
                player.photonView.Owner,
                spawn
            );
        }

        Debug.Log("New match started.");
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

        PhotonHashtable props = new PhotonHashtable();
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

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, PhotonHashtable changedProps)
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

    public void Spawn()
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

        SpawnPlayers spawnPlayers = FindObjectOfType<SpawnPlayers>();
        spawnPlayers.photonView.RPC("SpawnPlayerRPC", RpcTarget.All);

        StartRoundCountdown(6);
    }
    IEnumerator StartMatchRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        StartGame();
    }

    void StartRoundCountdown(double duration)
    {
        PhotonHashtable props = new PhotonHashtable();
        props["MatchStartTime"] = PhotonNetwork.Time + duration;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        StartCoroutine(StartMatchRoutine((float)duration));
    }

    public void StartGame()
    {

        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (InGame()) return;
        PhotonHashtable props = new PhotonHashtable();
        props["GameStarted"] = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
        DamageSystem.Instance.SetRound(true);
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

    private void ResetAllScores()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            scoreByActor[player.ActorNumber] = 0;

            PhotonHashtable props = new PhotonHashtable();
            props["Score"] = 0;
            player.SetCustomProperties(props);
        }
    }

    void ResetWeapons()
    {
        foreach (PlayerCombatController player in FindObjectsOfType<PlayerCombatController>())
        {
            player.photonView.RPC(
                    "RPC_ResetWeapon",
                    player.photonView.Owner
                );
        }
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