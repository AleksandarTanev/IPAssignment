using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallGenerator : MonoBehaviour
{
    [Range(1, 10000)]
    [SerializeField] private int _numOfBallsToGenerate;
    [SerializeField] private int _ballSpeed;

    [Space]
    [SerializeField] private Playground _playground;
    [SerializeField] private Ball _ballPrefab;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GenerateBalls();
        }
    }

    private void GenerateBalls()
    {
        for (int i = 0; i < _numOfBallsToGenerate; i++)
        {
            var newBall = Instantiate(_ballPrefab, this.transform);
            newBall.gameObject.SetActive(true);

            //newBall.transform.position = new Vector3(_playground.transform.position.x, _playground.transform.position.y, _playground.transform.position.z);
            newBall.transform.position = GetRandomPositionInPlayground();

            newBall.direction = (new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f))).normalized;
            //newBall.direction = Vector3.down;

            newBall.speed = _ballSpeed;
            newBall.velocity = newBall.direction * newBall.speed;
        }
    }

    private Vector3 GetRandomPositionInPlayground()
    {
        var wallSize = _playground.GetWallSize();
        var playgroundPosition = _playground.transform.position;
        var ballSize = _ballPrefab.transform.localScale.x;

        wallSize -= ballSize;
        var halfWallSize = wallSize / 2;

        float x = UnityEngine.Random.Range(-halfWallSize, halfWallSize);
        float y = UnityEngine.Random.Range(-halfWallSize, halfWallSize);
        float z = UnityEngine.Random.Range(-halfWallSize, halfWallSize);

        return new Vector3(x, y, z) + playgroundPosition;
    }

    /*
    public Transform targetWall;
    public List<Vector3> points;

    [Button("Test")]
    private void RandomizeDirections()
    {
        points.Clear();
        for (int i = 0; i < 10; i++)
        {
            var wallNormal = targetWall.transform.up;
            var randomDirection = (new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.1f, 1f), UnityEngine.Random.Range(-1f, 1f))).normalized;

            var direction = Quaternion.FromToRotation(Vector3.up, wallNormal) * randomDirection;

            points.Add(targetWall.position + direction * 5);
        }
    }

    private void OnDrawGizmos()
    {
        if (!targetWall)
        {
            return;
        }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(targetWall.position + targetWall.transform.up * 5, targetWall.position);
        Gizmos.color = Color.white;

        for (int i = 0; i < points.Count; i++)
        {
            Gizmos.DrawLine(points[i], targetWall.position);
        }
    }*/
}
