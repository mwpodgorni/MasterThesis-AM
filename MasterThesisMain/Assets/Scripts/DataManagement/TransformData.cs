using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformData : MonoBehaviour
{
    [ObservableProperty] public Vector3 position => transform.position;
    [ObservableProperty] public Quaternion rotation => transform.rotation;
    [ObservableProperty] public Vector3 scale => transform.localScale;
}
