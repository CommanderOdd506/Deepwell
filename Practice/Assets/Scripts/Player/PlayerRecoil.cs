using UnityEngine;
using Photon.Pun;

public class PlayerRecoil : MonoBehaviourPun
{
    Vector3 originalPos;
    Vector3 currentRecoil;
    Vector3 targetRecoil;

    private Vector2 recoilRotation;
    public Vector2 RecoilRotation => recoilRotation;

    void Start()
    {
        if (!photonView.IsMine) return;
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        UpdateRecoil();
        UpdateCameraRecoil();
    }

    public void ApplyRecoil(RecoilData data)
    {
        targetRecoil += new Vector3(
            Random.Range(-data.kick.x, data.kick.x),
            data.kick.y,
            -data.kickBack
        );

        recoilRotation += new Vector2(
            -data.cameraPunch.x,
            Random.Range(-data.cameraPunch.y, data.cameraPunch.y)
        );
    }

    void UpdateRecoil()
    {
        targetRecoil = Vector3.Lerp(
            targetRecoil,
            Vector3.zero,
            Time.deltaTime * 10f
        );

        currentRecoil = Vector3.Lerp(
            currentRecoil,
            targetRecoil,
            Time.deltaTime * 20f
        );

        transform.localPosition = originalPos + currentRecoil;
    }

    void UpdateCameraRecoil()
    {
        recoilRotation = Vector2.Lerp(
            recoilRotation,
            Vector2.zero,
            Time.deltaTime * 8f  // CHANGED: Higher = faster decay (try 8-15)
        );
    }
}
