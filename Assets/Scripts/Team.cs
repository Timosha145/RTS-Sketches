using System.Collections.Generic;
using UnityEngine;

public class Team : MonoBehaviour
{
    [SerializeField] public TeamDataSO TeamDataSO;

    public Material Material { get; private set; }
    public List<Pawn> PawnsInTeam { get; private set; }
    public List<TileBase> CapturedTiles { get; private set; }
    public int CurrentMaxPawns { get; private set; }
    public int CurrentQuantityOfPawns { get { return PawnsInTeam.Count; } }

    private void Awake()
    {
        CurrentMaxPawns = TeamDataSO.MinPawns;
        PawnsInTeam = new List<Pawn>();
        CapturedTiles = new List<TileBase>();

        Material = new Material(TeamDataSO.DefaultMaterial);
        Material.color = TeamDataSO.TeamColor;
    }

    public void IncreaseMaxPawns()
    {
        int possibleCurrentMaxPawns = TeamDataSO.MinPawns + CapturedTiles.Count + TeamDataSO.CaptureBonusModifier;
        CurrentMaxPawns = Mathf.Clamp(possibleCurrentMaxPawns, TeamDataSO.MinPawns, TeamDataSO.MaxPawns);
    }

    public void DescreaseMaxPawns()
    {
        int possibleCurrentMaxPawns = TeamDataSO.MinPawns + CapturedTiles.Count - TeamDataSO.CaptureBonusModifier;
        CurrentMaxPawns = Mathf.Clamp(possibleCurrentMaxPawns, TeamDataSO.MinPawns, TeamDataSO.MaxPawns);
    }

    public void AddPawn(Pawn pawn)
    {
        PawnsInTeam.Add(pawn);
    }

    public void RemovePawn(Pawn pawn)
    {
        PawnsInTeam.Remove(pawn);
    }

    public void AddTile(TileBase tile)
    {
        CapturedTiles.Add(tile);
    }

    public void RemoveTile(TileBase tile)
    {
        CapturedTiles.Remove(tile);
    }
}