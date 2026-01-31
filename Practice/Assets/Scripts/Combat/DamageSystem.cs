using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class DamageSystem : MonoBehaviourPunCallbacks
{
    public static DamageSystem Instance { get; private set; }
    private Vector3 spawn;
    private SpawnPlayers spawnPlayers;
    // ActorNumber -> PlayerHealth
    private Dictionary<int, PlayerHealth> playerHealthMap =
        new Dictionary<int, PlayerHealth>();
    private Dictionary<int, int> healthByActor = new Dictionary<int, int>();
    private Dictionary<int, WeaponData> weaponByActor = new Dictionary<int, WeaponData>();
    private Dictionary<int, bool> isAliveByActor = new Dictionary<int, bool>();
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
        spawnPlayers = GetComponentInParent<SpawnPlayers>();
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
        isAliveByActor[actorNumber] = true;

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

            // ADDED: Prevent self-damage
            if (target.photonView.Owner.ActorNumber == shooterActor)
                return;

            ApplyDamage(target.photonView.Owner.ActorNumber, damage);
            NotifyHitMarker(shooterActor);
        }
    }

    public void NotifyHitMarker(int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!playerHealthMap.TryGetValue(actorNumber, out PlayerHealth targetHealth))
            return;

        PlayerCombatController targetController = targetHealth.GetComponent<PlayerCombatController>();
        if (targetController != null)
        {
            targetController.photonView.RPC("RPC_ShowHitMarker", RpcTarget.All);
        }
    }
    public void ApplyDamage(int targetActorNumber, int damage)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (isAliveByActor.TryGetValue(targetActorNumber, out bool alive) && !alive)
            return;

        if (!playerHealthMap.TryGetValue(targetActorNumber, out PlayerHealth targetHealth) || targetHealth == null)
        {
            CleanupActor(targetActorNumber);
            return;
        }

        if (!healthByActor.TryGetValue(targetActorNumber, out int current))
            current = targetHealth.maxHealth;

        int newHealth = Mathf.Max(0, current - damage);
        healthByActor[targetActorNumber] = newHealth;

        // Send health to everyone so all instances stay consistent (UI still only updates on owner)
        targetHealth.photonView.RPC("RPC_SetHealth", RpcTarget.All, newHealth);

        if (newHealth <= 0)
        {
            isAliveByActor[targetActorNumber] = false;

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

        CleanupActor(otherPlayer.ActorNumber);
    }
    private void CleanupActor(int actorNumber)
    {
        playerHealthMap.Remove(actorNumber);
        healthByActor.Remove(actorNumber);
        weaponByActor.Remove(actorNumber);
        isAliveByActor.Remove(actorNumber);

        Debug.Log($"[DamageSystem] Cleaned up Actor {actorNumber}");
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

       
        StartCoroutine(RespawnDelay(deadActorNumber));
        
    }

    private IEnumerator RespawnDelay(int deadActorNumber)
    {
        yield return new WaitForSeconds(3);
        if (!playerHealthMap.TryGetValue(deadActorNumber, out PlayerHealth health) || health == null)
        {
            CleanupActor(deadActorNumber);
            yield return null;
        }
        healthByActor[deadActorNumber] = health.maxHealth;
        isAliveByActor[deadActorNumber] = true;

        Vector3 spawnPoint = spawnPlayers.GetRandomSpawn();

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
