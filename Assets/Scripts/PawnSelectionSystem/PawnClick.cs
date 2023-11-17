using Unity.VisualScripting;
using UnityEngine;

public class PawnClick : MonoBehaviour
{
    private Ray _ray;

    private void Start()
    {
        GameInput.Instance.OnInteractPerformed += GameInput_OnInteractPerformed;
    }

    private void Update()
    {
        HandleEnemySelection();
    }

    private void GameInput_OnInteractPerformed(object sender, System.EventArgs e)
    {
        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out RaycastHit raycastHit, float.MaxValue, PawnSelections.Instance.PlayerPawnLayer))
        {
            // Ray hit a clickable object
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (raycastHit.collider.TryGetComponent(out Pawn Pawn))
                {
                    PawnSelections.Instance.ShiftClickSelect(Pawn);
                }
            }
            else
            {
                if (raycastHit.collider.TryGetComponent(out Pawn Pawn))
                {
                    PawnSelections.Instance.ClickSelect(Pawn);
                }
            }
        }
        else
        {
            // Ray didn't hit a clickable object
            if (!Input.GetKey(KeyCode.LeftShift))
            {
                PawnSelections.Instance.DeselectAll();
            }
        }
    }

    private void HandleEnemySelection()
    {
        if (PawnSelections.Instance.PawnSelectedList.Count == 0)
        {
            return;
        }

        _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(_ray, out RaycastHit raycastHit, float.MaxValue, PawnSelections.Instance.EnemyPawnLayer))
        {
            // Reselect enemy Pawn IF collider object has a Pawn component AND previous call was not on the same gameobject
            if (raycastHit.collider.TryGetComponent(out Pawn Pawn) && PawnSelections.Instance.SelectedEnemyPawn != Pawn)
            {
                PawnSelections.Instance.DeselectEnemy();

                if (Pawn.Team != Player.Instance.Team)
                {
                    PawnSelections.Instance.SelectEnemy(Pawn);
                }
            }
        }
        else
        {
            PawnSelections.Instance.DeselectEnemy();
        }
    }
}
