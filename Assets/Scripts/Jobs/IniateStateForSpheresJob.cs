using Unity.Jobs;
using Unity.Collections;
using UnityEngine;

public struct IniateStateForSpheresJob : IJobParallelFor
{
    public float speed;
    public Bounds boundsToSpawnIn;

    public NativeArray<SphereState> states;

    public Unity.Mathematics.Random random;

    public void Execute(int index)
    {
        var newState = new SphereState();

        newState.velocity = GetRandomDirection() * speed;
        newState.startingPosition = GetRandomPositionInBounds(boundsToSpawnIn);

        states[index] = newState;
    }

    private Vector3 GetRandomDirection()
    {
        return (new Vector3(random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f), random.NextFloat(-1f, 1f))).normalized;
    }

    private Vector3 GetRandomPositionInBounds(Bounds bounds)
    {
        float x = random.NextFloat(-bounds.size.x / 2, bounds.size.x / 2);
        float y = random.NextFloat(-bounds.size.y / 2, bounds.size.y / 2);
        float z = random.NextFloat(-bounds.size.z / 2, bounds.size.z / 2);

        return new Vector3(x, y, z) + bounds.center;
    }
}
