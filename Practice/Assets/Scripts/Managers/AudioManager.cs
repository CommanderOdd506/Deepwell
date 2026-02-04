using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AudioManager : MonoBehaviourPun
{
    public static AudioManager Instance;

    [Header("Weapon Database (same on all clients)")]
    [SerializeField] private List<WeaponData> weapons = new List<WeaponData>();

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfx2DSource;

    [Header("3D SFX Pool")]
    [SerializeField] private AudioSource sfx3DPrefab;
    [SerializeField] private int poolSize = 20;

    private AudioSource[] pool;
    private int poolIndex;

    private Dictionary<int, WeaponData> weaponLookup;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildWeaponLookup();
        BuildPool();
    }

    private void BuildWeaponLookup()
    {
        weaponLookup = new Dictionary<int, WeaponData>();

        foreach (var weapon in weapons)
        {
            if (weapon == null) continue;

            if (!weaponLookup.ContainsKey(weapon.weaponId))
                weaponLookup.Add(weapon.weaponId, weapon);
        }
    }

    private void BuildPool()
    {
        pool = new AudioSource[poolSize];

        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = Instantiate(sfx3DPrefab, transform);
            pool[i].name = $"SFX3D_{i}";
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 1f; // 3D
        }
    }

    // -----------------------------
    // PUBLIC API (call these)
    // -----------------------------

    /// <summary>
    /// Plays a weapon shot sound in 2D locally (instant feedback for shooter).
    /// </summary>
    public void PlayWeaponShot2D_Local(int weaponId, float volume = 1f)
    {
        if (!TryGetWeaponShotClip(weaponId, out AudioClip clip))
            return;

        sfx2DSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Plays a weapon shot sound in 3D for other players (networked).
    /// </summary>
    public void PlayWeaponShot3D_Networked(int weaponId, Vector3 position, float volume = 1f)
    {
        photonView.RPC(nameof(RPC_PlayWeaponShot3D), RpcTarget.Others, weaponId, position, volume);
    }

    // -----------------------------
    // RPCs
    // -----------------------------

    [PunRPC]
    private void RPC_PlayWeaponShot3D(int weaponId, Vector3 position, float volume)
    {
        if (!TryGetWeaponShotClip(weaponId, out AudioClip clip))
            return;

        PlayClip3D_Local(clip, position, volume);
    }

    // -----------------------------
    // INTERNAL LOCAL PLAY METHODS
    // -----------------------------

    private void PlayClip3D_Local(AudioClip clip, Vector3 position, float volume)
    {
        if (clip == null) return;
        if (pool == null || pool.Length == 0) return;

        AudioSource src = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Length;

        src.transform.position = position;
        src.Stop();
        src.clip = clip;
        src.volume = volume;
        src.pitch = Random.Range(0.95f, 1.05f);
        src.Play();
    }

    private bool TryGetWeaponShotClip(int weaponId, out AudioClip clip)
    {
        clip = null;

        if (weaponLookup == null || weaponLookup.Count == 0)
            BuildWeaponLookup();

        if (!weaponLookup.TryGetValue(weaponId, out WeaponData weapon))
            return false;

        clip = weapon.shotClip;
        return clip != null;
    }
}
