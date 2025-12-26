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
    private bool isAlive;
    bool deathProcessed;
    int actorNumber;
    private PhotonView photonView;
    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        actorNumber = photonView.Owner.ActorNumber;
        Initialize();
        // Only register on the master client
        if (PhotonNetwork.IsMasterClient)
        {
            RegisterWithDamageSystem();
        }
        else
        {
            // Non-master clients request registration
            photonView.RPC(
                "RPC_RequestRegistration",
                RpcTarget.MasterClient
            );
        }
    }

    void RegisterWithDamageSystem()
    {
        DamageSystem damageSystem = FindObjectOfType<DamageSystem>();
        if (damageSystem != null)
        {
            damageSystem.RegisterPlayer(this);

            Debug.Log($"[DamageSystem] Registered PlayerHealth for Actor {actorNumber}");
        }
    }

    [PunRPC]
    void RPC_RequestRegistration()
    {
        RegisterWithDamageSystem();
    }

    void Initialize()
    {
        currentHealth = maxHealth;
        isAlive = true;
        deathProcessed = false;
    }

    [PunRPC]
    void RPC_SetHealth(int newHealth)
    {
        currentHealth = newHealth;
        UpdateUI();
        CheckDeath();
    }

    [PunRPC]
    void RPC_Respawn(Vector3 spawnPoint)
    {
        gameObject.transform.position = spawnPoint;
    }

    void CheckDeath()
    {
        if (!isAlive || deathProcessed)
            return;

    }
    [PunRPC]
    void RPC_OnDeath()
    {

        isAlive = false;
        //DisableInput();
        // Disable input, combat, etc.
        // Notify GameModeManager via event later
    }

    private void UpdateUI()
    {
        healthText.text = currentHealth.ToString();
    }
}
