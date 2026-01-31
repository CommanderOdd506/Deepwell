using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AudioManager : MonoBehaviourPun
{
    public static AudioManager Instance;

    [Header("Music")]
    public AudioSource musicSource;

    [Header("SFX Settings")]
    public AudioSource twoDSource;
    public AudioSource sfxPrefab;
    public int poolSize = 20;

    private AudioSource[] pool;
    private int poolIndex;
    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Build pool
        pool = new AudioSource[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            pool[i] = Instantiate(sfxPrefab, transform);
            pool[i].gameObject.name = $"SFX_{i}";
            pool[i].playOnAwake = false;
            pool[i].spatialBlend = 1f; // 3D sound
        }
    }

    public void PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f, float pitchMin = 0.95f, float pitchMax = 1.05f)
    {
        if (!clip) return;

        AudioSource src = pool[poolIndex];
        poolIndex = (poolIndex + 1) % pool.Length;

        src.transform.position = position;
        src.clip = clip;
        src.volume = volume;
        src.pitch = Random.Range(pitchMin, pitchMax);
        src.Stop();
        src.Play();
    }

    public void PlaySFX2D(AudioClip clip, float volume = 1f)
    {
        if (!clip) return;

        twoDSource.PlayOneShot(clip, volume);
    }
}
