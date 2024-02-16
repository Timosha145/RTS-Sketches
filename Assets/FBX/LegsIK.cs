using PoplarLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

public class LegsIK : MonoBehaviour
{
    [SerializeField] private Transform _leftFootRig;
    [SerializeField] private Transform _leftLegHint;
    [SerializeField] private Transform _leftLegTarget;
    [SerializeField] private Transform _leftLegStepTarget;
    [Space]
    [SerializeField] private Transform _rightFootRig;
    [SerializeField] private Transform _rightLegHint;
    [SerializeField] private Transform _rightLegTarget;
    [SerializeField] private Transform _rightLegStepTarget;
    [Space]
    [SerializeField] private LayerMask _ground;
    [SerializeField] private Transform _target;

    private NavMeshAgent _agent;
    private float smoothness = 5.0f;
    private float _mainDistance = 0.3f;
    private float _leftLegDistance, _rightLegDistance;
    private bool _moveLeftLegNext = true;
    private bool _moveLeftLegTargetNext = true;

    private Vector3 _leftLegPos, _leftLegOldPos;
    private Vector3 _rightLegPos, _rightLegOldPos;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        _leftLegPos = _leftLegOldPos  = _leftLegTarget.position;
        _rightLegPos = _rightLegOldPos = _rightLegTarget.position;
        _leftLegDistance = _rightLegDistance = _mainDistance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _agent.SetDestination(_target.position);
        }

        if (Vector3.Distance(_leftLegTarget.position, _leftLegStepTarget.position) > _leftLegDistance && _moveLeftLegTargetNext)
        {
            Debug.Log("MOVE LEFT TAR");
            _moveLeftLegTargetNext = false;
            _leftLegDistance = _mainDistance;
            _rightLegDistance = _mainDistance * 2;
            _leftLegPos = GetGroundHitPoint(_leftLegStepTarget.position);
        }

        if (Vector3.Distance(_rightLegTarget.position, _rightLegStepTarget.position) > _rightLegDistance && !_moveLeftLegTargetNext)
        {
            Debug.Log("MOVE Right TAR");
            _moveLeftLegTargetNext = true;
            _rightLegDistance = _mainDistance;
            _leftLegDistance = _mainDistance * 2;
            _rightLegPos = GetGroundHitPoint(_rightLegStepTarget.position);
        }

        bool rightLegGrounded = IsGrounded(_rightFootRig.position, out float angleR);
        bool leftLegGrounded = IsGrounded(_leftFootRig.position, out float angleL);

        if (rightLegGrounded && _moveLeftLegNext)
        {
            Debug.Log("MOVE LEFT");
            _leftLegTarget.position = Vector3.Lerp(_leftLegTarget.position, _leftLegPos, Time.deltaTime * smoothness);
            _moveLeftLegNext = !PoplarUtils.IsCloseEnough(_leftLegTarget.position, _leftLegPos, 0.075f);
            _leftLegOldPos = _leftLegPos;
        }
        else
        {
            _leftLegTarget.position = _leftLegOldPos;
        }

        if (leftLegGrounded && !_moveLeftLegNext)
        {
            Debug.Log("MOVE Right");
            _rightLegTarget.position = Vector3.Lerp(_rightLegTarget.position, _rightLegPos, Time.deltaTime * smoothness);
            _moveLeftLegNext = PoplarUtils.IsCloseEnough(_rightLegTarget.position, _rightLegPos, 0.075f);
            _rightLegOldPos = _rightLegPos;
        }
        else
        {
            _rightLegTarget.position = _rightLegOldPos;
        }
    }

    Vector3 GetGroundHitPoint(Vector3 start)
    {
        RaycastHit hit;

        if (Physics.Raycast(start + Vector3.up * 0.1f, Vector3.down, out hit, 0.5f, _ground))
        {
            return hit.point;
        }

        return start;
    }

    protected bool IsGrounded(Vector3 pos, out float groundAngle)
    {
        groundAngle = 0f;

        if (Physics.Raycast(pos, Vector3.down, out RaycastHit hit, 0.4f, _ground))
        {
            groundAngle = Vector3.Angle(hit.normal, Vector3.up);
            return true;
        }

        return false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetGroundHitPoint(_leftLegPos), 0.1f);
        Gizmos.DrawSphere(GetGroundHitPoint(_rightLegPos), 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetGroundHitPoint(_leftLegOldPos), 0.1f);
        Gizmos.DrawSphere(GetGroundHitPoint(_rightLegOldPos), 0.1f);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(_rightFootRig.position, _rightFootRig.position + Vector3.down * 0.2f);
        Gizmos.DrawLine(_leftFootRig.position, _leftFootRig.position + Vector3.down * 0.2f);
    }
}
