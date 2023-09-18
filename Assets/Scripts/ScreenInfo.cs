using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScreenInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ballCountText;

    private void Update()
    {
        _ballCountText.text = Sphere.Count.ToString();
    }
}
