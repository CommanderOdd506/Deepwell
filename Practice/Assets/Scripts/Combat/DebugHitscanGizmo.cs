using UnityEngine;

public class DebugHitscanGizmo : MonoBehaviour
{
    private Vector3 lastOrigin;
    private Vector3 lastDirection;
    private float lastRange;
    private float lastTime;

    private const float gizmoDuration = 1.0f;

    public void RecordRay(Vector3 origin, Vector3 direction, float range)
    {
        lastOrigin = origin;
        lastDirection = direction.normalized;
        lastRange = range;
        lastTime = Time.time;
    }

    void OnDrawGizmos()
    {
        if (Time.time - lastTime > gizmoDuration)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(lastOrigin, lastOrigin + lastDirection * lastRange);
        Gizmos.DrawSphere(lastOrigin, 0.05f);
    }
}
