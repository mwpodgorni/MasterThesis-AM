using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Task : MonoBehaviour
{
    public string taskDescription;

    public abstract bool IsComplete();
}
