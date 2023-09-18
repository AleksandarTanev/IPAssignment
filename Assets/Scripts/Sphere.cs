using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    public static int Count;
    public static float Speed = 10;

    public SphereState state;

    private Dictionary<string, Vector3> enteredColliders = new Dictionary<string, Vector3>();

    private void Start()
    {
        Count += 1;
    }

   /* private void OnTriggerEnter(Collider other)
    {
        Vector3 direction;

        enteredColliders.Add(other.name, other.transform.up);

        if (enteredColliders.Count == 1)
        {
            var wallNormal = other.gameObject.transform.up;
            var randomDirection = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(-1f, 1f));
            direction = (Quaternion.FromToRotation(Vector3.up, wallNormal) * randomDirection).normalized;
        }
        else
        {
            var vectorSum = Vector3.zero;
            foreach (var item in enteredColliders)
            {
                vectorSum += item.Value;
            }

            direction = vectorSum.normalized;
        }

        state.velocity = direction * Sphere.Speed;
    }

    private void OnTriggerExit(Collider other)
    {
        enteredColliders.Remove(other.name);
    }*/

    private void OnDestroy()
    {
        Count -= 1;
    }
}

public struct SphereState
{
    public Vector3 startingPosition;
    public Vector3 velocity;
}