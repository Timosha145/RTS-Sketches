using PoplarLib;
using System;
using UnityEngine;
using UnityEngine.AI;

public class PawnAI : ExtendedMonoBehaviour
{
    [SerializeField] private bool ISTEST;

    [field: SerializeField] public Team Team { get; private set; }
    [field: SerializeField] public float Offset { get; private set; } = 1f;

    [SerializeField] private float _rotationSpeed = 4f;
    [SerializeField] private float _distanceToSee = 2f;

    public event EventHandler<OnSelectedEventArgs> OnSelected;
    public event EventHandler OnDeselected;
    public event EventHandler<OnHealthChangedEventArgs> OnHealthChanged;
    public event EventHandler OnStartedMoving, OnEndedMoving;
    public event EventHandler OnStartedSitting, OnEndedSitting;
    public event EventHandler OnAttack;

    public class OnSelectedEventArgs : EventArgs
    {
        public Color Color;

        public OnSelectedEventArgs(Color color)
        {
            Color = color;
        }
    }

    public class OnHealthChangedEventArgs : EventArgs
    {
        public float Health;

        public OnHealthChangedEventArgs(float health)
        {
            Health = health;
        }
    }

    [HideInInspector] public Vector3 TargetPosition;

    public float Health { get; private set; }
    public float MaxHealth { get; private set; }

    protected State _currentState = State.Idle;
    protected NavMeshAgent _navMeshAgent;
    protected PawnAI _pawnToFollow;
    protected PawnAI _targetPawn;
    protected float _walkRadiusOnTarget = 3f;

    private float _damage;
    private float _timerToSit, _timerToSitMax = 5f;
    private float _lastMoveTime;
    private float _carvingTime = 0.5f;
    private float _carvingMoveThreshold = 0.01f;
    private float _distanceIgnoreThreshold = 1f;
    private float _timerToAttack, _timerToAttackMax = 2f;
    private float _distanceToAttack;

    private bool _propertiesAreSet = false;
    private bool _isAnimationPlaying;
    private Vector3 _targetPosition = Vector3.zero;
    private Vector3 _lastPosition;

    public enum State
    {
        Idle,
        Sitting,
        Moving,
        Attacking,
        PlayingAnimation,
        PrepairingToAttack
    }

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _lastPosition = transform.position;
        _distanceToAttack = Offset * 0.6f;
    }

    private void Start()
    {
        Setup();

        Team.IncreaseCurrentQuantityOfPawns();
        OnEndedSitting += Pawn_OnEndedSitting;
    }

    private void OnDestroy()
    {
        Team.DescreaseCurrentQuantityOfPawns();
        PawnSelections.Instance.PawnList.Remove(this);

        if (PawnSelections.Instance.SelectedEnemyPawn == this)
        {
            PawnSelections.Instance.DeselectEnemy();
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleSitting();
        HandleAttack();

        if (ISTEST)
        {
            Debug.Log("State " + _currentState);
        }

    }

    public void SetPropertiesOnce(Team team, Vector3 targetPos)
    {
        if (!_propertiesAreSet)
        {
            Team = team;
            MoveToTarget(targetPos);

            _propertiesAreSet = true;
        }
    }

    public void OnAttackTarget()
    {
        if (_targetPawn != null)
        {
            _targetPawn.ChangeHealth(-Math.Abs(_damage));
        }
    }

    public void OnAttackEnd()
    {
        if (_currentState == State.Attacking)
        {
            _currentState = State.Idle;
        }
    }

    public bool IsSitting()
    {
        return _currentState == State.Sitting;
    }

    public void Select()
    {
        OnSelected?.Invoke(this, new OnSelectedEventArgs(Color.white));
    }

    public void SelectAsEnemy()
    {
        OnSelected?.Invoke(this, new OnSelectedEventArgs(Color.red));
    }

    public void Deselect()
    {
        OnDeselected?.Invoke(this, EventArgs.Empty);
    }

    public void StopFollowing()
    {
        _pawnToFollow = null;
    }

    public void FollowEnemy(PawnAI enemyPawn)
    {
        _pawnToFollow = enemyPawn;
    }

    public void ChangeHealth(float value)
    {
        Health += Mathf.Clamp(value, -Health, MaxHealth - Health);
        OnHealthChanged?.Invoke(this, new OnHealthChangedEventArgs(Health));

        if (Health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void MoveToTarget(Vector3 targetPos)
    {
        if (_currentState == State.Sitting)
        {
            OnEndedSitting?.Invoke(this, EventArgs.Empty);
        }

        _targetPosition = targetPos;
    }

    public void AnimationIsPlaying()
    {
        _isAnimationPlaying = true;
    }

    public void AnimationIsNotPlaying()
    {
        _isAnimationPlaying = false;
        OnAttackEnd();
    }

    // If PawnAI's current State is Idle, it's State will change to Sitting after certain amount of seconds
    protected void HandleSitting()
    {
        if (_currentState == State.Idle && HandleTimer(ref _timerToSit, _timerToSitMax))
        {
            _currentState = State.Sitting;
            OnStartedSitting?.Invoke(this, EventArgs.Empty);
        }
        else if (_currentState != State.Idle)
        {
            _timerToSit = 0f;
        }
    }

    protected void HandleAttack()
    {
        // Check IF destination of PawnAI is not too far to start fighting AND distance to the closest enemy PawnAI is minimum of given value
        if (IsPositionCloseEnough(_navMeshAgent.destination, transform.position, _distanceIgnoreThreshold) && TryGetClosestEnemy(_distanceToSee, out _targetPawn))
        {
            if (Vector3.Distance(_targetPawn.transform.position, transform.position) < _distanceToAttack)
            {
                //_currentState = State.PrepairingToAttack;

                ResetDestination();
                SlerpRotateTowards(_targetPawn.transform.position, _rotationSpeed);

                if (_currentState != State.Attacking && HandleTimer(ref _timerToAttack, _timerToAttackMax))
                {
                    _currentState = State.Attacking;
                    OnAttack?.Invoke(this, EventArgs.Empty);

                    _pawnToFollow = null;
                }
            }
            else
            {
                // Reset the timer if PawnAI is gone from the attack distance
                MoveToTarget(_targetPawn.transform.position);
                _timerToAttack = 0f;
            }
        }
        else
        {
            _targetPawn = null;
        }
    }

    // Checks during moving
    protected void HandleMovement()
    {

        // If target position was set start moving
        if (_targetPosition != Vector3.zero && !_isAnimationPlaying)
        {
            StartedMoving();
        }

        // If PawnAI has moved
        if (ShouldMove() && !_isAnimationPlaying)
        {
            _currentState = State.Moving;
            _lastMoveTime = Time.time;
            _lastPosition = transform.position;

            if (ShouldFollowEnemy())
            {
                MoveToTarget(_pawnToFollow.transform.position);
                TargetPosition = _pawnToFollow.transform.position;
            }
            else if (ShouldReturn())
            {
                MoveToTarget(TargetPosition);
            }
        }

        // If PawnAI's path is blocked or PawnAI has reached destination
        if (ShouldStop())
        {
            if (ISTEST)
            {
                Debug.Log("STOP");
            }

            ResetDestination();
            _currentState = State.Idle;

            OnEndedMoving?.Invoke(this, EventArgs.Empty);
        }
    }

    private void Setup()
    {
        MaxHealth = Team.Health;
        _damage = Team.Damage;
        Health = Team.Health;
        TargetPosition = transform.position;

        if (Player.Instance.Team == Team)
        {
            gameObject.layer = PawnSelections.Instance.PlayerPawnLayerId;
        }
        else
        {
            gameObject.layer = PawnSelections.Instance.EnemyPawnLayerId;
        }

        PawnSelections.Instance.PawnList.Add(this);
    }

    private void Pawn_OnEndedSitting(object sender, EventArgs e)
    {
        _timerToSit = 0f;
        _currentState = State.Idle;
    }

    // Do something at first move
    private void StartedMoving()
    {
        _navMeshAgent.destination = _targetPosition;
        _lastPosition = transform.position;
        _lastMoveTime = Time.time;
        _targetPosition = Vector3.zero;

        OnStartedMoving?.Invoke(this, EventArgs.Empty);
    }

    // If PawnAI is too far from target point, go back
    private bool ShouldReturn()
    {
        return !IsPositionCloseEnough(TargetPosition, transform.position, _walkRadiusOnTarget);
    }

    // If PawnAI to follow is not null AND PawnAI is not too close to following PawnAI
    private bool ShouldFollowEnemy()
    {
        return _pawnToFollow != null && !IsPositionCloseEnough(_pawnToFollow.transform.position, transform.position, _distanceIgnoreThreshold);
    }

    // If PawnAI has moved at least on a certain distance return true
    private bool ShouldMove()
    {
        return Vector3.Distance(_lastPosition, transform.position) > _carvingMoveThreshold;
    }

    // If PawnAI's current State is Moving AND the last time it has moved + carving time is less than current Time OR PawnAI's NavMeshAgent doesn't have a path return true
    private bool ShouldStop()
    {
        return (_currentState == State.Moving) && (_lastMoveTime + _carvingTime < Time.time || !_navMeshAgent.hasPath);
    }

    private void ResetDestination()
    {
        _navMeshAgent.destination = transform.position;
    }

    private bool TryGetClosestEnemy(float seeingDistance, out PawnAI targetPawn)
    {
        targetPawn = null;

        // In priority is PawnAI that was selected to follow, so if it's not null, set it as a target
        if (_pawnToFollow != null)
        {
            targetPawn = _pawnToFollow;
        }
        else
        {
            foreach (PawnAI pawn in PawnSelections.Instance.PawnList)
            {
                // Distance between this PawnAI and current PawnAI from the list that is not from the same team
                if (pawn.Team != Team && IsPositionCloseEnough(pawn.transform.position, transform.position, seeingDistance))
                {
                    seeingDistance = Vector3.Distance(pawn.transform.position, transform.position);
                    targetPawn = pawn;
                }
            }
        }

        if (targetPawn != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
