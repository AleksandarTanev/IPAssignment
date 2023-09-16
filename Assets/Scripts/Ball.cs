using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public static int count;

    public Vector3 velocity;

    public float speed;
    public Vector3 direction;

    private Dictionary<string, Vector3> enteredColliders = new Dictionary<string, Vector3>();

    private void Start()
    {
        count += 1;
    }

    private void FixedUpdate()
    {
        this.transform.position += (velocity * Time.fixedDeltaTime);
    }
    
    private void OnTriggerEnter(Collider other)
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

        velocity = direction * speed;
        /*
        if (enteredColliders.Count > 1)
        {
            string msg = "| ";
            foreach (var item in enteredColliders)
            {
                msg += item + " |";
            }
            Debug.Log(msg);
        }*/
    }

    private void OnTriggerExit(Collider other)
    {
        enteredColliders.Remove(other.name);
    }

    private void OnDestroy()
    {
        count -= 1;
    }
}
