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
			rootNode.AddObject(go);
		}
    }
}

public class OctreeNode
{
	private Bounds nodeBounds;
    private float minSize;

	private Bounds[] childBounds;

	private OctreeNode[] children;

    public OctreeNode(Bounds b, float minNodeSize)
	{
		nodeBounds = b;
		minSize = minNodeSize;

		float quarter = nodeBounds.size.y / 4f;
		float childLength = nodeBounds.size.y / 2;
		var childActualSize = new Vector3(childLength, childLength, childLength);

        childBounds = new Bounds[8];
        childBounds[0] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, -quarter), childActualSize);
        childBounds[1] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, -quarter), childActualSize);
        childBounds[2] = new Bounds(nodeBounds.center + new Vector3(-quarter, quarter, quarter), childActualSize);
        childBounds[3] = new Bounds(nodeBounds.center + new Vector3(quarter, quarter, quarter), childActualSize);
        childBounds[4] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, -quarter), childActualSize);
        childBounds[5] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, -quarter), childActualSize);
        childBounds[6] = new Bounds(nodeBounds.center + new Vector3(-quarter, -quarter, quarter), childActualSize);
        childBounds[7] = new Bounds(nodeBounds.center + new Vector3(quarter, -quarter, quarter), childActualSize);
    }

	public void AddObject(GameObject go)
	{
		DivideAndAdd(go);
	}

    private void DivideAndAdd(GameObject go)
    {
		if (nodeBounds.size.y <= minSize)
		{
			return;
		}

		if (children == null)
		{
            children = new OctreeNode[8];
        }

		bool dividing = false;
		for (int i = 0; i < children.Length; i++)
		{
			if (children[i] == null)
			{
				children[i] = new OctreeNode(childBounds[i], minSize);
			}

			if (childBounds[i].Intersects(go.GetComponent<Collider>().bounds))
			{
				dividing = true;
				children[i].DivideAndAdd(go);
            }
		}

		if (!dividing)
		{
			children = null;
		}
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