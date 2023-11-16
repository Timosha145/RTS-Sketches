using UnityEngine;

public class Team : MonoBehaviour
{
    [field: SerializeField] public Color Color { get; private set; }
    [field: SerializeField] public Material Material { get; private set; }
    [field: SerializeField] public float Health { get; private set; }
    [field: SerializeField] public float Damage { get; private set; }
    [field: SerializeField] public float MaxPawns { get; private set; }
    [field: SerializeField] public float SpawnRateTime { get; private set; }

    [SerializeField] private int _maxPawnsBonusModifier = 1;
    [SerializeField] private float _minPawns;

    public float CurrentMaxPawns { get; private set; }
    public int CurrentQuantityOfPawns { get; private set; }

    private void Awake()
    {
        CurrentMaxPawns = _minPawns;
        CurrentQuantityOfPawns = 0;
        Material.color = Color;
    }

    public void IncreaseMaxPawns()
    {
        CurrentMaxPawns = Mathf.Clamp(CurrentQuantityOfPawns + _maxPawnsBonusModifier, _minPawns, MaxPawns);
    }

    public void DescreaseMaxPawns()
    {
        CurrentMaxPawns = Mathf.Clamp(CurrentQuantityOfPawns - _maxPawnsBonusModifier, _minPawns, MaxPawns);
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
