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
using OtherOctree;

public class Playground : PlaygroundBase
{
    public Bounds PlaygroundBounds => _volumeBounds;
    public List<GameObject> Spheres => _spheres;

    [Space]
    [SerializeField] private int _minNumOfSphereOnClick;
    [SerializeField] private int _maxNumOfSphereOnClick;
    [Space]
    [SerializeField] private Bounds _volumeBounds;
    [Space]
    [SerializeField] private float _speed;
    [SerializeField] private float _secondsSphereToBeRed;

    [Space]
    [SerializeField] private GameObject _spherePrefab;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;

    private List<GameObject> _spheres;

    private TransformAccessArray _allTransforms;
    private NativeArray<float> spheresRedColorTime;
    private List<SphereState> _sphereStates;

    private KDTree _tree;

    private MaterialPropertyBlock _blockWhiteColor;
    private MaterialPropertyBlock _blockRedColor;

    private float _collisionRange;

    private void Start()
    {
        _collisionRange = _spherePrefab.transform.localScale.x;

        _blockWhiteColor = new MaterialPropertyBlock();
        _blockRedColor = new MaterialPropertyBlock();

        _blockWhiteColor.SetColor("_Color", Color.white);
        _blockRedColor.SetColor("_Color", Color.red);

        _spheres = new List<GameObject>();
        _sphereStates = new List<SphereState>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CreateNewSpheres();
            RebuildTransformsArray();
            CreateNewColorsArray();
            CreateKDTree();
        }

        if (_spheres.Count > 0)
        {
            RefreshTreePositions();
            CheckSphereCollisions();

            ContainSpheresInPlayground();
            ApplyVelocities();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Clear();
        }
    }

    private void CreateNewSpheres()
    {
        int numOfSphere = UnityEngine.Random.Range(_minNumOfSphereOnClick, _maxNumOfSphereOnClick);
        var newSpheres = InstantiateNewSpheres(numOfSphere);

        NativeArray<SphereState> states = new NativeArray<SphereState>(newSpheres.Count, Allocator.TempJob);

        InitiateStateForSpheresJob job = new InitiateStateForSpheresJob()
        {
            randomSeed = (uint)DateTime.UtcNow.Millisecond,
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

        _spheres.AddRange(newSpheres);
        _sphereStates.AddRange(newSphereStates);
    }

    private void CreateNewColorsArray()
    {
        if (spheresRedColorTime.IsCreated)
        {
            spheresRedColorTime.Dispose();
        }
        spheresRedColorTime = new NativeArray<float>(_spheres.Count, Allocator.Persistent);
    }

    private void RebuildTransformsArray()
    {
        if (_allTransforms.isCreated)
        {
            _allTransforms.Dispose();
        }

        _allTransforms = new TransformAccessArray(_spheres.Count);
        for (int i = 0; i < _spheres.Count; i++)
        {
            _allTransforms.Add(_spheres[i].transform);
        }
    }

    private void CreateKDTree()
    {
        if (_tree.IsCreated)
        {
            _tree.Dispose();
        }

        _tree = new KDTree(_spheres.Count, Allocator.Persistent, KDTree.DefaultKDTreeParams);
    }

    private void RefreshTreePositions()
    {
        NativeArray<float3> nativePositions = new NativeArray<float3>(_spheres.Count, Allocator.TempJob);
        for (int i = 0; i < _spheres.Count; i++)
        {
            nativePositions[i] = _spheres[i].transform.position;
        }

        var jobHandle = _tree.BuildTree(nativePositions);

        jobHandle.Complete();

        nativePositions.Dispose();
    }

    private void CheckSphereCollisions()
    {
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(_spheres.Count, Allocator.TempJob);
        for (int i = 0; i < _spheres.Count; i++)
        {
            velocities[i] = _sphereStates[i].velocity;
        }

        NativeArray<float3> nativePositions = new NativeArray<float3>(_spheres.Count, Allocator.TempJob);
        for (int i = 0; i < _spheres.Count; i++)
        {
            nativePositions[i] = _spheres[i].transform.position;
        }

        SphereCollisionJob job = new SphereCollisionJob()
        {
            secondsSphereToBeRed = _secondsSphereToBeRed,
            speed = _speed,
            velocities = velocities,
            positions = nativePositions,
            spheresRedColorTime = spheresRedColorTime,
            tree = _tree,
            range = _collisionRange
        };

        JobHandle jobHandle = job.Schedule(nativePositions.Length, 32);
        jobHandle.Complete();

        for (int i = 0; i < _spheres.Count; i++)
        {
            var mr = _spheres[i].GetComponent<MeshRenderer>();

            if (spheresRedColorTime[i] > 0)
            {
                mr.SetPropertyBlock(_blockRedColor);
                spheresRedColorTime[i] -= Time.deltaTime;

                var state = _sphereStates[i];
                state.velocity = velocities[i];
                _sphereStates[i] = state;
            }
            else
            {
                mr.SetPropertyBlock(_blockWhiteColor);
            }
        }

        velocities.Dispose();
        nativePositions.Dispose();
    }

    private void ApplyVelocities()
    {
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(_spheres.Count, Allocator.TempJob);

        for (int i = 0; i < _spheres.Count; i++)
        {
            velocities[i] = _sphereStates[i].velocity;
        }

        ApplySphereVelocityJob job = new ApplySphereVelocityJob()
        {
            velocities = velocities,
            deltaTime = Time.deltaTime,
        };

        JobHandle jobHandle = job.Schedule(_allTransforms);
        jobHandle.Complete();

        velocities.Dispose();
    }

    private void ContainSpheresInPlayground()
    {
        NativeArray<Vector3> velocities = new NativeArray<Vector3>(_spheres.Count, Allocator.TempJob);

        for (int i = 0; i < _spheres.Count; i++)
        {
            velocities[i] = _sphereStates[i].velocity;
        }

        NativeArray<Vector3> positions = new NativeArray<Vector3>(_spheres.Count, Allocator.TempJob);

        for (int i = 0; i < _spheres.Count; i++)
        {
            positions[i] = _spheres[i].transform.position;
        }

        ContainSpheresInPlaygroundJob job = new ContainSpheresInPlaygroundJob()
        {
            speed = _speed,
            velocities = velocities,
            positions = positions,
            bounds = _volumeBounds
        };

        JobHandle jobHandle = job.Schedule(_spheres.Count, 32);
        jobHandle.Complete();

        for (int i = 0; i < _spheres.Count(); i++)
        {
            var st = _sphereStates[i];
            st.velocity = velocities[i];
            _sphereStates[i] = st;
        }

        velocities.Dispose();
        positions.Dispose();
    }

    private List<GameObject> InstantiateNewSpheres(int num)
    {
        List<GameObject> newSpheres = new List<GameObject>();
        for (int i = 0; i < num; i++)
        {
            var newSphere = Instantiate(_spherePrefab);
            newSphere.gameObject.SetActive(true);
            newSpheres.Add(newSphere);
        }

        return newSpheres;
    }

    public override int GetSpheresCount() => _spheres.Count;
    public override int GetMinSpheresOnClick() => _minNumOfSphereOnClick;
    public override int GetMaxSpheresOnClick() => _maxNumOfSphereOnClick;

    public void Clear()
    {
        for (int i = 0; i < _spheres.Count; i++)
        {
            Destroy(_spheres[i].gameObject);
        }

        _spheres.Clear();
        _sphereStates.Clear();

        OnDestroy();
    }

    private void OnDestroy()
    {
        if (_allTransforms.isCreated)
        {
            _allTransforms.Dispose();
        }

        if (_tree.IsCreated)
        {
            _tree.Dispose();
        }

        if (spheresRedColorTime.IsCreated)
        {
            spheresRedColorTime.Dispose();
        }
    }

    private void OnDrawGizmos()
    {
        if (showPlaygroundBordersInScene)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(PlaygroundBounds.center, PlaygroundBounds.size);
        }
    }
}