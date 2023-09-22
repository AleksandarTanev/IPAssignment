using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct ApplySphereVelocityJob : IJobParallelForTransform
{
    [ReadOnly]
    public float deltaTime;

    [ReadOnly]
    public NativeArray<Vector3> velocities;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += velocities[index] * deltaTime;
    }
}
