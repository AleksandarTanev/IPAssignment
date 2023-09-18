using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

public class BallGenerator : MonoBehaviour
{
    [Range(1, 10000)]
    [SerializeField] private int _numOfBallsToGenerate;
    [SerializeField] private int _ballSpeed;

    [Space]
    [SerializeField] private Playground _playground;
    [SerializeField] private Sphere _spherePrefab;
}
