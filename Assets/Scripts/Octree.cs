using OtherOctree;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Octree
{
    public OctreeNode rootNode;

	public Octree(GameObject[] worldObjects, float minNodeSize)
	{
		Bounds bounds = new Bounds();

		foreach (var go in worldObjects)
		{
			bounds.Encapsulate(go.GetComponent<Collider>().bounds);
		}

		float maxSize = Mathf.Max(new float[] { bounds.size.x, bounds.size.y, bounds.size.z });
		Vector3 sizeVector = new Vector3(maxSize, maxSize, maxSize) * 0.5f;
		bounds.SetMinMax(bounds.center - sizeVector, bounds.center + sizeVector);

		rootNode = new OctreeNode(bounds, minNodeSize);

		AddObjects(worldObjects);
	}

    public void AddObjects(GameObject[] worldObjects)
    {
		foreach (var go in worldObjects)
		{
			rootNode.AddData(new OctreeObject(go.GetComponent<Collider>().bounds));
		}
    }
}

public struct OctreeObject
{
	private Bounds bounds;

    public OctreeObject(Bounds bounds)
    {
		this.bounds = bounds;
    }

    public Bounds GetBounds() => bounds;
}

public class OctreeNode
{
	public static int PreferredMaxDataPerNode = 1;

    private Bounds nodeBounds;
    private float minSize;

	private OctreeNode[] children;
	private HashSet<OctreeObject> data;

    public OctreeNode(Bounds b, float minNodeSize)
	{
		nodeBounds = b;
		minSize = minNodeSize;
    }

    bool CanSplit()
    {
        return nodeBounds.size.x >= minSize &&
               nodeBounds.size.y >= minSize &&
               nodeBounds.size.z >= minSize;
    }

    private void SplitNode()
	{
        float Offset = nodeBounds.size.y / 4f;
        float childLength = nodeBounds.size.y / 2;
        var childActualSize = new Vector3(childLength, childLength, childLength);

        children = new OctreeNode[8]
		{
			new OctreeNode(new Bounds(nodeBounds.center + new Vector3(-Offset, -Offset,  Offset), childActualSize), minSize),
			new OctreeNode(new Bounds(nodeBounds.center + new Vector3( Offset, -Offset,  Offset), childActualSize), minSize),
			new OctreeNode(new Bounds(nodeBounds.center + new Vector3(-Offset, -Offset, -Offset), childActualSize), minSize),
			new OctreeNode(new Bounds(nodeBounds.center + new Vector3( Offset, -Offset, -Offset), childActualSize), minSize),
			new OctreeNode(new Bounds(nodeBounds.center + new Vector3(-Offset,  Offset,  Offset), childActualSize), minSize),
			new OctreeNode(new Bounds(nodeBounds.center + new Vector3( Offset,  Offset,  Offset), childActualSize), minSize),
			new OctreeNode(new Bounds(nodeBounds.center + new Vector3(-Offset,  Offset, -Offset), childActualSize), minSize),
			new OctreeNode(new Bounds(nodeBounds.center + new Vector3( Offset,  Offset, -Offset), childActualSize), minSize)
		};
    }

	public void AddData(OctreeObject ob)
	{
        if (children == null)
        {
			// is this the first time we're adding data to this node
			if (data == null)
			{ 
				data = new HashSet<OctreeObject>(); 
			}

			// should we split AND are we able to split?
			if ((data.Count + 1) >= OctreeNode.PreferredMaxDataPerNode && CanSplit())
			{
				SplitNode();

				AddDataToChildren(ob);
			}
			else
			{ 
				data.Add(ob); 
			}

            return;
        }

        AddDataToChildren(ob);
	}

    private void AddDataToChildren(OctreeObject data)
    {
        foreach (var child in children)
        {
			if (child.Overlaps(data.GetBounds()))
			{ 
				child.AddData(data);
			}
        }
    }

    private bool Overlaps(Bounds Other)
    {
        return nodeBounds.Intersects(Other);
    }

    public void Draw()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(nodeBounds.center, nodeBounds.size);

		if (children != null)
		{
            foreach (var ch in children)
            {
				if (ch != null)
				{
                    ch.Draw();
                }
            }
        }
	}
}