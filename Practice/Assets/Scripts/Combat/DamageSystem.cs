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
    private Dictionary<int, int> healthByActor = new Dictionary<int, int>();
    private Dictionary<int, WeaponData> weaponByActor = new Dictionary<int, WeaponData>();
    [SerializeField] private WeaponData[] weaponDatabase;
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


    public void RegisterPlayer(
    int actorNumber,
    PlayerHealth health,
    WeaponData startingWeapon
)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        playerHealthMap[actorNumber] = health;
        healthByActor[actorNumber] = health.maxHealth;
        weaponByActor[actorNumber] = startingWeapon;

        Debug.Log($"[DamageSystem] Registered Actor {actorNumber}");
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
            PlayerHealth target = hit.collider.GetComponentInParent<PlayerHealth>();
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

        if (!healthByActor.TryGetValue(targetActorNumber, out int current))
            current = targetHealth.maxHealth;

        int newHealth = Mathf.Max(0, current - damage);
        healthByActor[targetActorNumber] = newHealth;

        // Send health to everyone so all instances stay consistent (UI still only updates on owner)
        targetHealth.photonView.RPC("RPC_SetHealth", RpcTarget.All, newHealth);

        if (newHealth <= 0)
        {
            targetHealth.photonView.RPC("RPC_OnDeath", RpcTarget.All);
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
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        playerHealthMap.Remove(otherPlayer.ActorNumber);
        healthByActor.Remove(otherPlayer.ActorNumber);
    }
    [PunRPC]
    public void RPC_UpdateWeapon(int actorNumber, int weaponId)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        WeaponData weapon = FindWeaponById(weaponId);
        if (weapon == null)
        {
            Debug.LogError($"[DamageSystem] Unknown weaponId {weaponId}");
            return;
        }

        weaponByActor[actorNumber] = weapon;

        Debug.Log($"[DamageSystem] Actor {actorNumber} switched to weapon {weapon.weaponId}");
    }
    private Vector3 GetSpawnPoint()
    {
        return spawn;
    }

    public WeaponData FindWeaponById(int id)
    {
        foreach (var w in weaponDatabase)
            if (w.weaponId == id)
                return w;

        return null;
    }
    private void NotifyDeath(int deadActorNumber)
    {
        Debug.Log($"Player {deadActorNumber} died.");

        if (!playerHealthMap.TryGetValue(deadActorNumber, out PlayerHealth health))
            return;

        healthByActor[deadActorNumber] = health.maxHealth;

        Vector3 spawnPoint = GetSpawnPoint(/* optionally pass deadActorNumber */);

        health.photonView.RPC("RPC_Respawn", RpcTarget.All, spawnPoint);
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

        //check for players equipped weapon and apply damage 
        if (!weaponByActor.TryGetValue(info.Sender.ActorNumber, out WeaponData weapon))
        {
            Debug.LogWarning($"[DamageSystem] No weapon for Actor {info.Sender.ActorNumber}");
            return;
        }

        int damage = weapon.damage;
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
