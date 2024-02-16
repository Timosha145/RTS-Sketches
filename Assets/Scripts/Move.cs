using PoplarLib;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Color = UnityEngine.Color;

public class Move : MonoBehaviour
{
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private float _speed;
    private float _maxAngle = 45f;

    [SerializeField]
    private Rigidbody _pelvis;
    private ConfigurableJoint _joint;
    private Rigidbody _body;

    private void Awake()
    {
        _body = GetComponent<Rigidbody>();
        _joint = _pelvis.GetComponent<ConfigurableJoint>();
    }

    private void FixedUpdate()
    {
        Debug.DrawLine(transform.position, _target.position, Color.red);
        if (!PoplarUtils.IsCloseEnough(_target.position, transform.position, 3) && IsGrounded(out float groundAngle) && groundAngle < _maxAngle)
        {
            Vector3 _toTarget = _target.position - transform.position;
            Vector3 _toTargetXZ = new Vector3(_toTarget.x, 0, _toTarget.z);
            Quaternion rotation = Quaternion.LookRotation(_toTargetXZ);

            transform.rotation = Quaternion.Inverse(rotation);
            if (_body.velocity.magnitude < _speed)
            {
                float bonusSpeed = _speed * (groundAngle / 3f);
                Debug.Log($"BONUS SPEED: {bonusSpeed}");
                _body.AddForce((_body.transform.forward + new Vector3(0, groundAngle / 100, 0)) * (_speed + bonusSpeed), ForceMode.Impulse);
            }
        }
    }

    private bool IsGrounded(out float groundAngle)
    {
        groundAngle = 0f;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 0.8f, _groundLayer))
        {
            groundAngle = Vector3.Angle(hit.normal, Vector3.up);
            return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * 0.8f);
        Gizmos.color = Color.green * new Color(1, 1, 1, 0.3f);
        //Gizmos.DrawSphere(transform.position, 3);
    }
}
