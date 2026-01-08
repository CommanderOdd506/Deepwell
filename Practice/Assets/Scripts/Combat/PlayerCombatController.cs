using Photon.Pun;
using UnityEngine;
using System.Collections;
using TMPro;

public enum ArmType
{
    RifleArms,
    PistolArms,
}

[System.Serializable]
public class WeaponReference
{
    public WeaponData weaponData;
    public GameObject objectReference;
    public GameObject worldReference;
}

public class PlayerCombatController : MonoBehaviourPun
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private WeaponData startingWeapon;

    private WeaponData currentWeapon;
    private ArmType currentArms;

    [SerializeField] private PlayerRecoil playerRecoil;
    [SerializeField] private AnimationController animationController;
    [SerializeField] WeaponReference[] weaponDatabase;
    int currentWeaponIndex = 0;

    [Header("Arm Types")]
    public GameObject rifleArms;
    public GameObject pistolArms;

    [Header("UI")]
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI maxAmmoText;
    public GameObject viewModelUI;
    public GameObject scope;
    public Camera cam;
    public float zoomFOV;
    private float startingFOV;
    public GameObject crosshair;

    public float dropDistance = 80f;

    bool lastFireInput;
    float nextFireTime;
    float magAmmo;
    float maxMagAmmo;
    bool isReloading;
    bool hasScope;
    bool isScoped;

    Coroutine reloadRoutine;
    Coroutine reloadMotionRoutine;
    Vector3 viewModelStartPos;

    void Start()
    {
        
        if (!photonView.IsMine)
            return;
        startingFOV = cam.fieldOfView;
        EquipWeapon(startingWeapon);

        viewModelStartPos = viewModelUI.transform.localPosition;
    }
    void Update()
    {
        if (!photonView.IsMine)
            return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f && !isReloading && !isScoped)
        {
            CycleWeapon(scroll > 0f ? 1 : -1);
        }

        bool fireInput = playerInput.firePressed;
        bool reloadInput = playerInput.reloadPressed;
        bool firePressedCheck = fireInput && !lastFireInput;
        bool aimInput = playerInput.aimPressed;

        if (aimInput && hasScope && !isScoped && !isReloading)
        {
            ActivateScope();
        }
        else if (!aimInput && isScoped) 
        {
            DeactivateScope();
        }

        if (reloadInput && !isReloading && magAmmo < maxMagAmmo && !isScoped)
        {
            StartCoroutine(ReloadEvent());
        }
        HandleFireInput(fireInput, firePressedCheck);

        lastFireInput = fireInput;
    }

    void HandleFireInput(bool held, bool pressed)
    {
        if (currentWeapon == null || isReloading || magAmmo <= 0)
            return;
        if (Time.time < nextFireTime)
            return;

        switch (currentWeapon.fireMode)
        {
            case FireMode.Auto:
                if (held)
                    TryFire();
                break;

            case FireMode.Semi:
                if (pressed)
                    TryFire();
                break;
        }
    }

    void CycleWeapon(int direction)
    {
        if (weaponDatabase.Length == 0)
            return;

        currentWeaponIndex += direction;

        if (currentWeaponIndex >= weaponDatabase.Length)
            currentWeaponIndex = 0;
        else if (currentWeaponIndex < 0)
            currentWeaponIndex = weaponDatabase.Length - 1;

        EquipWeapon(weaponDatabase[currentWeaponIndex].weaponData);
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        if (!photonView.IsMine || newWeapon == currentWeapon || newWeapon == null)
        {
            Debug.Log("Equip rejected");
            return;
        }
        currentWeapon = newWeapon;  
        maxMagAmmo = newWeapon.magSize;
        magAmmo = maxMagAmmo;
        hasScope = newWeapon.isScoped;

        if (!hasScope)
        {
            DeactivateScope();
        }

        lastFireInput = false;
        ArmType newType = newWeapon.armType;
        if (newType != null)
        {
            pistolArms.SetActive(false);
            rifleArms.SetActive(false);
            currentArms = newType;
            switch (newType)
            {
                case ArmType.RifleArms:
                    rifleArms.SetActive(true);
                    animationController.SetWeaponState(1);
                    break;
                case ArmType.PistolArms:
                    pistolArms.SetActive(true);
                    animationController.SetWeaponState(2);
                    break;
                default:
                    rifleArms.SetActive(true);
                    break;
            }
        }

        int newWeaponId = newWeapon.weaponId;
        foreach (WeaponReference weapon in weaponDatabase)
        {
            if (weapon.weaponData.weaponId == newWeaponId)
            {
                currentWeapon = weapon.weaponData;
                weapon.objectReference.SetActive(true);
                weapon.worldReference.SetActive(true);
                continue;
            }
            else
            {
                weapon.objectReference.SetActive(false);
                weapon.worldReference.SetActive(false);
                continue;
            }
        }
        photonView.RPC("RPC_SyncWeaponVisuals", RpcTarget.AllBuffered, newWeaponId);

        if (DamageSystem.Instance != null && PhotonNetwork.InRoom)
        {
            DamageSystem.Instance.photonView.RPC(
            "RPC_UpdateWeapon",
            RpcTarget.MasterClient,
            photonView.OwnerActorNr,
            currentWeapon.weaponId
            );
        }
        UpdateUI();
    }
    private void UpdateUI()
    {
        ammoText.text = magAmmo.ToString();
        maxAmmoText.text = maxMagAmmo.ToString();
    }
    void TryFire()
    {
        if (!photonView.IsMine)
            return;

        Debug.Log("Fire!");

        float fireDelay;

        if (currentWeapon.fireMode == FireMode.Semi)
        {
            fireDelay = 0.05f; // minimal debounce
        }
        else
        {
            fireDelay = 60f / Mathf.Max(currentWeapon.rpm, 1f);
        }

        nextFireTime = Time.time + fireDelay;
        magAmmo--;
        UpdateUI();
        playerRecoil.ApplyRecoil(currentWeapon.recoilData);

        DamageSystem.Instance.photonView.RPC(
            "RPC_RequestFire",
            RpcTarget.MasterClient,
            playerCamera.transform.position,
            playerCamera.transform.forward
        );
    }

    void ActivateScope()
    {
        if (!photonView.IsMine)
            return;

        if (!isScoped)
        {
            isScoped = true;
            cam.fieldOfView = 50;
            scope.SetActive(true);
            crosshair.SetActive(false);
            viewModelUI.SetActive(false);
        }
    }

    void DeactivateScope()
    {
        if (!photonView.IsMine)
            return;

        if (isScoped)
        {
            isScoped = false;
            cam.fieldOfView = startingFOV;
            scope.SetActive(false);
            crosshair.SetActive(true);
            viewModelUI.SetActive(true);
        }
    }

    private IEnumerator ReloadEvent()
    {
        if (currentWeapon == null)
            yield break;

        isReloading = true;

        // Start viewmodel motion
        if (reloadMotionRoutine != null)
            StopCoroutine(reloadMotionRoutine);

        reloadMotionRoutine = StartCoroutine(ReloadViewModelMotion(currentWeapon.reloadTime));

        float reloadDuration = Mathf.Max(currentWeapon.reloadTime, 0.05f);
        yield return new WaitForSeconds(reloadDuration);

        magAmmo = maxMagAmmo;
        isReloading = false;
        UpdateUI();
    }

    public WeaponData GetCurrentWeapon()
    {
        return currentWeapon;
    }

    IEnumerator ReloadViewModelMotion(float reloadTime)
    {
        RectTransform rt = viewModelUI.GetComponent<RectTransform>();

        float dropTime = reloadTime * 0.25f;
        float riseTime = reloadTime * 0.25f;

        Vector3 downPos = viewModelStartPos + Vector3.down * dropDistance;

        // Drop
        float t = 0f;
        while (t < dropTime)
        {
            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / dropTime);
            rt.localPosition = Vector3.Lerp(viewModelStartPos, downPos, eased);
            yield return null;
        }

        rt.localPosition = downPos;

        // Hold down for the middle of reload
        yield return new WaitForSeconds(reloadTime - (dropTime + riseTime));

        // Rise
        t = 0f;
        while (t < riseTime)
        {
            t += Time.deltaTime;
            float eased = Mathf.SmoothStep(0f, 1f, t / riseTime);
            rt.localPosition = Vector3.Lerp(downPos, viewModelStartPos, eased);
            yield return null;
        }

        rt.localPosition = viewModelStartPos;
    }
    [PunRPC]
    void RPC_SyncWeaponVisuals(int weaponId)
    {
        foreach (WeaponReference weapon in weaponDatabase)
        {
            bool isEquipped = (weapon.weaponData.weaponId == weaponId);

            if (photonView.IsMine)
            {
                // You see your viewmodel
                weapon.objectReference.SetActive(isEquipped);
                weapon.worldReference.SetActive(false);
            }
            else
            {
                // Others see your world model
                weapon.objectReference.SetActive(false);
                weapon.worldReference.SetActive(isEquipped);
            }
        }
    }
}
