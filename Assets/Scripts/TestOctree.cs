using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestOctree : MonoBehaviour
{
    public GameObject[] worldObjects;
    public int nodeMinSize = 5;

    private Octree octree;

    private void Start()
    {
        octree = new Octree(worldObjects, nodeMinSize);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            octree.rootNode.Draw();
        }
    }
}
