using Photon.Pun;
using UnityEngine;

public class UpperBodyAim : MonoBehaviourPun, IPunObservable
{
    [Header("References")]
    public Transform spine1;
    public Transform spine2;
    public Transform cameraPivot;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float spine1Influence = 0.3f;
    [Range(0f, 1f)]
    public float spine2Influence = 0.7f;

    [Range(-90f, 90f)]
    public float minPitch = -60f;
    [Range(-90f, 90f)]
    public float maxPitch = 60f;

    private PhotonView pv;
    private float networkPitch;

    private Quaternion spine1OriginalRotation;
    private Quaternion spine2OriginalRotation;
    private bool capturedOriginalRotations = false;

    void Start()
    {
        pv = photonView;
    }

    void Update()
    {
        if (pv.IsMine && cameraPivot != null)
        {
            float currentPitch = cameraPivot.localEulerAngles.x;
            if (currentPitch > 180f)
                currentPitch -= 360f;

            networkPitch = currentPitch;
        }
    }

    void LateUpdate()
    {
        if (spine1 == null || spine2 == null || cameraPivot == null)
            return;

        if (!capturedOriginalRotations)
        {
            spine1OriginalRotation = spine1.localRotation;
            spine2OriginalRotation = spine2.localRotation;
            capturedOriginalRotations = true;
        }

        float pitchToApply = pv.IsMine ? GetLocalPitch() : networkPitch;
        pitchToApply = Mathf.Clamp(pitchToApply, minPitch, maxPitch);

        if (spine1 != null)
        {
            Quaternion additionalRot = Quaternion.Euler(0f, 0f, pitchToApply * spine1Influence);
            spine1.localRotation = spine1OriginalRotation * additionalRot;
        }

        if (spine2 != null)
        {
            Quaternion additionalRot = Quaternion.Euler(0f, 0f, pitchToApply * spine1Influence);
            spine2.localRotation = spine2OriginalRotation * additionalRot;
        }
    }

    float GetLocalPitch()
    {
        float pitch = cameraPivot.localEulerAngles.x;
        if (pitch > 180f)
            pitch -= 360f;
        return pitch;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(GetLocalPitch());
        }
        else
        {
            networkPitch = (float)stream.ReceiveNext();
        }
    }
}
