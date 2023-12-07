using UnityEngine;
using UnityEngine.UI;

public class TileVisual : MonoBehaviour
{
    [SerializeField] private TileBase _tile;
    [SerializeField] private Image _background;
    [SerializeField] private Image _foreground;

    private void Start()
    {
        _tile.OnChangeCaptureProgress += Tile_OnChangeCaptureProgress;
        _tile.OnCapturingTeamChanged += Tile_OnCapturingTeamChanged;
        _tile.OnCaptured += Tile_OnCaptured;
    }

    private void Tile_OnCaptured(object sender, TileBase.CapturedEventArgs e)
    {
        _background.color = e.Team.TeamDataSO.TeamColor;
    }

    private void Tile_OnChangeCaptureProgress(object sender, TileBase.CaptureProgressEventArgs e)
    {
        _foreground.fillAmount = e.Progress;
    }

    private void Tile_OnCapturingTeamChanged(object sender, TileBase.CapturedEventArgs e)
    {
        _foreground.color = e.Team.TeamDataSO.TeamColor;
    }
}
