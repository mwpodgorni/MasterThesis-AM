using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyorObject : MonoBehaviour
{
    [SerializeField] RelativeDirection _direction = RelativeDirection.Forward;
    [SerializeField] float velocity = 1.0f;

    Vector3 GetDirection()
    {
        switch(_direction)
        {
            case RelativeDirection.Left:
                return -transform.right;
            case RelativeDirection.Right:
                return transform.right;
            case RelativeDirection.Forward:
                return transform.forward;
            case RelativeDirection.Backward:
                return -transform.forward;
        }

        return transform.forward;
    }

    public void OnCollisionStay(Collision other)
    {
        if (other.rigidbody != null && !other.rigidbody.isKinematic)
        {
            Vector3 movement = velocity * GetDirection() * Time.deltaTime;
            other.gameObject.GetComponent<Rigidbody>().MovePosition(
                other.transform.position + movement
            );
        }
    }

    public enum RelativeDirection
    {
        Left,
        Right,
        Forward,
        Backward,
    }
}
