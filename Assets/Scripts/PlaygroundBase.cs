using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlaygroundBase : MonoBehaviour
{
    public bool showPlaygroundBordersInScene;

    public abstract int GetSpheresCount();
    public abstract int GetMinSpheresOnClick();
    public abstract int GetMaxSpheresOnClick();
}
