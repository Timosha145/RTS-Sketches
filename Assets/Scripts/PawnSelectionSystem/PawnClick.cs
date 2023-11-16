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
                if (raycastHit.collider.TryGetComponent(out PawnAI PawnAI))
                {
                    PawnSelections.Instance.ShiftClickSelect(PawnAI);
                }
            }
            else
            {
                if (raycastHit.collider.TryGetComponent(out PawnAI PawnAI))
                {
                    PawnSelections.Instance.ClickSelect(PawnAI);
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
            // Reselect enemy PawnAI IF collider object has a PawnAI component AND previous call was not on the same gameobject
            if (raycastHit.collider.TryGetComponent(out PawnAI PawnAI) && PawnSelections.Instance.SelectedEnemyPawn != PawnAI)
            {
                PawnSelections.Instance.DeselectEnemy();

                if (PawnAI.Team != Player.Instance.Team)
                {
                    PawnSelections.Instance.SelectEnemy(PawnAI);
                }
            }
        }
        else
        {
            PawnSelections.Instance.DeselectEnemy();
        }
    }
}
