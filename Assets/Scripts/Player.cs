using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [field: SerializeField] public Team Team { get; private set; }
    [SerializeField] private LayerMask _groundLayerMask;

    public static Player Instance { get; private set; }

    private PawnSelections _pawnSelections;

    //private Order _order = Order.LineUpOnTarget;

    private enum Order
    {
        CircleTarget,
        LineUpOnTarget,
    }


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        GameInput.Instance.OnOrderPerformed += Instance_OnOrderPerformed;

        _pawnSelections = PawnSelections.Instance;
    }

    private void Instance_OnOrderPerformed(object sender, System.EventArgs e)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, _groundLayerMask))
        {
            if (_pawnSelections.SelectedEnemyPawn!=null)
            {
                foreach (PawnAI PawnAI in _pawnSelections.PawnSelectedList)
                {
                    PawnAI.FollowEnemy(_pawnSelections.SelectedEnemyPawn);
                }
            }
            else
            {
                PawnTask.LineUpPawnsOnTarget(_pawnSelections.PawnSelectedList, raycastHit.point);
            }

            //switch (_order)
            //{
            //    case Order.CircleTarget:
            //        PawnAI.CircleTarget(PawnSelections.Instance.PawnSelectedList, raycastHit.point);
            //        break;
            //    case Order.LineUpOnTarget:
            //        PawnAI.LineUpPawnsOnTarget(PawnSelections.Instance.PawnSelectedList, raycastHit.point);
            //        break;
            //    default:
            //        break;
            //}
        }
    }

}

