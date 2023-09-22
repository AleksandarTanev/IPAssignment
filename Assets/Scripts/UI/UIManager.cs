using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ballCountText;
    [SerializeField] private TextMeshProUGUI _fpsText;
    [SerializeField] private TextMeshProUGUI _minText;
    [SerializeField] private TextMeshProUGUI _maxText;

    private float _deltaTime;
    private PlaygroundBase playground;

    private void Start()
    {
        playground = FindObjectOfType<PlaygroundBase>();
    }

    private void Update()
    {
        _ballCountText.text = playground.GetSpheresCount().ToString();

        _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
        float fps = 1.0f / _deltaTime;
        _fpsText.text = Mathf.Ceil(fps).ToString();

        _minText.text = playground.GetMinSpheresOnClick().ToString();
        _maxText.text = playground.GetMaxSpheresOnClick().ToString();
    }
}
