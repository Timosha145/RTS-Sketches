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

    public void ClickSelect(Pawn Pawn)
    {
        DeselectAll();

        Pawn.Select();
        PawnSelectedList.Add(Pawn);
    }

    public void ShiftClickSelect(Pawn Pawn)
    {
        if (!PawnSelectedList.Contains(Pawn) && PawnSelectedList.Count < _maxSelected)
        {
            Pawn.Select();

            PawnSelectedList.Add(Pawn);
        }
        else
        {
            Pawn.Deselect();

            PawnSelectedList.Remove(Pawn);
        }
    }

    public void DragSelect(Pawn Pawn)
    {
        if (!PawnSelectedList.Contains(Pawn) && PawnSelectedList.Count < _maxSelected)
        {
            Pawn.Select();

            PawnSelectedList.Add(Pawn);
        }
    }

    public void DeselectAll()
    {
        foreach (Pawn Pawn in PawnSelectedList)
        {
            Pawn.Deselect();
        }

        PawnSelectedList.Clear();
    }

    public void SelectEnemy(Pawn Pawn)
    {
        Pawn.SelectAsEnemy();
        SelectedEnemyPawn = Pawn;
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
