using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleFriction : MonoBehaviour
{
    [SerializeField]
    private PhysicMaterial _defaultFriction;
    [SerializeField]
    private PhysicMaterial _zeroFriction;
    [SerializeField]
    private Collider _leftCollider;
    [SerializeField]
    private Collider _rightCollider;

    private void SetLeftFriction()
    {
        _leftCollider.material = _defaultFriction;
        _rightCollider.material = _zeroFriction;
    }

    private void SetRightFriction()
    {
        _leftCollider.material = _zeroFriction;
        _rightCollider.material = _defaultFriction;
    }
}
