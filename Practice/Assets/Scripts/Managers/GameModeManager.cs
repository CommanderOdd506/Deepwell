using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public enum GameMode
{
    GunGame,
    FFA
}
public class GameModeManager : MonoBehaviourPunCallbacks
{
    public static GameModeManager Instance {  get; private set; }
    [SerializeField] private GameMode startingMode;
    public static GameMode CurrentMode { get; private set; }
    private Dictionary<int, int> scoreByActor = new Dictionary<int, int>();
    [SerializeField] private WeaponData[] gunGameOrder;

    // Start is called before the first frame update

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

    public void RegisterPlayer(int actorNumber)
    {
        scoreByActor.TryAdd(actorNumber, 0);
        Debug.Log($"[GameModeManager] Player {actorNumber} registered in GameManager");
    }

    public void CleanupActor(int actorNumber)
    {
        scoreByActor.Remove(actorNumber);
    }

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
    void HandleFFA(int killerActor, int victimActor)
    {
        scoreByActor.TryAdd(killerActor, 0);

        scoreByActor[killerActor]++;

        Debug.Log($"[FFA] {killerActor} killed {victimActor}. Score: {scoreByActor[killerActor]}");
    }

    void HandleGunGame(int killerActor, int victimActor)
    {
        scoreByActor.TryAdd(killerActor, 0);

        scoreByActor[killerActor]++;

        PromoteWeapon(killerActor);

        Debug.Log($"[GunGame] {killerActor} advanced to tier {scoreByActor[killerActor]}");
    }

    public void PromoteWeapon(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!scoreByActor.ContainsKey(actorNumber))
            return;

        int tier = scoreByActor[actorNumber];

        // WIN CONDITION CHECK
        if (tier >= gunGameOrder.Length)
        {
            Debug.Log($"[GunGame] Player {actorNumber} wins!");
            return;
        }

        int newWeaponId = gunGameOrder[tier].weaponId;

        // Find the player's PhotonView
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
    // Update is called once per frame
    void Update()
    {
        
    }
}
