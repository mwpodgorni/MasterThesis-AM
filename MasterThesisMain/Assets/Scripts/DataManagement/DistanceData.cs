using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceData : MonoBehaviour
{
    [SerializeField] Transform _target;

    [ObservableProperty] public float distance => Vector3.Distance(transform.position, _target.position);
}
