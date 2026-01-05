using UnityEngine;
using Photon.Pun;
using System.Collections;

public class VisibilityControl : MonoBehaviourPun
{
    [Header("Mesh Groups")]
    public Renderer[] bodyRenderers;       // World model
    public Renderer[] viewModelRenderers;  // Arms + gun
    public GameObject[] localObjects;
    public GameObject[] remoteObjects;

    AudioListener audioListener;

    void Awake()
    {
        audioListener = GetComponentInChildren<AudioListener>();
    }

    void OnEnable()
    {

        StartCoroutine(ApplyVisibilityNextFrame());
    }

    IEnumerator ApplyVisibilityNextFrame()
    {
        yield return null; // wait 1 frame for Photon ownership

        if (photonView.IsMine)
            SetLocalPlayerVisibility();
        else
            SetRemotePlayerVisibility();
    }

    void SetLocalPlayerVisibility()
    {
        // Hide body, show viewmodel
        foreach (var r in bodyRenderers)
            r.enabled = false;

        
        foreach (var r in viewModelRenderers)
            r.enabled = true;

        foreach (GameObject obj in localObjects)
            obj.SetActive(true);

        foreach (GameObject obj in remoteObjects)
            obj.SetActive(false);

        if (audioListener)
            audioListener.enabled = true;
    }

    void SetRemotePlayerVisibility()
    {
        // Show body, hide viewmodel
        foreach (var r in bodyRenderers)
            r.enabled = true;

        foreach (var r in viewModelRenderers)
            r.enabled = false;

        foreach (GameObject obj in localObjects)
            obj.SetActive(false);

        foreach (GameObject obj in remoteObjects)
            obj.SetActive(true);

        if (audioListener)
            audioListener.enabled = false;
    }
}