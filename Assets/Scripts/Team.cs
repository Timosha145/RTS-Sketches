using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    [SerializeField] public TeamDataSO TeamDataSO;

    public Material Material { get; private set; }
    public List<Pawn> PawnsInTeam { get; private set; }
    public int CurrentMaxPawns { get; private set; }
    public int CurrentQuantityOfPawns { get { return PawnsInTeam.Count; } }

    private void Awake()
    {
        CurrentMaxPawns = TeamDataSO.MinPawns;
        PawnsInTeam = new List<Pawn>();

        Material = new Material(TeamDataSO.DefaultMaterial);
        Material.color = TeamDataSO.TeamColor;
    }

    public void IncreaseMaxPawns()
    {
        CurrentMaxPawns = Mathf.Clamp(CurrentQuantityOfPawns + TeamDataSO.CaptureBonusModifier, TeamDataSO.MinPawns, TeamDataSO.MaxPawns);
    }

    public void DescreaseMaxPawns()
    {
        CurrentMaxPawns = Mathf.Clamp(CurrentQuantityOfPawns - TeamDataSO.CaptureBonusModifier, TeamDataSO.MinPawns, TeamDataSO.MaxPawns);
    }

    public void AddPawn(Pawn pawn)
    {
        PawnsInTeam.Add(pawn);
    }

    public void RemovePawn(Pawn pawn)
    {
        PawnsInTeam.Remove(pawn);
    }
}