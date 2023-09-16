using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class Playground : MonoBehaviour
{
    [SerializeField] private int _wallSize;
    [SerializeField] private int _mulitiplier;
    [SerializeField] private GameObject _wallPrefab;

    [Button("Build Playground")]
    private void BuildPlayground()
    {
        for (int i = this.transform.childCount; i > 0; --i)
        { 
            DestroyImmediate(this.transform.GetChild(0).gameObject);
        }

        float r = (float)(_wallSize * _mulitiplier) / 2;
        float wallScale = _wallSize;

        // Creating and adjusting walls
        GameObject top = Instantiate(_wallPrefab, this.transform);
        top.gameObject.SetActive(true);
        top.name = "top";
        top.transform.rotation = Quaternion.Euler(180, 0, 0);
        top.transform.localScale= new Vector3(wallScale, 1, wallScale);

        GameObject bottom = Instantiate(_wallPrefab, this.transform);
        bottom.gameObject.SetActive(true);
        bottom.name = "bottom";
        bottom.transform.rotation = Quaternion.Euler(0, 0, 0);
        bottom.transform.localScale = new Vector3(wallScale, 1, wallScale);

        GameObject front = Instantiate(_wallPrefab, this.transform);
        front.gameObject.SetActive(true);
        front.name = "front";
        front.transform.rotation = Quaternion.Euler(0, 0, -90);
        front.transform.localScale = new Vector3(wallScale, 1, wallScale);

        GameObject back = Instantiate(_wallPrefab, this.transform);
        back.gameObject.SetActive(true);
        back.name = "back";
        back.transform.rotation = Quaternion.Euler(0, 0, 90);
        back.transform.localScale = new Vector3(wallScale, 1, wallScale);

        GameObject right = Instantiate(_wallPrefab, this.transform);
        right.gameObject.SetActive(true);
        right.name = "right";
        right.transform.rotation = Quaternion.Euler(-90, 0, 0);
        right.transform.localScale = new Vector3(wallScale, 1, wallScale);

        GameObject left = Instantiate(_wallPrefab, this.transform);
        left.gameObject.SetActive(true);
        left.name = "left";
        left.transform.rotation = Quaternion.Euler(90, 0, 0);
        left.transform.localScale = new Vector3(wallScale, 1, wallScale);

        // Setting walls position
        top.transform.position = new Vector3(0, r, 0) + this.transform.position;
        bottom.transform.position = new Vector3(0, -r, 0) + this.transform.position;

        front.transform.position = new Vector3(-r, 0,  0) + this.transform.position;
        back.transform.position = new Vector3(r, 0, 0) + this.transform.position;

        right.transform.position = new Vector3(0, 0, r) + this.transform.position;
        left.transform.position = new Vector3(0, 0, -r) + this.transform.position;
    }

    public float GetWallSize()
    {
        return _wallSize * _mulitiplier;
    }
}
