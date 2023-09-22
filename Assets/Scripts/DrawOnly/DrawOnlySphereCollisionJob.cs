using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct DrawOnlySphereCollisionJob : IJobParallelFor
{
    public float secondsSphereToBeRed;

    public float speed;
    public KDTree tree;
    public float range;

    [ReadOnly]
    public NativeArray<float3> positions;

    public NativeArray<DrawOnlySphereState> states;

    // Detecting and reacting to only one collision, the rest are ignored
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
                collisionSphereIndex = neighbours[i].index;
                break;
            }
        }

        if (collisionSphereIndex != -1)
        {
            var state = states[index];

            state.timeLeftToBeRed = secondsSphereToBeRed;
            state.velocity = RecalculateVelocityAfterCollusion(positions[index], positions[collisionSphereIndex]);

            states[index] = state;
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
