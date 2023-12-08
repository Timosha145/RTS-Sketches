using PoplarLib;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Team))]
public class Bot : ExtendedMonoBehaviour
{
    private float _timerToMakeMove, _timerToMakeMoveMax = 5f;
    private float _timerToClearData, _timerToClearDataMax = 30f;
    private float _orderPosThreshold = 1f;
    private float _scorePerPawn;
    private Team _team;
    private List<Vector3> _excludedTilePositions = new List<Vector3>();

    private class Group
    {
        public List<Pawn> Pawns { get; }
        public int MaxPawns { get; }
        public Vector3 Destination { get; }

        public Group(List<Pawn> pawns, int maxPawns, Vector3 destination)
        {
            Pawns = pawns;
            MaxPawns = maxPawns < 1 ? 1 : maxPawns;
            Destination = destination;
        }

        public bool IsGroupFull()
        {
            return Pawns.Count >= MaxPawns;
        }

        // The smaller group comparing with it's max size, the lower chance of going to capture tile is
        public bool ShouldGoCapturing()
        {
            float chance = Random.Range(0, 100) * 0.01f;
            float fullnessOfGroup = Pawns.Count / (float)MaxPawns;

            Debug.Log($"Chances: fullness: {fullnessOfGroup} >= {chance}");
            if (fullnessOfGroup >= chance)
            {
                return true;
            }
            else
            {
                return false;
            }
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

        if (HandleTimer(ref _timerToClearData, _timerToClearDataMax))
        {
            _excludedTilePositions.Clear();
        }
    }

    private void SendPawnsToCapture()
    {
        if (_team.CapturedTiles.Count == GameManager.Instance.Tiles.Count)
        {
            return;
        }

        List<TileBase> tilesToCaptureForPawns = new List<TileBase>();
        List<Group> formedPawnGroups = new List<Group>();
        List<Pawn> availablePawns = _team.PawnsInTeam;

        foreach (Pawn pawn in availablePawns)
        {
            if (IsDestinationNearToAnyExcludedPos(pawn.GetDestination()))
            {
                continue;
            }

            RepeatIteration:
                TileBase closestUncapturedTile = GetClosestUncapturedTile(pawn.transform.position, _excludedTilePositions);

                if (IsPositionCloseEnough(pawn.GetDestination(), closestUncapturedTile.transform.position, _orderPosThreshold))
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
                        _excludedTilePositions.Add(group.Destination);
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
            if (formedPawnGroups[i].ShouldGoCapturing())
            {
                PawnTask.LineUpPawnsOnTarget(formedPawnGroups[i].Pawns, tilesToCaptureForPawns[i].transform.position);
            }
        }
    }

    private bool IsDestinationNearToAnyExcludedPos(Vector3 pos)
    {
        foreach (Vector3 tilePos in _excludedTilePositions)
        {
            if (IsPositionCloseEnough(pos, tilePos, _orderPosThreshold))
            {
                return true;
            }
        }

        return false;
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

    private TileBase GetClosestUncapturedTile(Vector3 startPos, List<Vector3> excludePos)
    {
        TileBase closestTile = GameManager.Instance.Tiles[0];
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