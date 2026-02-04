using System.Collections.Generic;
using UnityEngine;

public class DebugHitscanGizmo : MonoBehaviour
{
    [System.Serializable]
    private struct RayRecord
    {
        public Vector3 origin;
        public Vector3 direction;
        public float range;
        public float time;
    }

    [Header("Gizmo Settings")]
    [SerializeField] private float gizmoDuration = 1.0f;
    [SerializeField] private float originSphereSize = 0.05f;
    [SerializeField] private int maxRays = 200; // prevents infinite buildup

    private readonly List<RayRecord> rays = new List<RayRecord>();

    public void RecordRay(Vector3 origin, Vector3 direction, float range)
    {
        RayRecord record = new RayRecord
        {
            origin = origin,
            direction = direction.normalized,
            range = range,
            time = Time.time
        };

        rays.Add(record);

        // Optional: cap list size so it doesn't grow forever
        if (rays.Count > maxRays)
            rays.RemoveAt(0);
    }

    private void OnDrawGizmos()
    {
        float now = Time.time;

        // Remove expired rays
        for (int i = rays.Count - 1; i >= 0; i--)
        {
            if (now - rays[i].time > gizmoDuration)
                rays.RemoveAt(i);
        }

        // Draw remaining rays
        Gizmos.color = Color.red;

        foreach (var ray in rays)
        {
            Gizmos.DrawLine(ray.origin, ray.origin + ray.direction * ray.range);
            Gizmos.DrawSphere(ray.origin, originSphereSize);
        }
    }
}
