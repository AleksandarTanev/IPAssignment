using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Linq;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class TestOctreeUnsafe : MonoBehaviour
{
    public SpheresManager spheresManager;

    private void Test()
    {

        /*var tree = new NOctree();
        tree.Build(spheresManager.PlaygroundBounds, 4);*/
    }

    [Space]
    public static int depth = 2;
    public static int childrenPerNode = 8;

   // public static FNode[] flatNodeArray;

    [Button("BuildFlatArray")]
    private void BuildFlatArray()
    {
        FOctree tree = new FOctree();
        tree.BuildTree();

       /* flatNodeArray = new FNode[CalcLengthForDepth(depth)];

        var rootNode = new FNode();
        rootNode.depth = 0;
        rootNode.index = 0;
        rootNode.localIndex = 0;
        rootNode.levelIndex = 0;

        flatNodeArray[0] = rootNode;

        rootNode.SplitIfPossible();

        Debug.Log(CalcLengthForDepth(depth));

        TestUnsafe();*/
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

    /*
    private unsafe void TestUnsafe()
    {
        var length = CalcLengthForDepth(depth);
        var alloc = Allocator.Temp;

        var startPtr = (FNode*)UnsafeUtility.Malloc((sizeof(FNode) * length), JobsUtility.CacheLineSize, alloc);

        var rootNode = new FNode()
        {
            depth = 0,
            index = 0,
            localIndex = 0,
            levelIndex = 0,
        };

        *startPtr = rootNode;
        rootNode.SplitIfPossible();

        Debug.Log(*(startPtr + 4)); //Display the last value '5'

        UnsafeUtility.Free(startPtr, alloc);
    }

    public unsafe void SetNode(FNode* firstElemPointer, int nodeIndex)
    {
        FNode* nodePtr = firstElemPointer + nodeIndex;

        *nodePtr = new FNode();
    }*/
}

public unsafe struct FOctree
{
    public static int depth = 1;
    public static int childrenPerNode = 8;

    public FNode* startPtr;

    public void BuildTree()
    {
        var length = CalcLengthForDepth(depth);
        var alloc = Allocator.Temp;

        startPtr = (FNode*)UnsafeUtility.Malloc((sizeof(FNode) * length), JobsUtility.CacheLineSize, alloc);

        try
        {
            var rootNode = new FNode()
            {
                depth = 0,
                index = 0,
                localIndex = 0,
                levelIndex = 0,
                tree = this,
            };

            *startPtr = rootNode;
            rootNode.SplitIfPossible();

        }
        catch (Exception e)
        {
            throw e;
        }
        finally
        {
            UnsafeUtility.Free(startPtr, alloc);
        }
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

    public struct FNode
    {
        public FOctree tree;

        public int depth;

        public int index; // index in main array

        public int localIndex; // The index in between the 8 child nodes
        public int levelIndex; // The index in between all nodes on the same level/depth

        //public int[] objects;

        public void SplitIfPossible()
        {
            if (depth + 1 > FOctree.depth)
            {
                return;
            }

            for (int i = 0; i < FOctree.childrenPerNode; i++)
            {
                // Create child node
                FNode newNode = new FNode();
                newNode.depth = depth + 1;
                newNode.localIndex = i;
                newNode.levelIndex = levelIndex * FOctree.childrenPerNode + i;
                newNode.index = FOctree.CalcLengthForDepth(depth) + newNode.levelIndex;
                newNode.tree = tree;
                //newNode.objects = new int[2] { 33, 51 };

                var newPtr = (tree.startPtr + newNode.index);
                *newPtr = newNode;
                //TestOctreeUnsafe.flatNodeArray[newNode.index] = newNode;

                newNode.SplitIfPossible();
            }
        }

        public override string ToString()
        {
            return $"depth [{depth}] | index[{index}]";
        }
    }
}



public unsafe static class TestUnsafe
{
    public static void Do()
    {
        int[] intArray = new int[5] { 1, 2, 3, 4, 5 };

        fixed (int* arrayPtr = &intArray[0])
        {
            Debug.Log(*(arrayPtr + 4)); //Display the last value '5'
        }
    }
}

[NativeContainer]
public unsafe struct TestConteiner
{

}