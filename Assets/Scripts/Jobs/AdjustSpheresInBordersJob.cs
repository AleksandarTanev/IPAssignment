using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public struct AdjustSpheresInBordersJob : IJobParallelFor
{
    [ReadOnly]
    public float speed;
    [ReadOnly]
    public Bounds bounds;

    public NativeArray<Vector3> velocities;

    [ReadOnly]
    public NativeArray<Vector3> positions;

    public void Execute(int index)
    {
        float x = positions[index].x;
        float y = positions[index].y;
        float z = positions[index].z;

        Vector3 originalDirection = velocities[index].normalized;
        Vector3 adjustedDirection = originalDirection;

        // If the sphere crosses the  border and the direction on that Axis is to continue, reverse that Axis direction

        if (x < bounds.center.x - bounds.extents.x && adjustedDirection.x < 0f) // Check if crossed left border and reverse, if needed
        {
            adjustedDirection.x *= -1;
        }
        else if (bounds.center.x + bounds.extents.x < x && adjustedDirection.x > 0f) // Check if crossed right border and reverse, if needed
        {
            adjustedDirection.x *= -1;
        }

        if (y < bounds.center.y - bounds.extents.y && adjustedDirection.y < 0f) // Check if crossed bottom border and reverse, if needed
        {
            adjustedDirection.y *= -1;
        }
        else if (bounds.center.y + bounds.extents.y < y && adjustedDirection.y > 0f) // Check if crossed top border and reverse, if needed
        {
            adjustedDirection.y *= -1;
        }

        if (z < bounds.center.z - bounds.extents.z && adjustedDirection.z < 0f) // Check if crossed back border and reverse, if needed
        {
            adjustedDirection.z *= -1;
        }
        else if (bounds.center.z + bounds.extents.z < z && adjustedDirection.z > 0f) // Check if crossed front border and reverse, if needed
        {
            adjustedDirection.z *= -1;
        }

        if (adjustedDirection != originalDirection)
        {
            velocities[index] = adjustedDirection * speed;
        }
    }
}
