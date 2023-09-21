using UnityEngine;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using Unity.Collections;
using Unity.VisualScripting;

public class OctreeUnsafe
{
    public static Bounds playground;
    public static Bounds[] allBounds;

    public static int PreferredMaxDataPerNode = 4;
    public static int MinSize = 1;

    private OctreeNodeUnsafe rootNode;

    public void BuildTree(Bounds bounds)
    {
        playground = bounds;
        rootNode = new OctreeNodeUnsafe(bounds);
    }

    public void SetObjects(Bounds[] objects)
    {
        /*allBounds = objects;

        for (int i = 0; i < allBounds.Length; i++)
        {
            rootNode.AddData(i);
        }*/

        allBounds = objects;
        NativeArray<Bounds> n = new NativeArray<Bounds>(allBounds, Allocator.TempJob);

        NativeArray<OctreeNodeUnsafe> rn = new NativeArray<OctreeNodeUnsafe>(1, Allocator.TempJob);
        rn[0] = new OctreeNodeUnsafe(playground);

        var job = new SetObjJob()
        {
            allBounds = n,
            rootNode = rn
        };
        var handle = job.Schedule();

        handle.Complete();

        rootNode = rn[0];

        n.Dispose();
        rn.Dispose();
    }
}

public struct SetObjJob : IJob
{
    public NativeArray<Bounds> allBounds;

    public NativeArray<OctreeNodeUnsafe> rootNode;

    public void Execute()
    {
        for (int i = 0; i < allBounds.Length; i++)
        {
           // rootNode[0].AddData(i);
        }
    }
}

public struct OctreeNodeUnsafe
{
    public Bounds nodeBounds;

    //public OctreeNodeUnsafe[] childNodes;
   // public HashSet<int> dataIndexes;

    public OctreeNodeUnsafe(Bounds bounds)
    {
        nodeBounds = bounds;
       // dataIndexes = new HashSet<int>();
      //  childNodes = null;
    }
    /*
    private void SplitNode()
    {
        float Offset = nodeBounds.size.y / 4f;
        float childLength = nodeBounds.size.y / 2;
        var childActualSize = new Vector3(childLength, childLength, childLength);

        childNodes = new OctreeNodeUnsafe[8]
        {
            new OctreeNodeUnsafe(new Bounds(nodeBounds.center + new Vector3(-Offset, -Offset,  Offset), childActualSize)),
            new OctreeNodeUnsafe(new Bounds(nodeBounds.center + new Vector3( Offset, -Offset,  Offset), childActualSize)),
            new OctreeNodeUnsafe(new Bounds(nodeBounds.center + new Vector3(-Offset, -Offset, -Offset), childActualSize)),
            new OctreeNodeUnsafe(new Bounds(nodeBounds.center + new Vector3( Offset, -Offset, -Offset), childActualSize)),
            new OctreeNodeUnsafe(new Bounds(nodeBounds.center + new Vector3(-Offset,  Offset,  Offset), childActualSize)),
            new OctreeNodeUnsafe(new Bounds(nodeBounds.center + new Vector3( Offset,  Offset,  Offset), childActualSize)),
            new OctreeNodeUnsafe(new Bounds(nodeBounds.center + new Vector3(-Offset,  Offset, -Offset), childActualSize)),
            new OctreeNodeUnsafe(new Bounds(nodeBounds.center + new Vector3( Offset,  Offset, -Offset), childActualSize))
        };

        foreach (var i in dataIndexes)
        {
            AddDataToChildren(i);
        }

        dataIndexes = null;
    }

    private void AddDataToChildren(int index)
    {
        for (int i = 0; i < childNodes.Length; i++)
        {
            if (childNodes[i].Overlaps(OctreeUnsafe.allBounds[index]))
            {
                childNodes[i].AddData(index);
            }
        }
    }

    public void AddData(int index)
    {
        if (childNodes == null)
        {
            // is this the first time we're adding data to this node
            if (dataIndexes == null)
            {
                dataIndexes = new HashSet<int>();
            }

            // should we split AND are we able to split?
            if ((dataIndexes.Count) >= OctreeNode.PreferredMaxDataPerNode && CanSplit())
            {
                SplitNode();

                AddDataToChildren(index);
            }
            else
            {
                dataIndexes.Add(index);
            }

            return;
        }

        AddDataToChildren(index);
    }

    private bool CanSplit()
    {
        return nodeBounds.size.x >= OctreeUnsafe.MinSize &&
               nodeBounds.size.y >= OctreeUnsafe.MinSize &&
               nodeBounds.size.z >= OctreeUnsafe.MinSize;
    }

    private bool Overlaps(Bounds other)
    {
        return nodeBounds.Intersects(other);
    }*/
}

/*
[NativeContainer]
[NativeContainerSupportsMinMaxWriteRestriction]
public unsafe struct OctreeUnsafe : IDisposable
{
    private int m_Capacity;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
    internal int m_Length;
    internal int m_MinIndex;
    internal int m_MaxIndex;
    AtomicSafetyHandle m_Safety;
    [NativeSetClassTypeToNullOnSchedule] DisposeSentinel m_DisposeSentinel;
#endif

    internal Allocator m_AllocatorLabel;

    public OctreeUnsafe(int capacity, Allocator allocator)
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        // Native allocation is only valid for Temp, Job and Persistent
        if (allocator <= Allocator.None)
            throw new ArgumentException("Allocator must be Temp, TempJob or Persistent", "allocator");
        if (capacity < 0)
            throw new ArgumentOutOfRangeException("capacity", "Capacity must be >= 0");
#endif

        m_Capacity = capacity;
        m_AllocatorLabel = allocator;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        m_Length = m_Capacity;
        m_MinIndex = 0;
        m_MaxIndex = -1;
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, allocator);
#endif

    }

    [WriteAccessRequired]
    public void Dispose()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif
    }
}*/