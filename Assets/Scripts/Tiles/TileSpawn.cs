using UnityEngine;
using PoplarLib;


public class TileSpawn : ExtendedMonoBehaviour
{
    [field: SerializeField] public Team Team { get; private set; }

    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _gatheringPoint;
    [SerializeField] private PawnAI _pawnPrefab;

    private float _timerToSpawn;

    private void Update()
    {
        HandleSpawning();
    }

    private void HandleSpawning()
    {
        if (Team.CurrentQuantityOfPawns < Team.CurrentMaxPawns)
        {
            if (HandleTimer(ref _timerToSpawn, Team.SpawnRateTime))
            {
                PawnAI spawnedPawn = Instantiate(_pawnPrefab, _spawnPoint.position, Quaternion.identity);
                spawnedPawn.SetPropertiesOnce(Team, _gatheringPoint.position);
            }
        }
        else
        {
            _timerToSpawn = 0;
        }
    }
}
