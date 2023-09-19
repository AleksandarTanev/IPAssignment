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
    [SerializeField] private GameObject spherePrefab;

    public int SpheresCount => spheres.Count;

    public List<GameObject> spheres = new List<GameObject>();

    private Unity.Mathematics.Random _random;

    private TransformAccessArray allTransforms;

    private List<SphereState> sphereStates = new List<SphereState>();

    public Bounds PlaygroundBounds => _volumeBounds;

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

        var newSphereStates = new List<SphereState>();
        for (int i = 0; i < states.Length; i++)
        {
            newSpheres[i].transform.position = states[i].startingPosition;

            newSphereStates.Add(states[i]);
        }

        states.Dispose();

        spheres.AddRange(newSpheres);
        sphereStates.AddRange(newSphereStates);
    }

    private void ApplyVelocities()
    {
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(spheres.Count, Allocator.TempJob);

        for (int i = 0; i < spheres.Count; i++)
        {
            velocities[i] = sphereStates[i].velocity;
        }

        SphereVelocityJob job = new SphereVelocityJob()
        {
            velocities = velocities,
            deltaTime = Time.deltaTime,
        };

        JobHandle jobHandle = job.Schedule(allTransforms);
        jobHandle.Complete();

        velocities.Dispose();
    }

    private void AdjustSpheresInBorders()
    {
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(spheres.Count, Allocator.TempJob);

        for (int i = 0; i < spheres.Count; i++)
        {
            velocities[i] = sphereStates[i].velocity;
        }

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
            var st = sphereStates[i];
            st.velocity = velocities[i];
            sphereStates[i] = st;
        }

        velocities.Dispose();
        positions.Dispose();
    }

    private List<GameObject> InstantiateNewSpheres(int num)
    {
        List<GameObject> newSpheres = new List<GameObject>();
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
        if (allTransforms.isCreated)
        {
            allTransforms.Dispose();
        }
    }
}