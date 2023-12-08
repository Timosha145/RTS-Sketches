using PoplarLib;
using System;
using System.Collections.Generic;
using UnityEngine;

public class TileBase : ExtendedMonoBehaviour
{
    [SerializeField] private float _timeToCapture = 3f;
    [SerializeField] private int _maxPawnBonusModifier;

    public event EventHandler<CaptureProgressEventArgs> OnChangeCaptureProgress;
    public event EventHandler<CapturedEventArgs> OnCaptured;
    public event EventHandler<CapturedEventArgs> OnCapturingTeamChanged;

    public class CaptureProgressEventArgs : EventArgs
    {
        public float Progress;
    }

    public class CapturedEventArgs : EventArgs
    {
        public Team Team;
    }

    protected List<Pawn> _pawnsInTile = new List<Pawn>();
    protected Team _capturedByTeam;
    protected Team _capturingTeam;

    private int _captureSpeed;
    private float _timerToCapture = 0f, _timerToCaptureMax;
    private float _captureProgress = 0f;

    private void Awake()
    {
        _timerToCaptureMax = _timeToCapture;
    }

    protected void Start()
    {
        GameManager.Instance.InitTile(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Pawn Pawn))
        {
            _pawnsInTile.Add(Pawn);

            // If capturing progress is zero assign capturing team to the tile
            if (_captureProgress == 0)
            {
                _capturingTeam = Pawn.Team;
                ChangeCapturingTeam(Pawn.Team);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Pawn Pawn))
        {
            _pawnsInTile.Remove(Pawn);

            if (_pawnsInTile.Count == 0)
            {
                _captureSpeed = 1;
            }
        }
    }

    public List<Pawn> GetPawns()
    {
        return _pawnsInTile;
    }

    protected void HandleCaptureSpeedModifier()
    {
        int bonus = 0;

        foreach (Pawn Pawn in _pawnsInTile)
        {
            if (Pawn.IsSitting())
            {
                bonus++;
            }
        }

        bonus = _pawnsInTile.Count == 0 || !CheckIfIsPawnsTeam(_capturingTeam) ? 1 : bonus;

        _captureSpeed = Mathf.Clamp(bonus, 0, _maxPawnBonusModifier);
    }

    protected void HandleCapturing()
    {
        if (ShouldCapture())
        {
            ChangeCapturingProgress(true);

            if (_captureProgress >= 1)
            {
                Captured();
            }
        }
        else if (ShouldUncapture())
        {
            ChangeCapturingProgress(false);
        }

        if (_captureProgress <= 0 && _pawnsInTile.Count > 0)
        {
            ChangeCapturingTeam(_pawnsInTile[0].Team);
        }
    }

    private void ChangeCapturingProgress(bool isPositiveValue)
    {
        int multiplier = isPositiveValue ? 1 : -1;

        _timerToCapture += Time.deltaTime * _captureSpeed * multiplier;
        _captureProgress = _timerToCapture / _timerToCaptureMax;

        OnChangeCaptureProgress?.Invoke(this, new CaptureProgressEventArgs()
        {
            Progress = _captureProgress
        });
    }

    private bool ShouldCapture()
    {
        return CheckPawnsSameTeam() && _capturedByTeam != _capturingTeam && CheckIfIsPawnsTeam(_capturingTeam) && _pawnsInTile.Count > 0;
    }

    private bool ShouldUncapture()
    {
        return _timerToCapture > 0 && CheckPawnsSameTeam() && (CheckIfIsPawnsTeam(_capturedByTeam) 
            || !CheckIfIsPawnsTeam(_capturingTeam) || _pawnsInTile.Count == 0);
    }

    private void Captured()
    {
        if (_capturedByTeam != null)
        {
            _capturedByTeam.RemoveTile(this);
        }

        _capturedByTeam = _capturingTeam;
        _timerToCapture = 0f;
        _captureProgress = 0f;

        OnChangeCaptureProgress?.Invoke(this, new CaptureProgressEventArgs()
        {
            Progress = _captureProgress
        });

        OnCaptured?.Invoke(this, new CapturedEventArgs()
        {
            Team = _capturedByTeam
        });

        _capturedByTeam.AddTile(this);
    }

    private bool CheckPawnsSameTeam()
    {
        // If each Pawn in the list is from the same team as the first Pawn in the list return true
        foreach (Pawn Pawn in _pawnsInTile)
        {
            if (_pawnsInTile[0].Team != Pawn.Team)
            {
                return false;
            }
        }

        return true;
    }

    private bool CheckIfIsPawnsTeam(Team team)
    {
        // If capturing team is same as each Pawn's team return true
        foreach (Pawn Pawn in _pawnsInTile)
        {
            if (Pawn.Team != team)
            {
                return false;
            }
        }

        return true;
    }

    private void ChangeCapturingTeam(Team team)
    {
        _capturingTeam = team;

        OnCapturingTeamChanged?.Invoke(this, new CapturedEventArgs()
        {
            Team = _capturingTeam
        });
    }
}
