public class TileIncreaser : TileBase
{
    private new void Start()
    {
        base.Start();
        OnCaptured += TilePeaks_OnCaptured;
    }

    private void TilePeaks_OnCaptured(object sender, CapturedEventArgs e)
    {
        if (_capturedByTeam != null)
        {
            _capturedByTeam.DescreaseMaxPawns();
        }

        e.Team.IncreaseMaxPawns();
    }

    private void Update()
    {
        HandleCapturing();
        HandleCaptureSpeedModifier();
    }
}
