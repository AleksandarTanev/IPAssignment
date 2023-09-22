using OtherOctree;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

public class TestOctreeUnsafe : MonoBehaviour
{
   /* public SpheresManager spheresManager;

    private FOctree tree;

    private void Start()
    {
        //BuildTree();

    }

    [Button("Test KDTree")]
    private void Test()
    {
        var positions = spheresManager.spheres.Select(x => (float3)x.transform.position).ToArray();
        KDTree tree = new KDTree(positions.Length, Allocator.Persistent, KDTree.DefaultKDTreeParams);

        NativeArray<float3> nativePositions = new NativeArray<float3>(positions, Allocator.TempJob);
        var jobHandle = tree.BuildTree(nativePositions);
        jobHandle.Complete();
        nativePositions.Dispose();


        float3 asd = spheresManager.spheres[0].transform.position;
        NativeArray<KDTree.Neighbour> neighbours = new NativeArray<KDTree.Neighbour>(100, Allocator.TempJob);
        int count = tree.GetEntriesInRange(asd, 10, ref neighbours);

        Debug.Log(count);

        for (int i = 0; i < count; i++)
        {
            var xxx = neighbours[i];
            Debug.Log(xxx.index);
        }

        neighbours.Dispose();
        tree.Dispose();
    }

    // public static FNode[] flatNodeArray;

    //[Button("BuildTree")]
    private void BuildTree()
    {
        if (tree.isCreated)
        {
            tree.Dispose();
        }

        tree = new FOctree();
        tree.playgroundBounds = spheresManager.PlaygroundBounds;
        tree.BuildTree();

        var a = tree.GetNodeAtIndex(4);

        NativeHashSet<int> asd = new NativeHashSet<int>(1, Allocator.Temp);
        asd.Add(123);
        asd.Add(23);
        asd.Add(55);

        asd.Dispose();
    }

    public static int CalcLengthForDepth(int depth)
    {
        int arrayLength = 1;

        for (int i = 1; i <= depth; i++)
        {
            arrayLength += (int)Mathf.Pow(childrenPerNode, i);
        }

        return arrayLength;
    }

    private void OnDrawGizmos()
    {
        if (!tree.isCreated)
        {
            return;
        }

        Gizmos.color = Color.green;

        if (Application.isPlaying)
        {
            for (int i = 0; i < tree.m_Length; i++)
            {
                FNode node = tree.GetNodeAtIndex(i);
                Gizmos.DrawWireCube(node.bounds.center, node.bounds.size);
            }
        }
    }

    private void OnDestroy()
    {
        //tree.Dispose();
    }*/
}
/*
[NativeContainer]
[NativeContainerSupportsMinMaxWriteRestriction]
public unsafe struct FOctree : IDisposable
{
    public FNode* rootNodePtr;
    public int m_Length;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
    public int m_MinIndex;
    public int m_MaxIndex;
    private AtomicSafetyHandle m_Safety;
    [NativeSetClassTypeToNullOnSchedule] private DisposeSentinel m_DisposeSentinel;
#endif

    public bool isCreated;

    public static int depth = 2;
    public static int childrenPerNode = 8;

    private Allocator allocator;
    
    public Bounds playgroundBounds;

    public void BuildTree()
    {
        var length = CalcLengthForDepth(depth);
        allocator = Allocator.Persistent;

        long totalSize = UnsafeUtility.SizeOf<FNode>() * length;

        rootNodePtr = (FNode*)UnsafeUtility.Malloc(totalSize, JobsUtility.CacheLineSize, allocator);
        UnsafeUtility.MemClear(rootNodePtr, totalSize);

        var rootNode = new FNode()
        {
            depth = 0,
            index = 0,
            localIndex = 0,
            levelIndex = 0,
            bounds = playgroundBounds,
        };

        *rootNodePtr = rootNode;
        rootNode.SplitIfPossible(this);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        m_Length = length;
        m_MinIndex = 0;
        m_MaxIndex = length - 1;
        DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, allocator);
#endif

        isCreated = true;
    }

    public static int CalcLengthForDepth(int depth)
    {
        int arrayLength = 1;

        for (int i = 1; i <= depth; i++)
        {
            arrayLength += (int)Mathf.Pow(childrenPerNode, i);
        }

        return arrayLength;
    }

    public FNode GetNodeAtIndex(int index)
    {
        if (index < m_MinIndex || index > m_MaxIndex)
            Debug.LogError("Index out of range: " + index);

        return *(rootNodePtr + index);
    }

    public void Dispose()
    {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
        DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

        UnsafeUtility.Free(rootNodePtr, allocator);
        rootNodePtr = null;
        m_Length = 0;
    }

    public struct FNode
    {
        public int depth;

        public int index; // index in main array

        public int localIndex; // The index in between the 8 child nodes
        public int levelIndex; // The index in between all nodes on the same level/depth

        public Bounds bounds;

        public NativeHashSet<int> content;

        public void SplitIfPossible(FOctree tree)
        {
            if (depth + 1 > FOctree.depth)
            {
                return;
            }

            float offset = bounds.size.y / 4f;
            float childLength = bounds.size.y / 2;
            var childActualSize = new Vector3(childLength, childLength, childLength);

            Bounds[] childBounds = new Bounds[8];
            childBounds[0] = new Bounds(bounds.center + new Vector3(-offset, -offset,  offset), childActualSize);
            childBounds[1] = new Bounds(bounds.center + new Vector3( offset, -offset,  offset), childActualSize);
            childBounds[2] = new Bounds(bounds.center + new Vector3(-offset, -offset, -offset), childActualSize);
            childBounds[3] = new Bounds(bounds.center + new Vector3( offset, -offset, -offset), childActualSize);
            childBounds[4] = new Bounds(bounds.center + new Vector3(-offset,  offset,  offset), childActualSize);
            childBounds[5] = new Bounds(bounds.center + new Vector3( offset,  offset,  offset), childActualSize);
            childBounds[6] = new Bounds(bounds.center + new Vector3(-offset,  offset, -offset), childActualSize);
            childBounds[7] = new Bounds(bounds.center + new Vector3( offset,  offset, -offset), childActualSize);

            for (int i = 0; i < childrenPerNode; i++)
            {
                FNode newNode = new FNode();
                newNode.depth = depth + 1;
                newNode.localIndex = i;
                newNode.levelIndex = levelIndex * FOctree.childrenPerNode + i;
                newNode.index = CalcLengthForDepth(depth) + newNode.levelIndex;
                newNode.bounds = childBounds[i];

                var newPtr = (tree.rootNodePtr + newNode.index);
                *newPtr = newNode;

                newNode.SplitIfPossible(tree);
            }
        }

        public void AddObject(int x)
        {
            if (!content.IsCreated)
            {
                content = new NativeHashSet<int>();
            }

            content.Add(x);
        }

        public override string ToString()
        {
            return $"depth [{depth}] | index[{index}]";
        }
    }
}*/