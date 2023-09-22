using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct SphereCollisionJob : IJobParallelFor
{
    public float secondsSphereToBeRed;

    public float speed;
    public KDTree tree;
    public float range;

    [ReadOnly]
    public NativeArray<float3> positions;

    public NativeArray<float> spheresRedColorTime;
    public NativeArray<Vector3> velocities;

    public void Execute(int index)
    {
        float3 positionToCheck = positions[index];

        NativeArray<KDTree.Neighbour> neighbours = new NativeArray<KDTree.Neighbour>(100, Allocator.Temp);
        int count = tree.GetEntriesInRange(positionToCheck, range, ref neighbours);

        int collisionSphereIndex = -1;
        for (int i = 0; i < count; i++)
        {
            if (neighbours[i].index != index)
            {
                spheresRedColorTime[index] = secondsSphereToBeRed;
                collisionSphereIndex = neighbours[i].index;
                break;
            }
        }

        if (collisionSphereIndex != -1)
        {
            velocities[index] = RecalculateVelocityAfterCollusion(positions[index], positions[collisionSphereIndex]);
        }

        neighbours.Dispose();
    }

    // Simplified calculation, just changing the direction to be oposite of the collusion point
    // Also here expect that the "sphere" are spheres, that have the same size
    private Vector3 RecalculateVelocityAfterCollusion(Vector3 thisSphPos, Vector3 otherSphPos)
    {
        Vector3 colPoint = Vector3.Lerp(thisSphPos, otherSphPos, 0.5f);

        return (thisSphPos - colPoint).normalized * speed;
    }
}
