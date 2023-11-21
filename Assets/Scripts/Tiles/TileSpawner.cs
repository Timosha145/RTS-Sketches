using PoplarLib;
using UnityEngine;


public class TileSpawner : ExtendedMonoBehaviour
{
    [field: SerializeField] public Team Team { get; private set; }

    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _gatheringPoint;
    [SerializeField] private Pawn _pawnPrefab;

    private float _timerToSpawn;

    private void Update()
    {
        HandleSpawning();
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
