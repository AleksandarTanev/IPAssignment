using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using UnityEditor;

public class SpheresManager : MonoBehaviour
{
    [Range(1, 10000)]
    [SerializeField] private int _numOfSphereOnClick;
    [SerializeField] private Bounds _volumeBounds;
    [SerializeField] private float _speed;

    [Space]
    [SerializeField] private Sphere spherePrefab;

    private List<Sphere> spheres = new List<Sphere>();

    private Unity.Mathematics.Random _random;

    private NativeArray<Vector3> velocities;
    private TransformAccessArray allTransforms;

    private void Start()
    {
        _random = new Unity.Mathematics.Random(1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            OnClick();

            if (allTransforms.isCreated)
            {
                allTransforms.Dispose();
            }
            
            //allTransforms = new TransformAccessArray(spheres.Select(x => x.transform).ToArray());
            allTransforms = new TransformAccessArray(spheres.Count);

            for (int i = 0; i < spheres.Count; i++)
            {
                allTransforms.Add(spheres[i].transform);
            }

            /*velocities = new NativeArray<Vector3>(spheres.Count, Allocator.Persistent);*/
        }

        if (spheres.Count > 0)
        {
            AdjustSpheresInBorders();
            ApplyVelocities();
        }
    }

    private void OnClick()
    {
        var newSpheres = InstantiateNewSpheres(_numOfSphereOnClick);

        NativeArray<SphereState> states = new NativeArray<SphereState>(newSpheres.Count, Allocator.TempJob);

        IniateStateForSpheresJob job = new IniateStateForSpheresJob()
        {
            random = new Unity.Mathematics.Random((uint)Random.Range(1, 100000)),
            states = states,
            speed = _speed,
            boundsToSpawnIn = _volumeBounds,
        };

        JobHandle jobHandle = job.Schedule(newSpheres.Count, 32);
        jobHandle.Complete();

        for (int i = 0; i < states.Length; i++)
        {
            newSpheres[i].state = states[i];
            newSpheres[i].transform.position = newSpheres[i].state.startingPosition;
        }

        states.Dispose();


        spheres.AddRange(newSpheres);
    }

    private void ApplyVelocities()
    {
        //NativeArray<SphereState> states = new NativeArray<SphereState>(spheres.Select(x => x.state).ToArray(), Allocator.TempJob);
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(spheres.Count, Allocator.TempJob);

        for (int i = 0; i < spheres.Count; i++)
        {
            velocities[i] = spheres[i].state.velocity;
        }

        //TransformAccessArray transforms = new TransformAccessArray(spheres.Select(x => x.transform).ToArray(), 32);

        SphereVelocityJob job = new SphereVelocityJob()
        {
            velocities = velocities,
            deltaTime = Time.deltaTime,
        };

        JobHandle jobHandle = job.Schedule(allTransforms);
        jobHandle.Complete();

        velocities.Dispose();
        //transforms.Dispose();
    }

    private void AdjustSpheresInBorders()
    {
        //NativeArray<SphereState> states = new NativeArray<SphereState>(spheres.Select(x => x.state).ToArray(), Allocator.TempJob);
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(spheres.Count, Allocator.TempJob);

        for (int i = 0; i < spheres.Count; i++)
        {
            velocities[i] = spheres[i].state.velocity;
        }

        //NativeArray<Vector3> positions = new NativeArray<Vector3>(spheres.Select(x => x.transform.position).ToArray(), Allocator.TempJob);
        NativeArray<Vector3> positions = new NativeArray<Vector3>(spheres.Count, Allocator.TempJob);

        for (int i = 0; i < spheres.Count; i++)
        {
            positions[i] = spheres[i].transform.position;
        }

        AdjustSpheresInBordersJob job = new AdjustSpheresInBordersJob()
        {
            speed = _speed,
            velocities = velocities,
            positions = positions,
            bounds = _volumeBounds
        };

        JobHandle jobHandle = job.Schedule(spheres.Count, 32);
        jobHandle.Complete();

        for (int i = 0; i < spheres.Count(); i++)
        {
            spheres[i].state.velocity = velocities[i];
        }

        velocities.Dispose();
        positions.Dispose();
    }

    private List<Sphere> InstantiateNewSpheres(int num)
    {
        List<Sphere> newSpheres = new List<Sphere>();
        for (int i = 0; i < num; i++)
        {
            var newSphere = Instantiate(spherePrefab);
            newSphere.gameObject.SetActive(true);
            newSpheres.Add(newSphere);
        }

        return newSpheres;
    }

    private void OnDestroy()
    {
        allTransforms.Dispose();
    }
}

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
/*
public struct SphereVelocityJob : IJobParallelForTransform
{
    [ReadOnly]
    public float deltaTime;

    [ReadOnly]
    public NativeArray<Vector3> velocities;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += velocities[index] * deltaTime;
    }
}*/

public struct SphereVelocityJob : IJobParallelForTransform
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