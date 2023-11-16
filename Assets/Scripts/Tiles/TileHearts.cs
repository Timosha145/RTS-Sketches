using UnityEngine;
public class TileHearts : TileBase
{
    [SerializeField] private float _healthModifier;

    private float _timerToHeal, _timerToHealMax = 1f;

    private void Update()
    {
        HandleCapturing();
        HandleCaptureSpeedModifier();
        HandleHealing();
    }

    private void HandleHealing()
    {
        if (_capturedByTeam != _capturingTeam)
        {
            return;
        }

        if (HandleTimer(ref _timerToHeal, _timerToHealMax))
        {
            foreach (PawnAI PawnAI in _pawnsInZone)
            {
                if (PawnAI.Health < PawnAI.MaxHealth && PawnAI.Team == _capturedByTeam)
                {
                    PawnAI.ChangeHealth(_healthModifier);
                }
            }
        }
    }
}
