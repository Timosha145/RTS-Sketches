using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PawnSelections : MonoBehaviour
{
    [SerializeField] private int _maxSelected = 9;

    [field: SerializeField] public int PlayerPawnLayerId { get; private set; }
    [field: SerializeField] public LayerMask PlayerPawnLayer { get; private set; }
    [field: SerializeField] public int EnemyPawnLayerId { get; private set; }
    [field: SerializeField] public LayerMask EnemyPawnLayer { get; private set; }

    public static PawnSelections Instance { get; private set; }

    public List<PawnAI> PawnList = new List<PawnAI>();
    public List<PawnAI> PawnSelectedList = new List<PawnAI>();
    public PawnAI SelectedEnemyPawn;

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

    public void ClickSelect(PawnAI PawnAI)
    {
        DeselectAll();

        PawnAI.Select();
        PawnSelectedList.Add(PawnAI);
    }

    public void ShiftClickSelect(PawnAI PawnAI)
    {
        if (!PawnSelectedList.Contains(PawnAI) && PawnSelectedList.Count < _maxSelected)
        {
            PawnAI.Select();

            PawnSelectedList.Add(PawnAI);
        }
        else
        {
            PawnAI.Deselect();

            PawnSelectedList.Remove(PawnAI);
        }
    }

    public void DragSelect(PawnAI PawnAI)
    {
        if (!PawnSelectedList.Contains(PawnAI) && PawnSelectedList.Count < _maxSelected)
        {
            PawnAI.Select();

            PawnSelectedList.Add(PawnAI);
        }
    }

    public void DeselectAll()
    {
        foreach (PawnAI PawnAI in PawnSelectedList)
        {
            PawnAI.Deselect();
        }

        PawnSelectedList.Clear();
    }

    public void SelectEnemy(PawnAI PawnAI)
    {
        PawnAI.SelectAsEnemy();
        SelectedEnemyPawn = PawnAI;
    }

    public void DeselectEnemy()
    {
        if (SelectedEnemyPawn!=null)
        {
            SelectedEnemyPawn.Deselect();
            SelectedEnemyPawn = null;
        }
    }
}
