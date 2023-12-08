using PoplarLib;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Team))]
public class Bot : ExtendedMonoBehaviour
{
    private float _timerToMakeMove, _timerToMakeMoveMax = 5f;
    private float _orderPosThreshold = 1f;
    private float _scorePerPawn;
    private Team _team;

    private class Group
    {
        public List<Pawn> Pawns { get; }
        public int MaxPawns { get; }
        public Vector3 Destination { get; }

        public Group(List<Pawn> pawns, int maxPawns, Vector3 destination)
        {
            Pawns = pawns;
            MaxPawns = maxPawns;
            Destination = destination;
        }

        public bool IsGroupFull()
        {
            return Pawns.Count < MaxPawns;
        }
    }

    private void Start()
    {
        _team = GetComponent<Team>();
        _scorePerPawn = _team.TeamDataSO.Health + _team.TeamDataSO.Damage;
    }

    private void Update()
    {
        if (HandleTimer(ref _timerToMakeMove, _timerToMakeMoveMax))
        {
            SendPawnsToCapture();
        }
    }

    private void SendPawnsToCapture()
    {
        if (_team.CapturedTiles.Count == GameManager.Instance.Tiles.Count)
        {
            Debug.Log($"All tiles were captured by Team {_team.TeamDataSO.name}");
            return;
        }

        List<TileBase> tilesToCaptureForPawns = new List<TileBase>();
        List<Group> formedPawnGroups = new List<Group>();
        List<Pawn> availablePawns = _team.PawnsInTeam;
        List<Vector3> excludeTilePositions = new List<Vector3>();

        foreach (Pawn pawn in availablePawns)
        {
            RepeatIteration:
                TileBase closestUncapturedTile = GetClosestUncapturedTilePos(pawn.transform.position, excludeTilePositions);

                if (IsPositionCloseEnough(pawn.GetDestanation(), closestUncapturedTile.transform.position, _orderPosThreshold))
                {
                    continue;
                }

                if (!tilesToCaptureForPawns.Contains(closestUncapturedTile))
                {
                    Group group = new Group(new List<Pawn> { pawn }, GetBalancedNumOfPawnsToSendCapturing(closestUncapturedTile), closestUncapturedTile.transform.position);

                    tilesToCaptureForPawns.Add(closestUncapturedTile);
                    formedPawnGroups.Add(group);
                } 
                else
                {
                    Group group = formedPawnGroups[tilesToCaptureForPawns.FindIndex(tile => tile == closestUncapturedTile)];

                    if (group.IsGroupFull())
                    {
                        excludeTilePositions.Add(group.Destination);
                        goto RepeatIteration;
                    }
                    else
                    {
                        group.Pawns.Add(pawn);
                    }
            }
        }

        for (int i = 0; i < formedPawnGroups.Count; i++)
        {
            PawnTask.LineUpPawnsOnTarget(formedPawnGroups[i].Pawns, tilesToCaptureForPawns[i].transform.position);
        }

        formedPawnGroups.Clear();
        tilesToCaptureForPawns.Clear();
    }

    private int GetBalancedNumOfPawnsToSendCapturing(TileBase tileToCapture)
    {
        int minNumOfPawnsToSend = 1;

        // Is it last tile to capture
        if (_team.CapturedTiles.Count + 1 == GameManager.Instance.Tiles.Count)
        {
            return _team.PawnsInTeam.Count;
        }
        else
        {
            int balancedNumOfPawnsToSendCapturing = Mathf.CeilToInt((GetScoreOfEnemyPawnsInTile(tileToCapture) / _scorePerPawn));
            return balancedNumOfPawnsToSendCapturing > 0 ? balancedNumOfPawnsToSendCapturing : minNumOfPawnsToSend;
        }
    }

    private float GetScoreOfEnemyPawnsInTile(TileBase tileToCapture)
    {
        float enemyPawnsTotalScore = 0;

        foreach (Pawn pawn in tileToCapture.GetPawns())
        {
            if (pawn.Team != _team)
            {
                enemyPawnsTotalScore += pawn.Health + pawn.Damage;
            }
        }

        return enemyPawnsTotalScore;
    }

    private TileBase GetClosestUncapturedTilePos(Vector3 startPos, List<Vector3> excludePos)
    {
        TileBase closestTile = new TileBase();
        float closestDistance = float.MaxValue;

        foreach (TileBase tile in GameManager.Instance.Tiles)
        {
            if (!_team.CapturedTiles.Contains(tile) && !excludePos.Contains(tile.transform.position))
            {
                Vector3 tilePos = tile.transform.position;
                float distance = Vector3.Distance(startPos, tilePos);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTile = tile;
                }
            }
        }

        return closestTile;
    }
}