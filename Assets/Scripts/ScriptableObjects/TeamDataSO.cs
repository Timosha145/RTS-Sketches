using UnityEngine;

[CreateAssetMenu(fileName = "NewTeamData", menuName = "Scriptable Objects/TeamData")]
public class TeamDataSO : ScriptableObject
{
    [field: SerializeField] public string Name { get; private set; }
    [field: Space]
    [field: SerializeField] public Color TeamColor { get; private set; }
    [field: SerializeField] public float Health { get; private set; }
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float SpawnRateTime { get; private set; }
    [field: SerializeField] public int CaptureBonusModifier { get; private set; }
    [field: SerializeField] public int MaxPawns { get; private set; }
    [field: SerializeField] public int MinPawns { get; private set; }
    [field: SerializeField] public Material DefaultMaterial { get; private set; }
    [field: SerializeField] public Pawn PawnPrefab { get; private set; }
}