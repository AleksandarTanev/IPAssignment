using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CheckSphereCollisionJob : IJobParallelFor
{
    public KDTree tree;
    public float range;
    public NativeArray<float3> positions;
    public NativeArray<bool> spheresToColor;

    public void Execute(int index)
    {
        float3 positionToCheck = positions[index];

        NativeArray<KDTree.Neighbour> neighbours = new NativeArray<KDTree.Neighbour>(100, Allocator.Temp);
        int count = tree.GetEntriesInRange(positionToCheck, range, ref neighbours);

        spheresToColor[index] = false;
        for (int i = 0; i < count; i++)
        {
            if (neighbours[i].index != index)
            {
                spheresToColor[index] = true;
                break;
            }
        }

        neighbours.Dispose();
    }
}
