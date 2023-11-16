using Unity.Mathematics;
using UnityEngine;

public class DragClick : MonoBehaviour
{
    [SerializeField] private RectTransform _boxVisual;

    private Rect _selectionBox;
    private Vector2 _startPos, _endPos;

    private void Start()
    {
        GameInput.Instance.OnInteractPerformed += GameInput_OnInteractPerformed;
        GameInput.Instance.OnInteractCanceled += GameInput_OnInteractCanceled;

        DrawVisual(_startPos, _endPos);
    }

    private void Update()
    {
        if (GameInput.Instance.HoldingInteract)
        {
            _endPos = Input.mousePosition;
            DrawVisual(_startPos, _endPos);
            DrawSelection(_startPos);
        }
    }

    private void GameInput_OnInteractCanceled(object sender, System.EventArgs e)
    {
        _startPos = Vector2.zero;
        _endPos = Vector2.zero;

        SelectPawns();
        DrawVisual(_startPos, _endPos);
    }

    private void GameInput_OnInteractPerformed(object sender, System.EventArgs e)
    {
        _startPos = Input.mousePosition;
    }

    private void DrawVisual(Vector2 boxStart, Vector2 boxEnd)
    {
        _boxVisual.position = (boxStart + boxEnd) / 2; // Get the center of the box
        _boxVisual.sizeDelta = new Vector2(math.abs(boxStart.x - boxEnd.x), math.abs(boxStart.y - boxEnd.y)); // Get the size of the box
    }

    private void DrawSelection(Vector2 boxStart)
    {
        // X calculations
        if (Input.mousePosition.x < boxStart.x)
        {
            // Dragging to the left
            _selectionBox.xMin = Input.mousePosition.x;
            _selectionBox.xMax = boxStart.x;
        }
        else
        {
            // Dragging to the right
            _selectionBox.xMin = boxStart.x;
            _selectionBox.xMax = Input.mousePosition.x;
        }

        // X calculations
        if (Input.mousePosition.y < boxStart.y)
        {
            // Dragging down
            _selectionBox.yMin = Input.mousePosition.y;
            _selectionBox.yMax = boxStart.y;
        }
        else
        {
            // Dragging up
            _selectionBox.yMin = boxStart.y;
            _selectionBox.yMax = Input.mousePosition.y;
        }
    }

    private void SelectPawns()
    {
        foreach (PawnAI PawnAI in PawnSelections.Instance.PawnList)
        {
            if (_selectionBox.Contains(Camera.main.WorldToScreenPoint(PawnAI.transform.position)) && PawnAI.Team == Player.Instance.Team)
            {
                // If any PawnAI is within the selection add them to selection
                PawnSelections.Instance.DragSelect(PawnAI);
            }
        }
    }
}
