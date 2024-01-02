using PoplarLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Team))]
public class Bot : ExtendedMonoBehaviour
{
    [field: SerializeField] public string Name { get; private set; }

    private float _timerToMakeMove, _timerToMakeMoveMax = 5f;
    private float _timerToClearData, _timerToClearDataMax = 30f;
    private float _timerToGameLost, _timerToGameLostMax = 30f;
    private float _timerToCheckTileActivity, _timerToCheckTileActivityMax = 10f;
    private float _timerToChangeBotType, _timerToChangeBotTypeMax = 60f;
    private float _orderPosThreshold = 1f;
    private float _scorePerPawn;
    private float _riskChanceModifier;
    private float _safeChanceModifier;
    private float _chanceToChangeBotTypeDuringGame;
    private Team _team;
    private List<Vector3> _excludedTilePositions = new List<Vector3>();
    private List<TileBase> _attackedTiles = new List<TileBase>();
    private List<Pawn> _ignorePawns;
    private List<Pawn> _patrollingPawns;
    private List<TileBase> _tilesInPriority;

    private void Awake()
    {
        _ignorePawns = new List<Pawn>();
        _patrollingPawns = new List<Pawn>();
        _chanceToChangeBotTypeDuringGame = Random.Range(0, 10);
        SetBotType();
    }

    private void Start()
    {
        _team = GetComponent<Team>();
        SetPriorityTiles();

        _scorePerPawn = _team.TeamDataSO.Health + _team.TeamDataSO.Damage;

        _team.OnTileBeingAttacked += Team_OnTileBeingAttacked;
    }

    private void Update()
    {
        if (HandleTimer(ref _timerToMakeMove, _timerToMakeMoveMax))
        {
            HandleMoves();
        }

        if (HandleTimer(ref _timerToClearData, _timerToClearDataMax))
        {
            HandleCacheClearing();
        }

        if (HandleTimer(ref _timerToCheckTileActivity, _timerToCheckTileActivityMax))
        {
            HandlePatrolling();
        }

        if (HandleTimer(ref _timerToChangeBotType, _timerToChangeBotTypeMax))
        {
            if (EvaluateChance(_chanceToChangeBotTypeDuringGame))
            {
                SetBotType();
            }
        }

        HandleGameLost();
    }

    private void Team_OnTileBeingAttacked(object sender, Team.OnTileBeingAttakcedEventArgs e)
    {
        TileBase tile = e.Tile;
        _attackedTiles.Add(tile);

        DefenseTile();
    }

    private void SetBotType()
    {
        BotType[] botTypes = (BotType[])Enum.GetValues(typeof(BotType));
        BotType botType = botTypes[Random.Range(0, botTypes.Length)];
        Name = $"Vasya {botType}";

        switch (botType)
        {
            case BotType.Aggressive:
                _riskChanceModifier = 1.5f;
                _safeChanceModifier = 0.6f;
                break;
            case BotType.Careful:
                _riskChanceModifier = 0.5f;
                _safeChanceModifier = 1.6f;
                break;
            case BotType.Classic:
                _riskChanceModifier = 1f;
                _safeChanceModifier = 1f;
                break;
            default:
                Debug.LogError("Unexpected BotType: " + botType);
                break;
        }

        _timerToClearDataMax *= _safeChanceModifier;
        _timerToMakeMoveMax *= _safeChanceModifier;
    }

    private void HandleGameLost()
    {
        if (GameManager.Instance.AreAllTilesCaptured() && _team.CapturedTiles.Count == 0 && HandleTimer(ref _timerToGameLost, _timerToGameLostMax))
        {
            Destroy(this);
            Debug.Log($"Team {_team.name} lost!");
        }
    }

    private void HandleCacheClearing()
    {
        float chanceToFreePatrollingPawn = 35 * _riskChanceModifier;
        float chanceToFreeIgnoredPawn = 15 * _riskChanceModifier;

        RemoveSpicificPawnsFromIgnored(GetNonCapturingPawns(_ignorePawns));
        _excludedTilePositions.Clear();
        _attackedTiles.Clear();

        ClearPawnListWithChance(_patrollingPawns, chanceToFreePatrollingPawn);
        ClearPawnListWithChance(_ignorePawns, chanceToFreeIgnoredPawn);
    }

    private void RemoveSpicificPawnsFromIgnored(List<Pawn> pawns)
    {
        _ignorePawns.RemoveAll(pawn => GetNonCapturingPawns(pawns).Contains(pawn));
    }

    private void ClearPawnListWithChance(List<Pawn> pawns, float chanceForEachPawn)
    {
        for (int i = pawns.Count - 1; i >= 0; i--)
        {
            Pawn pawn = pawns[i];

            if (pawn == null || EvaluateChance(chanceForEachPawn))
            {
                pawns.RemoveAt(i);
            }
        }
    }

    private void HandleMoves()
    {
        List<Pawn> healthyPawns = new List<Pawn>();
        List<Pawn> weakPawns = new List<Pawn>();

        FilterPawnsByHealth(_team.PawnsInTeam, ref weakPawns, ref healthyPawns);

        if (TrySendPawnsToHeal(weakPawns))
        {
            SendPawnsToCapture(healthyPawns);
        }
        else
        {
            SendPawnsToCapture(GetNonIgnorePawns(_team.PawnsInTeam));
        }
    }

    private void SetPriorityTiles()
    {
        if (_tilesInPriority != null || _team.TileSpawner == null) return;

        Vector3 _spawnTilePos = _team.TileSpawner.transform.position;
        _tilesInPriority = GameManager.Instance.Tiles
            .OrderBy(tile => Vector3.Distance(tile.transform.position, _spawnTilePos))
            .ToList();
    }

    private void DefenseTile()
    {
        foreach (TileBase tile in _attackedTiles)
        {
            float chanceToDefendTile = GetTilePriorityInPercentege(tile) * _safeChanceModifier;

            if (!tile.IsAnyPawnOfTeam(_team) && EvaluateChance(chanceToDefendTile))
            {
                FormGroupAndSendToTile(tile, PawnOrder.CircleTarget, _ignorePawns);
            }
        }
    }

    private void HandlePatrolling()
    {
        int tileActivityModifier = 3;

        foreach (TileBase tile in _team.CapturedTiles)
        {
            float chanceToSendPatrolingByActivity = GetTilePriorityInPercentege(tile) + tile.Activity * tileActivityModifier;
            if (EvaluateChance(chanceToSendPatrolingByActivity) && tile.Activity > 0)
            {
                FormGroupAndSendToTile(tile, PawnOrder.LineUpOnTarget, _patrollingPawns);
            }
        }
    }

    private float GetTilePriorityInPercentege(TileBase tile)
    {
        if (_tilesInPriority.Count == 0)
        {
            Debug.LogError($"There no priority tiles for team: [{_team}]!");
        }

        return 100 - ((_tilesInPriority.IndexOf(tile) + 1) / _tilesInPriority.Count) * 100;
    }

    private void FormGroupAndSendToTile(TileBase tile, PawnOrder order, List<Pawn> addToList = null)
    {
        int closestPawnsThreshold = 12;
        int neededPawnCount = GetBalancedNumOfPawnsToSendCapturing(tile);
        List<Pawn> closestPawns = GetClosestPawns(tile.transform.position, neededPawnCount, closestPawnsThreshold);
        Group group = new Group(closestPawns, neededPawnCount, tile.transform.position);
        addToList?.AddRange(closestPawns);

        RemoveSpicificPawnsFromIgnored(closestPawns);

        if (group.ShouldGoCapturing(_riskChanceModifier))
        {
            group.Order(order);
        }
    }

    private List<Pawn> GetClosestPawns(Vector3 pos, int neededPawnCount, int threshold = int.MaxValue)
    {
        List<Pawn> closestPawns = _team.PawnsInTeam
            .OrderBy(pawn => Vector3.Distance(pawn.transform.position, pos))
            .Take(neededPawnCount)
            .Where(pawn => IsCloseEnough(pos, pawn.transform.position, threshold))
            .ToList();

        return closestPawns;
    }

    private TileBase[] GetClosestUncapturedTiles(Vector3 pos, int neededTileCount, List<Vector3> excludedTilePositions = null)
    {
        TileBase[] closestTiles = GameManager.Instance.Tiles
            .OrderBy(tile => Vector3.Distance(tile.transform.position, pos))
            .Where(tile => tile.GetCapturedByTeam() != _team && (excludedTilePositions == null
                || !excludedTilePositions.Contains(tile.transform.position)))
            .Take(neededTileCount)
            .ToArray();

        return closestTiles;
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
            int numOfTilesToChooseFrom = 3;
            TileBase[] closestUncapturedTiles = GetClosestUncapturedTiles(pawn.transform.position, numOfTilesToChooseFrom, _excludedTilePositions);
            TileBase tileToCapture = closestUncapturedTiles[Random.Range(0, closestUncapturedTiles.Length - 1)];

            if (IsCloseEnough(pawn.GetDestination(), tileToCapture.transform.position, _orderPosThreshold)
                || IsDestinationNearToAnyExcludedPos(pawn.GetDestination()))
            {
                continue;
            }

            if (!tilesToCaptureForPawns.Contains(tileToCapture) || _attackedTiles.Contains(tileToCapture))
            {
                Group group = new Group(new List<Pawn> { pawn }, GetBalancedNumOfPawnsToSendCapturing(tileToCapture),
                    tileToCapture.transform.position);
                
                if (group.IsGroupFull())
                {
                    _excludedTilePositions.Add(group.Destination);
                }

                tilesToCaptureForPawns.Add(tileToCapture);
                formedPawnGroups.Add(group);
            }
            else if (IsAnyGroupNotFull(formedPawnGroups))
            {
                Group group = formedPawnGroups[tilesToCaptureForPawns.FindIndex(tile => tile == tileToCapture)];
                group.Pawns.Add(pawn);
            }
        }

        for (int i = 0; i < formedPawnGroups.Count; i++)
        {
            if (formedPawnGroups[i].ShouldGoCapturing(_riskChanceModifier))
            {
                PawnTask.OrderPawns(PawnOrder.LineUpOnTarget, formedPawnGroups[i].Pawns, tilesToCaptureForPawns[i].transform.position);
                _ignorePawns.AddRange(formedPawnGroups[i].Pawns);
            }
        }
    }

    private List<Pawn> GetNonCapturingPawns(List<Pawn> pawns)
    {
        List<Pawn> capturingPawns = new List<Pawn>();

        foreach (Pawn pawn in pawns)
        {
            if (pawn == null)
            {
                continue;
            }

            TileBase closestTile = GetClosestUncapturedTiles(pawn.transform.position, 1)[0];
            if (!IsCloseEnough(closestTile.transform.position, pawn.GetDestination(), _orderPosThreshold)
                || closestTile.GetCapturedByTeam() == _team)
            {
                capturingPawns.Add(pawn);
            }
        }

        return capturingPawns;
    }

    private List<Pawn> GetNonIgnorePawns(List<Pawn> pawns)
    {
        return pawns.Where(pawn => !_ignorePawns.Contains(pawn) && !_patrollingPawns.Contains(pawn)).ToList();
    }

    private bool IsAnyGroupNotFull(List<Group> groups)
    {
        return groups.Any(group => !group.IsGroupFull());
    }

    private void FilterPawnsByHealth(List<Pawn> pawns, ref List<Pawn> weakPawns, ref List<Pawn> healthyPawns)
    {
        float chanceToSendWeakPawn = 15 * _riskChanceModifier;
        float dangerousHealthInPersentege = 0.3f;

        foreach (Pawn pawn in pawns)
        {
            List<Pawn> targetList = (pawn.Health / pawn.MaxHealth < dangerousHealthInPersentege && EvaluateChance(chanceToSendWeakPawn))
                ? GetNonIgnorePawns(weakPawns)
                : healthyPawns;

            targetList.Add(pawn);
        }
    }

    private bool TrySendPawnsToHeal(List<Pawn> pawns)
    {
        Vector3 tileHealerPos = new Vector3();
        bool hasFoundAnyCapturedHealerTile = false;
        float chanceNotToSendPawnHealing = 55 * _riskChanceModifier;

        foreach (Pawn pawn in pawns)
        {
            if (TryGetClosestTileHealerPos(ref tileHealerPos, pawn.transform.position) && !EvaluateChance(chanceNotToSendPawnHealing))
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

        if (pawn.Health == pawn.MaxHealth)
        {
            _ignorePawns.Remove(pawn);
            pawn.OnHealthChanged -= Pawn_OnHealthChanged;
        }
    }

    private bool IsDestinationNearToAnyExcludedPos(Vector3 pos)
    {
        return _excludedTilePositions.Any(tilePos => IsCloseEnough(pos, tilePos, _orderPosThreshold));
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
        float chanceToIncreaseMinPawns = 25 * _safeChanceModifier;
        int minNumOfPawnsToSend = EvaluateChance(chanceToIncreaseMinPawns) ? 2 : 1;

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

enum BotType
{
    Aggressive,
    Careful,
    Classic
}