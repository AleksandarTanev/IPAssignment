using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Octree
{
    public OctreeNode rootNode;

	public Octree(OctreeObject[] objects, float minNodeSize)
	{
		Bounds bounds = new Bounds();

		foreach (var ob in objects)
		{
			bounds.Encapsulate(ob.GetBounds());
		}

		float maxSize = Mathf.Max(new float[] { bounds.size.x, bounds.size.y, bounds.size.z });
		Vector3 sizeVector = new Vector3(maxSize, maxSize, maxSize) * 0.5f;
		bounds.SetMinMax(bounds.center - sizeVector, bounds.center + sizeVector);

		rootNode = new OctreeNode(bounds, minNodeSize);

		AddObjects(objects);
	}

    public void AddObjects(OctreeObject[] objects)
    {
		foreach (var ob in objects)
		{
			rootNode.AddData(ob);
		}
    }

    public HashSet<OctreeObject> FindDataInRange(Vector3 searchLocation, float searchRange)
    {
        HashSet<OctreeObject> foundData = new HashSet<OctreeObject>();

        rootNode.FindDataInRange(searchLocation, searchRange, foundData);

        return foundData;
    }

    public HashSet<OctreeObject> FindDataInBox(Bounds searchBounds)
    {
        HashSet<OctreeObject> foundData = new HashSet<OctreeObject>();

        rootNode.FindDataInBox(searchBounds, foundData);

        return foundData;
    }
}

public class OctreeObject
{
    public int index;

	private Bounds bounds;

    public OctreeObject(Bounds bounds, int index)
    {
		this.bounds = bounds;
		this.index = index;
    }

    public Bounds GetBounds() => bounds;
    public float GetRadius() => bounds.extents.x;
    public Vector3 GetLocation() => bounds.center;
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
			if ((data.Count) >= OctreeNode.PreferredMaxDataPerNode && CanSplit())
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

    private bool Overlaps(Bounds other)
    {
        return nodeBounds.Intersects(other);
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

        foreach (var d in data)
        {
            AddDataToChildren(d);
        }

        data = null;
    }

    public void FindDataInRange(Vector3 searchLocation, float searchRange, HashSet<OctreeObject> outFoundData)
    {
        Bounds SearchBounds = new Bounds(searchLocation, searchRange * Vector3.one * 2f);

        FindDataInBox(SearchBounds, outFoundData, false);

        outFoundData.RemoveWhere(Datum =>
        {
            float testRange = searchRange + Datum.GetRadius();

            return (searchLocation - Datum.GetLocation()).sqrMagnitude > (testRange * testRange);
        });
    }

    public void FindDataInBox(Bounds searchBounds, HashSet<OctreeObject> outFoundData, bool bExactBounds = true)
    {
        if (children == null)
        {
            if (data == null || data.Count == 0)
                return;

            // optimised check for a root node with no children
            if (bExactBounds)
            {
                foreach (var d in data)
                {
                    if (searchBounds.Intersects(d.GetBounds()))
                    { 
                        outFoundData.Add(d);
                    }
                }

                return;
            }

            outFoundData.UnionWith(data);

            return;
        }

        foreach (var c in children)
        {
            if (c.Overlaps(searchBounds))
            {
                Debug.Log(1);

                c.FindDataInBox(searchBounds, outFoundData, bExactBounds);
            }
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

    public string GetDebugInfo(string prefix)
    {
        string msg;
        if (data == null)
        {
            msg = prefix + "0";
        }
        else
        {
            msg = prefix + data.Count() + " | " + $"[{string.Join(", ", data.Select(x => x.index))}]";
        }

        if (children != null)
        {
            foreach (var c in children)
            {
                msg += "\n" + c.GetDebugInfo(prefix + "-");
            }
        }

        return msg;
    }
}