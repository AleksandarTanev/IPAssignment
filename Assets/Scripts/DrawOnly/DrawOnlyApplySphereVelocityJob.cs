using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

[BurstCompile]
public struct DrawOnlyApplySphereVelocityJob : IJobParallelFor
{
    [ReadOnly]
    public float deltaTime;

    public NativeArray<DrawOnlySphereState> states;

    public void Execute(int index)
    {
        var st = states[index];
        st.position += st.velocity * deltaTime;
        states[index] = st;
    }
}
