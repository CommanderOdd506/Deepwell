using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerHealth : MonoBehaviourPun
{
    public int currentHealth;
    public int maxHealth;
    public TextMeshProUGUI healthText;
    public RawImage bloodPanel;
    private PlayerCombatController playerCombatController;
    private MouseLook mouseLook;
    private PlayerMovement playerMovement;
    private PlayerInput playerInput;
    private bool isAlive;
    bool deathProcessed;
    int actorNumber;
    public SkinnedMeshRenderer skin;
    
    public float respawnTimer = 3;
    public float bloodStartFadeDelay = 2f;
    public float bloodFadeSpeed = 10;

    private float _bloodTimer;
    private float currentAlpha = 0f;
    private bool isFading = false;


    [Header("Player Reference")]
    public GameObject ragdollPrefab;
    public GameObject deathPanel;

    private void Start()
    {
        if (!photonView.IsMine)
            return;

        playerCombatController = GetComponent<PlayerCombatController>();
        playerMovement = GetComponent<PlayerMovement>();
        playerInput = GetComponent<PlayerInput>();
        mouseLook = GetComponent<MouseLook>();
        Initialize();
        bloodPanel.color = new Vector4(1,0,0,0);
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

    void Update()
    {
        if (!photonView.IsMine || bloodPanel == null)
            return;

        if (!isFading)
            return;

        _bloodTimer += Time.deltaTime;

        if (_bloodTimer > bloodStartFadeDelay)
        {
            currentAlpha = Mathf.Lerp(currentAlpha, 0f, bloodFadeSpeed * Time.deltaTime);

            Color c = bloodPanel.color;
            c.a = currentAlpha;
            bloodPanel.color = c;

            if (currentAlpha <= 0.01f)
            {
                currentAlpha = 0f;
                isFading = false;
                c.a = currentAlpha;
                bloodPanel.color = c;
            }
        }
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

        if (newHealth < currentHealth)
        {
            _bloodTimer = 0;
            currentAlpha = 1f;
            isFading = true;

            Color c = bloodPanel.color;
            c.a = 1f;
            bloodPanel.color = c;
        }

        currentHealth = newHealth;
        UpdateUI();
    }

    [PunRPC]
    void RPC_Respawn(Vector3 spawnPoint)
    {
        if (!photonView.IsMine)
        {
            skin.enabled = true;
        }
        transform.position = spawnPoint;
        playerCombatController.SetLife(true);
        deathPanel.SetActive(false);
        playerInput.ToggleInput(true);
        playerMovement.ToggleMovement(true);
        mouseLook.ToggleMovement(true);
        currentHealth = maxHealth;
        isAlive = true;
        deathProcessed = false;


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
        isAlive = false;

        SpawnRagdoll();
        
        if (photonView.IsMine)
        {
            playerCombatController.SetLife(false);
            if (deathPanel) deathPanel.SetActive(true);
            if (playerInput) playerInput.ToggleInput(false);
            if (playerMovement) playerMovement.ToggleMovement(false);
            if (mouseLook) mouseLook.ToggleMovement(false);
            
            Collider[] colliders = GetComponentsInChildren<Collider>(true);
            foreach (Collider col in colliders)
                col.enabled = false;

            transform.position += Vector3.down * 20f;
        }
        else
        {
            skin.enabled = false;
        }
    }

    void SpawnRagdoll()
    {
        if (ragdollPrefab == null)
        {
            Debug.LogWarning("[PlayerHealth] No ragdollPrefab assigned.");
            return;
        }

        Instantiate(ragdollPrefab, transform.position, transform.rotation);
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
