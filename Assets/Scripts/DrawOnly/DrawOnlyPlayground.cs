using Unity.Jobs;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
using Unity.Mathematics;
using System;

public class DrawOnlyPlayground : PlaygroundBase
{
    public Bounds PlaygroundBounds => _volumeBounds;

    [Space]
    [SerializeField] private int _minNumOfSphereOnClick;
    [SerializeField] private int _maxNumOfSphereOnClick;
    [Space]
    [SerializeField] private Bounds _volumeBounds;
    [Space]
    [SerializeField] private float _speed;
    [SerializeField] private float _secondsSphereToBeRed;

    [Space]
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField] private Material _materialRed;
    [SerializeField] private Material _materialWhite;

    private NativeArray<DrawOnlySphereState> _sphereStates;

    private KDTree _tree;

    private MaterialPropertyBlock _blockWhiteColor;
    private MaterialPropertyBlock _blockRedColor;

    private float _collisionRange = 1;

    private Matrix4x4[] _metrices;
    private RenderParams _rpRed;
    private RenderParams _rpWhite;

    private int _numOfRedColoredSpehere;

    private void Start()
    {
        _blockWhiteColor = new MaterialPropertyBlock();
        _blockRedColor = new MaterialPropertyBlock();

        _blockWhiteColor.SetColor("_Color", Color.white);
        _blockRedColor.SetColor("_Color", Color.red);

        _rpRed = new RenderParams(_materialRed);
        _rpWhite = new RenderParams(_materialWhite);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            int numOfSphere = UnityEngine.Random.Range(_minNumOfSphereOnClick, _maxNumOfSphereOnClick);
            CreateNewStates(numOfSphere);
            CreateKDTree();
        }

        if (_sphereStates.IsCreated && _sphereStates.Length > 0)
        {
            RefreshTreePositions();
            CheckSphereCollisions();

            ContainSpheresInPlayground();
            ApplyVelocities();

            DrawSphereMeshes();

            UpdateSpheresColorTimer();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Clear();
        }
    }

    private void CreateNewStates(int amount)
    {
        var newSphereStates = new NativeArray<DrawOnlySphereState>(amount, Allocator.TempJob);

        DrawOnlyInitiateStateForSpheresJob job = new DrawOnlyInitiateStateForSpheresJob()
        {
            randomSeed = (uint)DateTime.UtcNow.Millisecond,
            states = newSphereStates,
            speed = _speed,
            boundsToSpawnIn = _volumeBounds,
        };

        JobHandle jobHandle = job.Schedule(amount, 32);
        jobHandle.Complete();

        // Add or Create sphere new persistant States
        List<DrawOnlySphereState> allStates = new List<DrawOnlySphereState>();
        if (_sphereStates.IsCreated)
        {
            // Saving the states of already created spheres
            for (int i = 0; i < _sphereStates.Length; i++)
            {
                allStates.Add(_sphereStates[i]);
            }

            _sphereStates.Dispose();
        }

        for (int i = 0; i < newSphereStates.Length; i++)
        {
            allStates.Add(newSphereStates[i]);
        }

        _sphereStates = new NativeArray<DrawOnlySphereState>(allStates.Count, Allocator.Persistent);
        for (int i = 0; i < _sphereStates.Length; i++)
        {
            _sphereStates[i] = allStates[i];
        }

        newSphereStates.Dispose();

        _metrices = new Matrix4x4[_sphereStates.Length];
    }

    private void CreateNewSpheresStates()
    {
        int numOfSphere = UnityEngine.Random.Range(_minNumOfSphereOnClick, _maxNumOfSphereOnClick);

        NativeArray<SphereState> states = new NativeArray<SphereState>(numOfSphere, Allocator.TempJob);

        InitiateStateForSpheresJob job = new InitiateStateForSpheresJob()
        {
            randomSeed = (uint)DateTime.UtcNow.Second,
            states = states,
            speed = _speed,
            boundsToSpawnIn = _volumeBounds,
        };

        JobHandle jobHandle = job.Schedule(numOfSphere, 32);
        jobHandle.Complete();

        var newSphereStates = new List<SphereState>();
        for (int i = 0; i < states.Length; i++)
        {
            newSphereStates.Add(states[i]);
        }

        states.Dispose();
    }

    private void CreateKDTree()
    {
        if (_tree.IsCreated)
        {
            _tree.Dispose();
        }

        _tree = new KDTree(_sphereStates.Length, Allocator.Persistent, KDTree.DefaultKDTreeParams);
    }

    private void RefreshTreePositions()
    {
        NativeArray<float3> nativePositions = new NativeArray<float3>(_sphereStates.Length, Allocator.TempJob);
        for (int i = 0; i < _sphereStates.Length; i++)
        {
            nativePositions[i] = _sphereStates[i].position;
        }

        var jobHandle = _tree.BuildTree(nativePositions);

        jobHandle.Complete();

        nativePositions.Dispose();
    }

    private void CheckSphereCollisions()
    {
        NativeArray<float3> nativePositions = new NativeArray<float3>(_sphereStates.Length, Allocator.TempJob);
        for (int i = 0; i < _sphereStates.Length; i++)
        {
            nativePositions[i] = _sphereStates[i].position;
        }

        DrawOnlySphereCollisionJob job = new DrawOnlySphereCollisionJob()
        {
            secondsSphereToBeRed = _secondsSphereToBeRed,
            speed = _speed,
            states = _sphereStates,
            positions = nativePositions,
            tree = _tree,
            range = _collisionRange
        };

        JobHandle jobHandle = job.Schedule(nativePositions.Length, 32);
        jobHandle.Complete();

        nativePositions.Dispose();
    }

    private void UpdateSpheresColorTimer()
    {
        for (int i = 0; i < _sphereStates.Length; i++)
        {
            if (_sphereStates[i].timeLeftToBeRed > 0)
            {
                var st = _sphereStates[i];
                st.timeLeftToBeRed -= Time.deltaTime;
                _sphereStates[i] = st;
            }
        }
    }

    private void ApplyVelocities()
    {
        DrawOnlyApplySphereVelocityJob job = new DrawOnlyApplySphereVelocityJob()
        {
            states = _sphereStates,
            deltaTime = Time.deltaTime,
        };

        JobHandle jobHandle = job.Schedule(_sphereStates.Length, 32);
        jobHandle.Complete();
    }

    private void ContainSpheresInPlayground()
    {
        DrawOnlyContainSpheresInPlaygroundJob job = new DrawOnlyContainSpheresInPlaygroundJob()
        {
            speed = _speed,
            states = _sphereStates,
            bounds = _volumeBounds
        };

        JobHandle jobHandle = job.Schedule(_sphereStates.Length, 32);
        jobHandle.Complete();
    }

    private void DrawSphereMeshes()
    {
        int redSpheresCount = _sphereStates.Count(x => x.timeLeftToBeRed > 0);

        Matrix4x4[] redMetrices = new Matrix4x4[redSpheresCount];
        Matrix4x4[] whiteMetrices = new Matrix4x4[_sphereStates.Length - redSpheresCount];

        int redIndex = 0;
        int whiteIndex = 0;
        for (int i = 0; i < _sphereStates.Length; i++)
        {
            if (_sphereStates[i].timeLeftToBeRed > 0)
            {
                redMetrices[redIndex].SetTRS(_sphereStates[i].position, Quaternion.identity, Vector3.one);
                redIndex++;
            }
            else
            {
                whiteMetrices[whiteIndex].SetTRS(_sphereStates[i].position, Quaternion.identity, Vector3.one);
                whiteIndex++;
            }
        }

        Graphics.RenderMeshInstanced(_rpRed, _mesh, 0, redMetrices);
        Graphics.RenderMeshInstanced(_rpWhite, _mesh, 0, whiteMetrices);
    }

    public override int GetSpheresCount() => _sphereStates.Length;

    public override int GetMinSpheresOnClick() => _minNumOfSphereOnClick;
    public override int GetMaxSpheresOnClick() => _maxNumOfSphereOnClick;

    public void Clear()
    {
        OnDestroy();
    }

    private void OnDestroy()
    {
        if (_sphereStates.IsCreated)
        {
            _sphereStates.Dispose();
        }

        if (_tree.IsCreated)
        {
            _tree.Dispose();
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