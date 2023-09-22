using Sirenix.OdinInspector;
using UnityEngine;

public class TestOctree : MonoBehaviour
{
    public Playground spheresManager;

    [Space]
    public BoxCollider boxColliderToSearchIn;

    public int nodeMinSize = 5;

    private Octree octree;

    private MaterialPropertyBlock block;

    [Space]
    public Collider cool_1;

    private void Start()
    {
        block = new MaterialPropertyBlock();
    }

    private void Update()
    {
        if (spheresManager.Spheres == null || spheresManager.Spheres.Count == 0)
        {
            return;
        }

        OctreeObject[] obs = new OctreeObject[spheresManager.Spheres.Count];
        for (int i = 0; i < obs.Length; i++)
        {
            obs[i] = new OctreeObject(spheresManager.Spheres[i].GetComponent<Collider>().bounds, i);
        }
        octree = new Octree(obs, nodeMinSize, spheresManager.PlaygroundBounds);

        Search();
    }

    private void OnDrawGizmos()
    {
        if (octree == null || octree.rootNode == null)
        {
            return;
        }
        
        if (Application.isPlaying)
        {
            octree.rootNode.Draw();
        }

        if (boxColliderToSearchIn)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(boxColliderToSearchIn.bounds.center, boxColliderToSearchIn.bounds.size);
        }
    }

    [Button("Search")]
    public void Search()
    {
        block.SetColor("_Color", Color.white);
        for (int i = 0; i < spheresManager.Spheres.Count; i++)
        {
            var mr = spheresManager.Spheres[i].GetComponent<MeshRenderer>();
            mr.SetPropertyBlock(block);
        }

        var found = octree.FindDataInBox(boxColliderToSearchIn.bounds);

        block.SetColor("_Color", Color.red);

        foreach (var f in found)
        {
           // Debug.Log(f.GetLocation());

            var mr = spheresManager.Spheres[f.index].GetComponent<MeshRenderer>();
            mr.SetPropertyBlock(block);
        }
    }

    [Button("LogDebugInfo")]
    public void LogDebugInfo()
    {
        Debug.Log(octree.rootNode.GetDebugInfo(""));
    }
}
