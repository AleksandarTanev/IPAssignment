using Ditzel;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TestKD : MonoBehaviour
{
    public Playground spheresManager;

    public GameObject objectToSearchAround;

    KDTree kdTree;
    NativeArray<float3> positions;

    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        
    }

    [Button("Build Tree")]
    private void BuildTree()
    {
        kdTree = new KDTree(spheresManager.Spheres.Count, Allocator.Persistent);

        positions = new NativeArray<float3>(spheresManager.Spheres.Count, Allocator.Persistent);
        for (int i = 0; i < spheresManager.Spheres.Count; i++)
        {
            positions[i] = new float3(spheresManager.Spheres[i].transform.position.x, spheresManager.Spheres[i].transform.position.y, spheresManager.Spheres[i].transform.position.z);
        }

        JobHandle jobHandle = kdTree.BuildTree(positions);

        jobHandle.Complete();
    }

    [Button("Search")]
    private void Search()
    {

        var f = new float3(objectToSearchAround.transform.position.x, objectToSearchAround.transform.position.y, objectToSearchAround.transform.position.z);

        NativeArray<KDTree.Neighbour> neighbours = new NativeArray<KDTree.Neighbour>();
        var asd = kdTree.GetEntriesInRange(f, 30, ref neighbours);

        Debug.Log(asd);
    }

    private void OnDestroy()
    {
        if (positions.IsCreated)
        {
            positions.Dispose();
        }

        if (kdTree.IsCreated)
        {
            kdTree.Dispose();
        }
    }
}
