using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ConfigurableJoint))]
public class CopyPhysicalBodyPart : MonoBehaviour
{
    [SerializeField]
    private Transform _target;
    private ConfigurableJoint _joint;
    private Quaternion _startRotation;

    private void Awake()
    {
        _joint = GetComponent<ConfigurableJoint>();
        GetComponent<Rigidbody>().maxAngularVelocity = 50f;
        _startRotation = transform.localRotation;
    }

    private void FixedUpdate()
    {
        _joint.targetRotation = Quaternion.Inverse(_target.localRotation) * _startRotation;
    }
}
