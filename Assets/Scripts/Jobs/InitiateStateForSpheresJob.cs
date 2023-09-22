using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;

[BurstCompile]
public struct InitiateStateForSpheresJob : IJobParallelFor
{
    public uint randomSeed;
    public float speed;
    public Bounds boundsToSpawnIn;

    public NativeArray<SphereState> states;

    public Unity.Mathematics.Random random;

    public void Execute(int index)
    {
        var newState = new SphereState();

        random = new Unity.Mathematics.Random(randomSeed + (uint)(index * 4) + 1);

        var direction = GetRandomDirection(index);
        newState.velocity = direction * speed;
        newState.startingPosition = GetRandomPositionInBounds(boundsToSpawnIn);

        states[index] = newState;
    }

    private Vector3 GetRandomDirection(int index)
    {
        // Making sure for a more random result, as without this X is almost always between -1 and 0
        for (int i = 0; i < index; i++)
        {
            random.NextFloat(-1f, 1f);
        }

        float x = random.NextFloat(-1f, 1f);
        float y = random.NextFloat(-1f, 1f);
        float z = random.NextFloat(-1f, 1f);

        return (new Vector3(x, y, z)).normalized;
    }

    private Vector3 GetRandomPositionInBounds(Bounds bounds)
    {
        float x = random.NextFloat(-bounds.size.x / 2, bounds.size.x / 2);
        float y = random.NextFloat(-bounds.size.y / 2, bounds.size.y / 2);
        float z = random.NextFloat(-bounds.size.z / 2, bounds.size.z / 2);

        return new Vector3(x, y, z) + bounds.center;
    }
}
