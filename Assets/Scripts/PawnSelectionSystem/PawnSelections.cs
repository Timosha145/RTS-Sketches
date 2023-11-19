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

    public List<Pawn> PawnList = new List<Pawn>();
    public List<Pawn> PawnSelectedList = new List<Pawn>();
    public Pawn SelectedEnemyPawn;

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

    public void ClickSelect(Pawn pawn)
    {
        DeselectAll();

        pawn.Select();
        PawnSelectedList.Add(pawn);
    }

    public void ShiftClickSelect(Pawn pawn)
    {
        if (!PawnSelectedList.Contains(pawn) && PawnSelectedList.Count < _maxSelected)
        {
            pawn.Select();

            PawnSelectedList.Add(pawn);
        }
        else
        {
            pawn.Deselect();

            PawnSelectedList.Remove(pawn);
        }
    }

    public void DragSelect(Pawn pawn)
    {
        if (!PawnSelectedList.Contains(pawn) && PawnSelectedList.Count < _maxSelected)
        {
            pawn.Select();

            PawnSelectedList.Add(pawn);
        }
    }

    public void DeselectAll()
    {
        foreach (Pawn pawn in PawnSelectedList)
        {
            pawn.Deselect();
        }

        PawnSelectedList.Clear();
    }

    public void SelectEnemy(Pawn pawn)
    {
        pawn.SelectAsEnemy();
        SelectedEnemyPawn = pawn;
    }

    public void DeselectEnemy()
    {
        if (SelectedEnemyPawn != null)
        {
            SelectedEnemyPawn.Deselect();
            SelectedEnemyPawn = null;
        }
    }
}
