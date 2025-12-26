using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DamageSystem : MonoBehaviourPunCallbacks
{
    public static DamageSystem Instance { get; private set; }
    private Vector3 spawn;
    // ActorNumber -> PlayerHealth
    private Dictionary<int, PlayerHealth> playerHealthMap =
        new Dictionary<int, PlayerHealth>();
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        spawn = gameObject.transform.position;
    }
    // Called by spawn system or player on spawn
    public void RegisterPlayer(PlayerHealth health)
    {
        int actorNumber = health.photonView.Owner.ActorNumber;
        playerHealthMap[actorNumber] = health;
    }

    public void ProcessHitscan(
    int shooterActor,
    Vector3 origin,
    Vector3 direction,
    int damage,
    float range
)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        DebugHitscanGizmo gizmo = GetComponent<DebugHitscanGizmo>();
        if (gizmo != null)
        {
            gizmo.RecordRay(origin, direction, range);

        }
        if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
        {
            PlayerHealth target = hit.collider.GetComponent<PlayerHealth>();
            if (target == null)
                return;
            ApplyDamage(target.photonView.Owner.ActorNumber, damage);
        }
    }

    public void ApplyDamage(int targetActorNumber, int damage)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!playerHealthMap.TryGetValue(targetActorNumber, out PlayerHealth targetHealth))
            return;

        int newHealth = Mathf.Max(0, targetHealth.currentHealth - damage);

        targetHealth.photonView.RPC(
            "RPC_SetHealth",
            targetHealth.photonView.Owner,
            newHealth
        );

        if (newHealth <= 0)
        {
            NotifyDeath(targetActorNumber);
        }
    }
    [ContextMenu("Test Damage First Player")]
    void TestDamage()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        foreach (var kvp in playerHealthMap)
        {
            ApplyDamage(kvp.Key, 25);
            break;
        }
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        playerHealthMap.Remove(otherPlayer.ActorNumber);
    }

    private Vector3 GetSpawnPoint()
    {
        return spawn;
    }

    private void NotifyDeath(int deadActorNumber)
    {
        Debug.Log($"Player {deadActorNumber} died.");
        if (!playerHealthMap.TryGetValue(deadActorNumber, out PlayerHealth health))
            return;
        // Example (later):
        // GameModeManager.Instance.OnPlayerDeath(deadActorNumber);
        Vector3 spawnPoint = GetSpawnPoint();

        health.photonView.RPC(
            "RPC_Respawn",
            health.photonView.Owner,
            spawnPoint
        );
    }
    [PunRPC]
    void RPC_RequestFire(
    Vector3 origin,
    Vector3 direction,
    PhotonMessageInfo info
)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        Debug.Log("Master caught a fire event");

        int damage = 25;
        float range = 100f;

        ProcessHitscan(
            info.Sender.ActorNumber,
            origin,
            direction,
            damage,
            range
        );
    }

}
