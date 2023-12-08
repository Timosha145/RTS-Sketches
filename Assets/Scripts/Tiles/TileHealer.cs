using UnityEngine;
public class TileHealer : TileBase
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
            foreach (Pawn Pawn in _pawnsInTile)
            {
                if (Pawn.Health < Pawn.MaxHealth && Pawn.Team == _capturedByTeam && Pawn.IsSitting())
                {
                    Pawn.ChangeHealth(_healthModifier);
                }
            }
        }
    }
}