using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScreenInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ballCountText;


    private SpheresManager manager;

    private void Start()
    {
        manager = FindObjectOfType<SpheresManager>();
    }

    private void Update()
    {
        _ballCountText.text = manager.SpheresCount.ToString();
    }
}
