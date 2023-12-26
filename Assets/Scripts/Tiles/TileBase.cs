using PoplarLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TileBase : ExtendedMonoBehaviour
{
    [SerializeField] private bool ISTEST = false;
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
            Pawn.OnDestroyed += Pawn_OnDestroyed;
            Pawn.OnStartedSitting += Pawn_OnStartedSitting;
        }
    }

    private void Pawn_OnStartedSitting(object sender, EventArgs e)
    {
        Pawn pawn = sender as Pawn;

        // If capturing progress is zero assign capturing team to the tile
        if (_captureProgress <= 0 && _capturingTeam != pawn.Team)
        {
            ChangeCapturingTeam(pawn.Team);
        }
    }

    private void Pawn_OnDestroyed(object sender, EventArgs e)
    {
        Pawn pawn = sender as Pawn;

        pawn.OnDestroyed -= Pawn_OnDestroyed;
        pawn.OnStartedSitting -= Pawn_OnStartedSitting;
        _pawnsInTile.Remove(pawn);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Pawn pawn))
        {
            RemovePawn(pawn);
        }
    }

    public bool IsCaptured()
    {
        return _capturedByTeam != null;
    }

    public List<Pawn> GetPawns()
    {
        return _pawnsInTile;
    }

    protected void HandleCaptureSpeedModifier()
    {
        int bonus = _pawnsInTile.Count == 0 || !AreAllPawnsOfTeam(_capturingTeam) ? 1 : _pawnsInTile.Count(pawn => pawn.IsSitting());
        _captureSpeed = Mathf.Clamp(bonus, 0, _maxPawnBonusModifier);
    }

    protected void HandleCapturing()
    {
        if (_captureProgress <= 0 && _pawnsInTile.Count > 0 && _capturingTeam != _pawnsInTile[0].Team)
        {
            ChangeCapturingTeam(_pawnsInTile[0].Team);
        }

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
    }

    private void RemovePawn(Pawn pawn)
    {
        pawn.OnDestroyed -= Pawn_OnDestroyed;
        pawn.OnStartedSitting -= Pawn_OnStartedSitting;
        _pawnsInTile.Remove(pawn);

        _captureSpeed = _pawnsInTile.Count == 0 ? 1 : _captureSpeed;
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
        return _pawnsInTile.Count > 0 && AreAllPawnsOfSameTeam() && _capturedByTeam != _capturingTeam && AreAllPawnsOfTeam(_capturingTeam);
    }

    private bool ShouldUncapture()
    {
        return _timerToCapture > 0 && AreAllPawnsOfSameTeam() && (AreAllPawnsOfTeam(_capturedByTeam) 
            || !AreAllPawnsOfTeam(_capturingTeam) || _pawnsInTile.Count == 0);
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

    private bool AreAllPawnsOfTeam(Team team)
    {
        return _pawnsInTile.All(pawn => pawn.Team == team);
    }

    private bool AreAllPawnsOfSameTeam()
    {
        return _pawnsInTile.Count > 0 ? _pawnsInTile.All(pawn => pawn.Team == _pawnsInTile[0].Team) : true;
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
