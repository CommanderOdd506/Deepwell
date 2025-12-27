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


    private void Start()
    {
        if (!photonView.IsMine)
            return;
        playerCombatController = GetComponent<PlayerCombatController>();
        Initialize();

        // EVERYONE requests registration
        photonView.RPC(
            "RPC_RequestRegistration",
            RpcTarget.MasterClient
        );
    }

    [PunRPC]
    void RPC_RequestRegistration(PhotonMessageInfo info)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        int actorNumber = info.Sender.ActorNumber;

        DamageSystem.Instance.RegisterPlayer(
            actorNumber,
            this,
            playerCombatController.GetCurrentWeapon()
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
        if (deathProcessed) return;

        deathProcessed = true;
        isAlive = false;

        // Disable input, weapons, etc.
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
