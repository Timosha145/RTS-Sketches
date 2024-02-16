using PoplarLib;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

public class FootSolver : MonoBehaviour
{
    [SerializeField] private LayerMask _terrain;
    [SerializeField] private FootSolver _parallelFoot;
    [SerializeField] private float _stepDistance;
    [SerializeField] private float _stepHeight;
    [SerializeField] private float _speed;
    [SerializeField] private Transform _body;
    [SerializeField] private Vector3 _footOffset;

    private Vector3 _oldPos;
    private Vector3 _newPos;
    private Vector3 _currentPos;
    private Vector3 _oldNormal;
    private Vector3 _newNormal;
    private Vector3 _currentNormal;
    private Vector3 _legOffset;
    private float _lerp = 1f;
    private float _footSpacing;

    private void Start()
    {
        _footSpacing = transform.localPosition.x;
        _oldPos = _newPos = _currentPos = transform.position;
        _oldNormal = _currentNormal = _newNormal = transform.up;
        _legOffset = transform.up;
        Debug.Log("OFFSET: " + _legOffset);
    }

    private void Update()
    {
        transform.position = _currentPos;
        //transform.up = _currentNormal;

        Debug.Log("Transform up: " + transform.up);

        Ray ray = new Ray(_body.position + (_body.right * _footSpacing), Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 10, _terrain))
        {
            if (Vector3.Distance(_newPos, hit.point) > _stepDistance && !_parallelFoot.isMoving() && _lerp >= 1)
            {
                int direction = _body.InverseTransformPoint(hit.point).z > _body.InverseTransformPoint(_newPos).z ? 1 : -1;
                _lerp = 0;
                _newPos = hit.point + (_body.forward * _stepDistance * direction) + _footOffset;
                _newNormal = hit.normal;
            }
        }

        if(_lerp < 1)
        {
            Vector3 tempPos = Vector3.Lerp(_oldPos, _newPos, _lerp);
            tempPos.y += Mathf.Sin(_lerp * Mathf.PI) * _stepHeight;
            _currentPos = tempPos;
            _currentNormal = Vector3.Lerp(_oldNormal, _newNormal, _lerp);
            _lerp += Time.deltaTime * _speed;
        }
        else
        {
            Debug.Log("ELSE");
            _oldPos = _newPos;
            _oldNormal = _newNormal;
        }
    }

    public bool isMoving()
    {
        return _lerp < 1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(_oldPos, 0.2f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(_newPos, 0.2f);
        Gizmos.color = Color.gray;
        Gizmos.DrawSphere(_currentPos, 0.2f);
    }
}
