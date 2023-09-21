using OtherOctree;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using static FOctree;

public class TestOctreeUnsafe : MonoBehaviour
{
    public SpheresManager spheresManager;

    private FOctree tree;

    private void Start()
    {
        BuildFlatArray();
    }

    private void Test()
    {

    }

    // public static FNode[] flatNodeArray;

    [Button("BuildFlatArray")]
    private void BuildFlatArray()
    {
        tree = new FOctree();
        tree.playgroundBounds = spheresManager.PlaygroundBounds;
        tree.BuildTree();
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
            for (int i = 0; i < tree.length; i++)
            {
                FNode node = tree.GetNodeAtIndex(i);
                Gizmos.DrawWireCube(node.bounds.center, node.bounds.size);
            }
        }
    }

    private void OnDestroy()
    {
        tree.Dispose();
    }
}

public unsafe struct FOctree : IDisposable
{
    public bool isCreated;

    public static int depth = 2;
    public static int childrenPerNode = 8;

    public FNode* startPtr;
    public int length;

    private Allocator allocator;

    public Bounds playgroundBounds;

    public void BuildTree()
    {
        length = CalcLengthForDepth(depth);
        allocator = Allocator.Persistent;

        startPtr = (FNode*)UnsafeUtility.Malloc((sizeof(FNode) * length), JobsUtility.CacheLineSize, allocator);

        var rootNode = new FNode()
        {
            depth = 0,
            index = 0,
            localIndex = 0,
            levelIndex = 0,
            bounds = playgroundBounds,
            tree = this,
        };

        *startPtr = rootNode;
        rootNode.SplitIfPossible();

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
        return *(startPtr + index);
    }

    public void Dispose()
    {
        UnsafeUtility.Free(startPtr, allocator);
    }

    public struct FNode
    {
        public FOctree tree;

        public int depth;

        public int index; // index in main array

        public int localIndex; // The index in between the 8 child nodes
        public int levelIndex; // The index in between all nodes on the same level/depth

        public Bounds bounds;

        public void SplitIfPossible()
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
                newNode.tree = tree;
                newNode.bounds = childBounds[i];

                var newPtr = (tree.startPtr + newNode.index);
                *newPtr = newNode;

                newNode.SplitIfPossible();
            }
        }

        public override string ToString()
        {
            return $"depth [{depth}] | index[{index}]";
        }
    }
}