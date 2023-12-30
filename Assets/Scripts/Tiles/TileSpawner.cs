using PoplarLib;
using UnityEngine;
using System;

public class TileSpawner : ExtendedMonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _gatheringPoint;

    public Team Team { get; private set; }

    private Pawn _pawnPrefab;
    private float _timerToSpawn;
    private bool _initialized = false;

    private void Update()
    {
        if (_initialized)
        {
            HandleSpawning();
        }
    }

    public void Init(Team team)
    {
        if (_initialized)
        {
            Debug.LogError($"{this} is being tried to be initialized again!");
            return;
        }

        Team = team;
        _initialized = true;
        _pawnPrefab = team.TeamDataSO.PawnPrefab;
    }

    private void HandleSpawning()
    {
        if (Team.CurrentQuantityOfPawns < Team.CurrentMaxPawns)
        {
            if (HandleTimer(ref _timerToSpawn, Team.TeamDataSO.SpawnRateTime))
            {
                Pawn spawnedPawn = Instantiate(_pawnPrefab, _spawnPoint.position, Quaternion.identity);
                spawnedPawn.SetPropertiesOnce(Team, _gatheringPoint.position);
            }
        }
        else
        {
            _timerToSpawn = 0;
        }
    }
}
