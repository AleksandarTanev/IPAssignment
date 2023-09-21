using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class NOctree : IDisposable
{
    public const int MAX_DEPTH = 4;

    private NNode rootNode;

    public void Build(Bounds rootBounds, int maxDepth)
    {
        NativeArray<NNode> rootNode = new NativeArray<NNode>(1, Allocator.TempJob);

        var job = new BuildTreeJob()
        {
            rootBounds = rootBounds,
            maxDepth = maxDepth,
            rootNodes = rootNode,
        };
        var jobHandle = job.Schedule();
        jobHandle.Complete();

        rootNode.Dispose();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

public unsafe struct NNode
{
    public int depth;
    public Bounds bounds;
    public NNode[] childNodes;

    public byte* pointer;

    public void TrySplit()
    {
        if (CanSplit())
        {
            SplitNode();
            for (int i = 0; i < childNodes.Length; i++)
            {
                childNodes[i].TrySplit();
            }
        }
    }

    private void SplitNode()
    {
        //pointer = (byte*)UnsafeUtility.Malloc((sizeof(NNode) * 8), JobsUtility.CacheLineSize, Allocator.Persistent);

        float Offset = bounds.size.y / 4f;
        float childLength = bounds.size.y / 2;
        var childActualSize = new Vector3(childLength, childLength, childLength);

        childNodes = new NNode[8];
        childNodes[0] = new NNode() { bounds = new Bounds(bounds.center + new Vector3(-Offset, -Offset,  Offset), childActualSize) , depth = depth - 1};
        childNodes[1] = new NNode() { bounds = new Bounds(bounds.center + new Vector3( Offset, -Offset,  Offset), childActualSize) , depth = depth - 1};
        childNodes[2] = new NNode() { bounds = new Bounds(bounds.center + new Vector3(-Offset, -Offset, -Offset), childActualSize) , depth = depth - 1};
        childNodes[3] = new NNode() { bounds = new Bounds(bounds.center + new Vector3( Offset, -Offset, -Offset), childActualSize) , depth = depth - 1};
        childNodes[4] = new NNode() { bounds = new Bounds(bounds.center + new Vector3(-Offset,  Offset,  Offset), childActualSize) , depth = depth - 1};
        childNodes[5] = new NNode() { bounds = new Bounds(bounds.center + new Vector3( Offset,  Offset,  Offset), childActualSize) , depth = depth - 1};
        childNodes[6] = new NNode() { bounds = new Bounds(bounds.center + new Vector3(-Offset,  Offset, -Offset), childActualSize) , depth = depth - 1};
        childNodes[7] = new NNode() { bounds = new Bounds(bounds.center + new Vector3( Offset,  Offset, -Offset), childActualSize) , depth = depth - 1};
    }

    public bool CanSplit()
    {
        return depth <= NOctree.MAX_DEPTH;
    }
}

public struct BuildTreeJob : IJob
{
    public Bounds rootBounds;
    public int maxDepth;

    public NativeArray<NNode> rootNodes;

    public void Execute()
    {
        NNode rootNode = new NNode();
        rootNode.depth = 0;
        rootNode.bounds = rootBounds;

        rootNode.TrySplit();

        rootNodes[0] = rootNode;
    }
}