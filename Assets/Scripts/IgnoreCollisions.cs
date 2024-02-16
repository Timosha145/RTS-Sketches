using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreCollisions : MonoBehaviour
{
    [SerializeField] private Collider[] _ignoreColliders;

    private void Awake()
    {
        for (int a = 0; a < _ignoreColliders.Length; a++)
        {
            for (int b = 0; b < _ignoreColliders.Length; b++)
            {
                Physics.IgnoreCollision(_ignoreColliders[a], _ignoreColliders[b], true);
            }
        }
    }
}
