using UnityEngine;

public class Wheel : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelMesh;

    [Header("Fix for mesh orientation")]
    public Vector3 rotationOffset = Vector3.zero; // tweak in inspector if needed

    void LateUpdate()
    {
        if (wheelCollider == null || wheelMesh == null)
            return;

        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);

        wheelMesh.position = pos;
        wheelMesh.rotation = rot * Quaternion.Euler(rotationOffset);
    }
}
