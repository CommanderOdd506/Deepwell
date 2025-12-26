using UnityEngine;
using Photon.Pun;

public class VisibilityControl : MonoBehaviourPun
{
    [Header("Mesh Groups")]
    public Renderer[] bodyRenderers;       // World model
    public Renderer[] viewModelRenderers;  // Arms + gun

    AudioListener audioListener;

    void Awake()
    {
        audioListener = GetComponentInChildren<AudioListener>();
    }

    void Start()
    {
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

        if (audioListener)
            audioListener.enabled = false;
    }
}