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

    private void Start()
    {
        _scorePerPawn = _team.TeamDataSO.Health + _team.TeamDataSO.Damage;
        _team = GetComponent<Team>();
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

        List<Vector3> destinationsForPawns = new List<Vector3>();
        List<List<Pawn>> formedPawnGroups = new List<List<Pawn>>();
        List<Pawn> availablePawns = _team.PawnsInTeam;

        foreach (Pawn pawn in availablePawns)
        {
            Vector3 closestUncapturedTilePos = GetClosestUncapturedTilePos(pawn.transform.position);

            if (IsPositionCloseEnough(pawn.GetDestanation(), closestUncapturedTilePos, _orderPosThreshold))
            {
                continue;
            }

            if (!destinationsForPawns.Contains(closestUncapturedTilePos))
            {
                destinationsForPawns.Add(closestUncapturedTilePos);
                formedPawnGroups.Add(new List<Pawn> { pawn });
            } 
            else
            {
                List<Pawn> group = formedPawnGroups[destinationsForPawns.FindIndex(pos => pos == closestUncapturedTilePos)];


                //.Add(pawn);
            }
        }

        for (int i = 0; i < formedPawnGroups.Count; i++)
        {
            PawnTask.LineUpPawnsOnTarget(formedPawnGroups[i], destinationsForPawns[i]);
        }

        formedPawnGroups.Clear();
        destinationsForPawns.Clear();
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

    private Vector3 GetClosestUncapturedTilePos(Vector3 startPos)
    {
        Vector3 closestTilePos = Vector3.zero;
        float closestDistance = float.MaxValue;

        foreach (TileBase tile in GameManager.Instance.Tiles)
        {
            if (!_team.CapturedTiles.Contains(tile))
            {
                Vector3 tilePos = tile.transform.position;
                float distance = Vector3.Distance(startPos, tilePos);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTilePos = tilePos;
                }
            }
        }

        return closestTilePos;
    }
}
