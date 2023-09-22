using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ballCountText;
    [SerializeField] private TextMeshProUGUI _fpsText;

    private float _deltaTime;
    private Playground playground;

    private void Start()
    {
        playground = FindObjectOfType<Playground>();
    }

    private void Update()
    {
        _ballCountText.text = playground.SpheresCount.ToString();

        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        float fps = 1.0f / _deltaTime;
        _fpsText.text = Mathf.Ceil(fps).ToString();
    }
}
