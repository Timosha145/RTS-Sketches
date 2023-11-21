using UnityEngine;
using UnityEngine.UI;

public class TileSpawnerVisual : MonoBehaviour
{
    [SerializeField] private TileSpawner _tile;
    [SerializeField] private Image _background;
    [SerializeField] private Image _foreground;

    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        _foreground.color = _tile.Team.TeamDataSO.TeamColor;
    }
}
