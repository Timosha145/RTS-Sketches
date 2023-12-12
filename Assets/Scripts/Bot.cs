using PoplarLib;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[RequireComponent(typeof(Team))]
public class Bot : ExtendedMonoBehaviour
{
    private float _timerToMakeMove, _timerToMakeMoveMax = 5f;
    private float _timerToClearData, _timerToClearDataMax = 30f;
    private float _timerToGameLost, _timerToGameLostMax = 15f;
    private float _orderPosThreshold = 1f;
    private float _scorePerPawn;
    private Team _team;
    private List<Vector3> _excludedTilePositions = new List<Vector3>();
    private List<Vector3> _attackedTilePositions = new List<Vector3>();
    private List<Pawn> _ignorePawns;

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

            return fullnessOfGroup >= chance;
        }
    }

    private void Awake()
    {
        _ignorePawns = new List<Pawn>();
    }

    private void Start()
    {
        _team = GetComponent<Team>();
        _scorePerPawn = _team.TeamDataSO.Health + _team.TeamDataSO.Damage;

        _team.OnTileBeingAttacked += Team_OnTileBeingAttacked;
    }

    private void Team_OnTileBeingAttacked(object sender, Team.OnTileBeingAttakcedEventArgs e)
    {
        TileBase tile = e.Tile;
        _attackedTilePositions.Add(tile.transform.position);
    }

    private void Update()
    {
        if (HandleTimer(ref _timerToMakeMove, _timerToMakeMoveMax))
        {
            HandleMoves();
        }

        if (HandleTimer(ref _timerToClearData, _timerToClearDataMax))
        {
            _excludedTilePositions.Clear();
            _attackedTilePositions.Clear();
        }

        HandleGameLost();
    }

    private bool IsChanceToUseUnhealedPawn(int chance = 15)
    {
        return chance > Random.Range(0, 100);
    }

    private void HandleGameLost()
    {
        if (GameManager.Instance.AreAllTilesCaptured() && _team.CapturedTiles.Count == 0 && HandleTimer(ref _timerToGameLost, _timerToGameLostMax))
        {
            Destroy(this);
            Debug.Log($"Team {_team.name} lost!");
        }
        else
        {
            _timerToGameLost = 0;
        }
    }

    private void HandleMoves()
    {
        List<Pawn> healthyPawns = new List<Pawn>();
        List<Pawn> weakPawns = new List<Pawn>();

        FilterPawnsByHealth(GetNonIngnorePawnsList(_team.PawnsInTeam), ref weakPawns, ref healthyPawns);

        if (TrySendPawnsToHeal(weakPawns))
        {
            SendPawnsToCapture(healthyPawns);
        }
        else
        {
            SendPawnsToCapture(_team.PawnsInTeam);
        }
    }

    private void SendPawnsToCapture(List<Pawn> pawns)
    {
        if (_team.CapturedTiles.Count == GameManager.Instance.Tiles.Count)
        {
            return;
        }

        List<TileBase> tilesToCaptureForPawns = new List<TileBase>();
        List<Group> formedPawnGroups = new List<Group>();

        foreach (Pawn pawn in pawns)
        {
            if (IsDestinationNearToAnyExcludedPos(pawn.GetDestination()))
            {
                continue;
            }

            TileBase closestUncapturedTile = GetClosestUncapturedTile(pawn.transform.position, _excludedTilePositions);

            if (IsPositionCloseEnough(pawn.GetDestination(), closestUncapturedTile.transform.position, _orderPosThreshold))
            {
                continue;
            }

            if (!tilesToCaptureForPawns.Contains(closestUncapturedTile))
            {
                Group group = new Group(new List<Pawn> { pawn }, GetBalancedNumOfPawnsToSendCapturing(closestUncapturedTile), closestUncapturedTile.transform.position);
                
                if (group.IsGroupFull())
                {
                    _excludedTilePositions.Add(group.Destination);
                }

                tilesToCaptureForPawns.Add(closestUncapturedTile);
                formedPawnGroups.Add(group);
            } 
            else if (IsAnyGroupNotFull(formedPawnGroups))
            {
                Group group = formedPawnGroups[tilesToCaptureForPawns.FindIndex(tile => tile == closestUncapturedTile)];
                group.Pawns.Add(pawn);
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

    private List<Pawn> GetNonIngnorePawnsList(List<Pawn> pawns)
    {
        pawns.RemoveAll(item => _ignorePawns.Contains(item));
        return pawns;
    }

    private bool IsAnyGroupNotFull(List<Group> groups)
    {
        return groups.Any(group => !group.IsGroupFull());
    }

    private void FilterPawnsByHealth(List<Pawn> pawns, ref List<Pawn> weakPawns, ref List<Pawn> healthyPawns)
    {
        float chanceToSendWeakPawn = 0.15f;
        float dangerousHealthInPersentege = 0.3f;

        foreach (Pawn pawn in pawns)
        {
            List<Pawn> targetList = (pawn.Health / pawn.MaxHealth < dangerousHealthInPersentege && Random.Range(0, 100) / 100 < chanceToSendWeakPawn)
                ? weakPawns
                : healthyPawns;

            targetList.Add(pawn);
        }
    }

    private bool TrySendPawnsToHeal(List<Pawn> pawns)
    {
        Vector3 tileHealerPos = new Vector3();
        bool hasFoundAnyCapturedHealerTile = false;

        foreach (Pawn pawn in pawns)
        {
            if (TryGetClosestTileHealerPos(ref tileHealerPos, pawn.transform.position))
            {
                pawn.OrderToMove(tileHealerPos);
                _ignorePawns.Add(pawn);
                pawn.OnHealthChanged += Pawn_OnHealthChanged;
                hasFoundAnyCapturedHealerTile = true;
            }
        }

        return hasFoundAnyCapturedHealerTile;
    }

    private void Pawn_OnHealthChanged(object sender, Pawn.OnHealthChangedEventArgs e)
    {
        Pawn pawn = sender as Pawn;

        if (pawn.Health == pawn.MaxHealth || IsChanceToUseUnhealedPawn())
        {
            _ignorePawns.Remove(pawn);
            pawn.OnHealthChanged -= Pawn_OnHealthChanged;
        }
    }

    private bool IsDestinationNearToAnyExcludedPos(Vector3 pos)
    {
        return _excludedTilePositions.Any(tilePos => IsPositionCloseEnough(pos, tilePos, _orderPosThreshold));
    }


    private bool TryGetClosestTileHealerPos(ref Vector3 tileHealerPos, Vector3 startPos)
    {
        bool hasFoundAnyCapturedHealerTile = false;
        float closestDistance = float.MaxValue;
        List<TileBase> capturedTiles = _team.CapturedTiles;

        foreach (TileBase tile in capturedTiles)
        {
            Vector3 tilePos = tile.transform.position;
            float distance = Vector3.Distance(startPos, tilePos);

            if (distance < closestDistance && tile is TileHealer)
            {
                closestDistance = distance;
                tileHealerPos = tile.transform.position;
                hasFoundAnyCapturedHealerTile = true;
            }
        }
        
        return hasFoundAnyCapturedHealerTile;
    }

    private int GetBalancedNumOfPawnsToSendCapturing(TileBase tileToCapture)
    {
        int minNumOfPawnsToSend = 1;

        // Is it last tile to capture
        if (_team.CapturedTiles.Count + 1 == GameManager.Instance.Tiles.Count)
        {
            return _team.PawnsInTeam.Count;
        }
        
        int balancedNumOfPawnsToSendCapturing = Mathf.CeilToInt((GetScoreOfEnemyPawnsInTile(tileToCapture) / _scorePerPawn));
        return balancedNumOfPawnsToSendCapturing > 0 ? balancedNumOfPawnsToSendCapturing : minNumOfPawnsToSend;
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
            if ((!_team.CapturedTiles.Contains(tile) && !excludePos.Contains(tile.transform.position)) || _attackedTilePositions.Contains(tile.transform.position))
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