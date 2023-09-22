using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Unity.Mathematics;
using static Unity.Collections.AllocatorManager;
using System;

public class SpheresManager : MonoBehaviour
{
    public Mesh mesh;
    public Material material;

    [Space]
    [Range(1, 10000)]
    [SerializeField] private int _numOfSphereOnClick;
    [SerializeField] private Bounds _volumeBounds;
    [SerializeField] private float _speed;
    [SerializeField] private float _secondsSphereToBeRed;

    [Space]
    [SerializeField] private GameObject spherePrefab;
    private float sphereToSphereColRange;

    public int SpheresCount => spheres.Count;

    public List<GameObject> spheres = new List<GameObject>();

    private TransformAccessArray allTransforms;

    private List<SphereState> sphereStates = new List<SphereState>();
    private NativeArray<float> spheresRedColorTime;

    private KDTree tree;

    public Bounds PlaygroundBounds => _volumeBounds;

    private MaterialPropertyBlock blockWhiteColor;
    private MaterialPropertyBlock blockRedColor;

    private void Start()
    {
        sphereToSphereColRange = spherePrefab.transform.localScale.x;

        blockWhiteColor = new MaterialPropertyBlock();
        blockRedColor = new MaterialPropertyBlock();

        blockWhiteColor.SetColor("_Color", Color.white);
        blockRedColor.SetColor("_Color", Color.red);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CreateNewSpheres();
            RebuildTransformsArray();

            CreateKDTree();
        }

        if (spheres.Count > 0)
        {
            RefreshTreePositions();
            CheckSphereCollisions();

            ContainSpheresInPlayground();
            ApplyVelocities();

            /*
            for (int i = 0; i < allTransforms.length; i++)
            {
                var tr = allTransforms[i];

                Graphics.DrawMesh(mesh, tr.position, Quaternion.identity, material, 0, Camera.main, 0, blockRedColor);
            }*/
        }
    }

    private void CreateNewSpheres()
    {
        var newSpheres = InstantiateNewSpheres(_numOfSphereOnClick);

        NativeArray<SphereState> states = new NativeArray<SphereState>(newSpheres.Count, Allocator.TempJob);

        IniateStateForSpheresJob job = new IniateStateForSpheresJob()
        {
            randomSeed = (uint)DateTime.UtcNow.Second,
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
            newSpheres[i].gameObject.name = i.ToString();
            newSphereStates.Add(states[i]);
        }

        states.Dispose();

        spheres.AddRange(newSpheres);
        sphereStates.AddRange(newSphereStates);

        if (spheresRedColorTime.IsCreated)
        {
            spheresRedColorTime.Dispose();
        }
        spheresRedColorTime = new NativeArray<float>(spheres.Count, Allocator.Persistent);
    }

    private void RebuildTransformsArray()
    {
        if (allTransforms.isCreated)
        {
            allTransforms.Dispose();
        }

        allTransforms = new TransformAccessArray(spheres.Count);
        for (int i = 0; i < spheres.Count; i++)
        {
            allTransforms.Add(spheres[i].transform);
        }
    }

    private void CreateKDTree()
    {
        if (tree.IsCreated)
        {
            tree.Dispose();
        }

        tree = new KDTree(spheres.Count, Allocator.Persistent, KDTree.DefaultKDTreeParams);
    }

    private void RefreshTreePositions()
    {
        NativeArray<float3> nativePositions = new NativeArray<float3>(spheres.Count, Allocator.TempJob);
        for (int i = 0; i < spheres.Count; i++)
        {
            nativePositions[i] = spheres[i].transform.position;
        }

        var jobHandle = tree.BuildTree(nativePositions);

        jobHandle.Complete();

        nativePositions.Dispose();
    }

    private void CheckSphereCollisions()
    {
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(spheres.Count, Allocator.TempJob);
        for (int i = 0; i < spheres.Count; i++)
        {
            velocities[i] = sphereStates[i].velocity;
        }

        NativeArray<float3> nativePositions = new NativeArray<float3>(spheres.Count, Allocator.TempJob);
        for (int i = 0; i < spheres.Count; i++)
        {
            nativePositions[i] = spheres[i].transform.position;
        }

        SphereCollisionJob job = new SphereCollisionJob()
        {
            secondsSphereToBeRed = _secondsSphereToBeRed,
            speed = _speed,
            velocities = velocities,
            positions = nativePositions,
            spheresRedColorTime = spheresRedColorTime,
            tree = tree,
            range = sphereToSphereColRange
        };

        JobHandle jobHandle = job.Schedule(nativePositions.Length, 32);
        jobHandle.Complete();

        for (int i = 0; i < spheres.Count; i++)
        {
            var mr = spheres[i].GetComponent<MeshRenderer>();

            if (spheresRedColorTime[i] > 0)
            {
                mr.SetPropertyBlock(blockRedColor);
                spheresRedColorTime[i] -= Time.deltaTime;

                var state = sphereStates[i];
                state.velocity = velocities[i];
                sphereStates[i] = state;
            }
            else
            {
                mr.SetPropertyBlock(blockWhiteColor);
            }
        }

        velocities.Dispose();
        nativePositions.Dispose();
    }

    private void ApplyVelocities()
    {
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(spheres.Count, Allocator.TempJob);

        for (int i = 0; i < spheres.Count; i++)
        {
            velocities[i] = sphereStates[i].velocity;
        }

        ApplySphereVelocityJob job = new ApplySphereVelocityJob()
        {
            velocities = velocities,
            deltaTime = Time.deltaTime,
        };

        JobHandle jobHandle = job.Schedule(allTransforms);
        jobHandle.Complete();

        velocities.Dispose();
    }

    private void ContainSpheresInPlayground()
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

        ContainSpheresInPlaygroundJob job = new ContainSpheresInPlaygroundJob()
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

        if (tree.IsCreated)
        {
            tree.Dispose();
        }

        if (spheresRedColorTime.IsCreated)
        {
            spheresRedColorTime.Dispose();
        }
    }
}