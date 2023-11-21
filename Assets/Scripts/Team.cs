using UnityEngine;

public class Team : MonoBehaviour
{
    [SerializeField] public TeamDataSO TeamDataSO;

    public Material Material { get; private set; }
    public float CurrentMaxPawns { get; private set; }
    public int CurrentQuantityOfPawns { get; private set; }

    private void Awake()
    {
        CurrentMaxPawns = TeamDataSO.MinPawns;
        CurrentQuantityOfPawns = 0;

        Material = new Material(TeamDataSO.DefaultMaterial);
        Material.color = TeamDataSO.TeamColor;
    }

    public void IncreaseMaxPawns()
    {
        CurrentMaxPawns = Mathf.Clamp(CurrentQuantityOfPawns + TeamDataSO.CaptureBonusModifier, TeamDataSO.MinPawns, TeamDataSO.MaxPawns);
    }

    public void DescreaseMaxPawns()
    {
        CurrentMaxPawns = Mathf.Clamp(CurrentQuantityOfPawns - TeamDataSO.CaptureBonusModifier, TeamDataSO.MinPawns, TeamDataSO.MaxPawns);
    }

    public void IncreaseCurrentQuantityOfPawns()
    {
        CurrentQuantityOfPawns++;
    }

    public void DescreaseCurrentQuantityOfPawns()
    {
        CurrentQuantityOfPawns--;
    }
}