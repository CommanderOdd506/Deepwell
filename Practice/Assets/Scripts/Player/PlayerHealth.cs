using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerHealth : MonoBehaviourPun
{
    public int currentHealth;
    public int maxHealth;
    public TextMeshProUGUI healthText;
    private PlayerCombatController playerCombatController;
    private bool isAlive;
    bool deathProcessed;
    int actorNumber;
    public GameObject ragdollPrefab;


    private void Start()
    {
        if (!photonView.IsMine)
            return;

        playerCombatController = GetComponent<PlayerCombatController>();
        Initialize();

        int weaponId = playerCombatController.GetCurrentWeapon()?.weaponId ?? -1;

        photonView.RPC(
            "RPC_RequestRegistration",
            RpcTarget.MasterClient,
            weaponId
        );
    }

    [PunRPC]
    void RPC_RequestRegistration(int weaponId, PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int actorNumber = info.Sender.ActorNumber;

        if (weaponId == -1)
        {
            Debug.LogError($"[PlayerHealth] Invalid weaponId for Actor {actorNumber}");
            return;
        }

        WeaponData weapon = DamageSystem.Instance.FindWeaponById(weaponId);
        if (weapon == null)
        {
            Debug.LogError($"[PlayerHealth] No weapon found for weaponId {weaponId} for Actor {actorNumber}");
            return;
        }

        DamageSystem.Instance.RegisterPlayer(
            actorNumber,
            this,
            weapon
        );
    }


    void Initialize()
    {
        currentHealth = maxHealth;
        isAlive = true;
        deathProcessed = false;

        if (photonView.IsMine && healthText == null)
            Debug.LogWarning("[PlayerHealth] healthText is NOT assigned on local player!");
        UpdateUI();
    }

    [PunRPC]
    void RPC_SetHealth(int newHealth, PhotonMessageInfo info)
    {
        Debug.Log(
            $"[RPC_SetHealth] GO={gameObject.name} " +
            $"NewHealth={newHealth} " +
            $"IsMine={photonView.IsMine} " +
            $"Sender={info.Sender.ActorNumber}"
        );

        currentHealth = newHealth;
        UpdateUI();
    }

    [PunRPC]
    void RPC_Respawn(Vector3 spawnPoint)
    {
        transform.position = spawnPoint;

        currentHealth = maxHealth;
        isAlive = true;
        deathProcessed = false;

        // ADDED: Re-enable colliders on respawn
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        UpdateUI();
    }

    public void SetHealthUI(TextMeshProUGUI ui)
    {
        healthText = ui;
        UpdateUI();
    }

    void CheckDeath()
    {
        if (!isAlive || deathProcessed)
            return;

    }

    [PunRPC]
    void RPC_OnDeath()
    {
        if (deathProcessed) return;  // This should work, but let's add more safety

        deathProcessed = true;
        isAlive = false;

        // Disable colliders
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        // CHANGED: Only spawn ragdoll once (remove the nested RPC call issue)
        if (ragdollPrefab != null)
        {
            Instantiate(ragdollPrefab, transform.position, transform.rotation);
        }
    }


    private void UpdateUI()
    {
        Debug.Log(
            $"[UpdateUI] GO={gameObject.name} " +
            $"IsMine={photonView.IsMine} " +
            $"HealthText={(healthText != null)} " +
            $"Health={currentHealth}"
        );

        if (!photonView.IsMine) return;

        healthText.text = currentHealth.ToString();
    }
}
