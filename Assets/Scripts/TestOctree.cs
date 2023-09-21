using Sirenix.OdinInspector;
using UnityEngine;

public class TestOctree : MonoBehaviour
{
    public SpheresManager spheresManager;

    [Space]
    public BoxCollider boxColliderToSearchIn;

    //public GameObject[] worldObjects;
    public int nodeMinSize = 5;

    private Octree octree;

    private MaterialPropertyBlock block;

    [Space]
    public Collider cool_1;
    public Collider cool_2;


    private void Start()
    {
        block = new MaterialPropertyBlock();
        /*
        OctreeObject[] obs = new OctreeObject[worldObjects.Length];
        for (int i = 0; i < obs.Length; i++)
        {
            obs[i] = new OctreeObject(worldObjects[i].GetComponent<Collider>().bounds, i);
        }

        octree = new Octree(obs, nodeMinSize);*/
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log(cool_1.bounds.Intersects(cool_2.bounds));
        }

        if (spheresManager.spheres == null || spheresManager.spheres.Count == 0)
        {
            return;
        }

        OctreeObject[] obs = new OctreeObject[spheresManager.spheres.Count];
        for (int i = 0; i < obs.Length; i++)
        {
            obs[i] = new OctreeObject(spheresManager.spheres[i].GetComponent<Collider>().bounds, i);
        }
        octree = new Octree(obs, nodeMinSize, spheresManager.PlaygroundBounds);

        Search();
    }

    private void OnDrawGizmos()
    {
        //return;
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
        for (int i = 0; i < spheresManager.spheres.Count; i++)
        {
            var mr = spheresManager.spheres[i].GetComponent<MeshRenderer>();
            mr.SetPropertyBlock(block);
        }

        var found = octree.FindDataInBox(boxColliderToSearchIn.bounds);

        block.SetColor("_Color", Color.red);

        foreach (var f in found)
        {
           // Debug.Log(f.GetLocation());

            var mr = spheresManager.spheres[f.index].GetComponent<MeshRenderer>();
            mr.SetPropertyBlock(block);
        }
    }

    [Button("LogDebugInfo")]
    public void LogDebugInfo()
    {
        Debug.Log(octree.rootNode.GetDebugInfo(""));
    }
}
