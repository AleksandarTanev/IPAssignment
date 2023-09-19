using Sirenix.OdinInspector;
using UnityEngine;

public class TestOctree : MonoBehaviour
{
    [Space]
    public BoxCollider boxColliderToSearchIn;

    public GameObject[] worldObjects;
    public int nodeMinSize = 5;

    private Octree octree;

    private MaterialPropertyBlock block;

    [Space]
    public Collider cool_1;
    public Collider cool_2;


    private void Start()
    {
        block = new MaterialPropertyBlock();

        OctreeObject[] obs = new OctreeObject[worldObjects.Length];
        for (int i = 0; i < obs.Length; i++)
        {
            obs[i] = new OctreeObject(worldObjects[i].GetComponent<Collider>().bounds, i);
        }
        octree = new Octree(obs, nodeMinSize);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log(cool_1.bounds.Intersects(cool_2.bounds));
        }
    }

    private void OnDrawGizmos()
    {
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
        for (int i = 0; i < worldObjects.Length; i++)
        {
            var mr = worldObjects[i].GetComponent<MeshRenderer>();
            mr.SetPropertyBlock(block);
        }

        var found = octree.FindDataInBox(boxColliderToSearchIn.bounds);

        block.SetColor("_Color", Color.red);

        foreach (var f in found)
        {
           // Debug.Log(f.GetLocation());

            var mr = worldObjects[f.index].GetComponent<MeshRenderer>();
            mr.SetPropertyBlock(block);
        }
    }

    [Button("LogDebugInfo")]
    public void LogDebugInfo()
    {
        Debug.Log(octree.rootNode.GetDebugInfo(""));
    }
}
