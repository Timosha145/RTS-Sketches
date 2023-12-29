using PoplarLib;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Pawn : ExtendedMonoBehaviour
{
    [SerializeField] private int _numberOfHitsPerAttack;
    [SerializeField] private float _rotationSpeed = 4f;
    [SerializeField] private float _distanceToSee = 1.75f;
    [SerializeField] private float _distanceToAttack;
    [SerializeField] private float _attackPerSeconds;

    [field: SerializeField] public Team Team { get; private set; }
    [field: SerializeField] public float Offset { get; private set; }

    public event EventHandler OnDestroyed;
    public event EventHandler<OnSelectedEventArgs> OnSelected;
    public event EventHandler OnDeselected;
    public event EventHandler<OnHealthChangedEventArgs> OnHealthChanged;
    public event EventHandler OnStartedMoving, OnEndedMoving;
    public event EventHandler OnStartedSitting, OnEndedSitting;
    public event EventHandler OnAttack;

    public float Health { get; protected set; }
    public float MaxHealth { get; protected set; }
    public float Damage { get; protected set; }

    protected State _currentState = State.Idle;
    protected NavMeshAgent _navMeshAgent;
    protected Pawn _pawnToFollow;
    protected Pawn _targetPawn;

    private int _numOfPlatingAnimations = 0;
    private float _timerToSit, _timerToSitMax = 3f;
    private float _lastMoveTime;
    private float _carvingTime = 0.5f;
    private float _carvingMoveThreshold = 0.01f;
    private float _pawnOnWayThreshold = 1.5f;
    private float _distanceIgnoreThreshold = 1f;
    private float _minimumDistanceToMove = 0.25f;
    private float _walkRadiusOnTarget = 3f;
    private float _timerToAttack, _timerToAttackMax = 2f;
    private float _timerToChangeLastPos, _timerToChangeLastPosMax = 0.3f;

    private bool _isAnotherPawnOnTargetPosition = false;
    private bool _propertiesAreSet = false;
    private bool _isAnimationPlaying = false; // Only certain animations change value of this variable

    private Vector3 _targetPosition = Vector3.zero;
    private Vector3 _lastPosition;
    private Vector3 _stayingPosition;

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

    public enum State
    {
        Idle,
        Sitting,
        Moving,
        Attacking,
        PlayingAnimation
    }

    private void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _lastPosition = transform.position;
        _timerToAttackMax = _attackPerSeconds;
    }

    private void Start()
    {
        OnEndedSitting += Pawn_OnEndedSitting;
        Team.AddPawn(this);
        Setup();
    }

    private void Update()
    {
        HandleMovement();
        HandleSitting();
        HandleAttack();
    }

    private void OnDestroy()
    {
        OnDestroyed?.Invoke(this, EventArgs.Empty);

        Team.RemovePawn(this);
        PawnSelections.Instance.PawnList.Remove(this);

        if (PawnSelections.Instance.SelectedEnemyPawn == this)
        {
            PawnSelections.Instance.DeselectEnemy();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _isAnotherPawnOnTargetPosition = other.TryGetComponent(out Pawn pawn) && IsCloseEnough(pawn.transform.position, _stayingPosition, _pawnOnWayThreshold);
    }

    public State GetState()
    {
        return _currentState;
    }

    public Vector3 GetDestination()
    {
        return _navMeshAgent.destination;
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

    public void FollowEnemy(Pawn enemyPawn)
    {
        if (_targetPawn != enemyPawn)
        {
            _pawnToFollow = enemyPawn;
            MoveToTarget(enemyPawn.transform.position);
        }
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

    public void OnAttackTarget()
    {
        if (_targetPawn != null)
        {
            _targetPawn.ChangeHealth(-Math.Abs(Damage / _numberOfHitsPerAttack));
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

    public void OrderToMove(Vector3 targetPos)
    {
        if (this != null)
        {
            _stayingPosition = targetPos;
            MoveToTarget(targetPos);
            StopFollowing();
        }
    }

    public void OnAnimationStarted()
    {
        _numOfPlatingAnimations++;
        _isAnimationPlaying = true;
    }

    public void OnAnimationStopped()
    {
        _numOfPlatingAnimations--;

        if (_numOfPlatingAnimations == 0)
        {
            _isAnimationPlaying = false;
        }
    }

    protected void HandleSitting()
    {
        if (_currentState == State.Idle && HandleTimer(ref _timerToSit, _timerToSitMax) && _targetPawn == null)
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
        if (IsAnyEnemyCloseEnoughToApproach())
        {
            if (IsAnyEnemyCloseEnoughToFight())
            {
                ResetDestination();
                SlerpRotateTowards(_targetPawn.transform.position, _rotationSpeed);

                if (_currentState != State.Attacking && HandleTimer(ref _timerToAttack, _timerToAttackMax))
                {
                    _currentState = State.Attacking;
                    OnAttack?.Invoke(this, EventArgs.Empty);

                    _pawnToFollow = null;
                }
            }
            else if (!ShouldReturnToStayingPos())
            {
                MoveToTarget(_targetPawn.transform.position);
                _timerToAttack = 0f;
            }
        }
        else
        {
            _targetPawn = null;
        }
    }

    protected void HandleMovement()
    {
        if (_isAnimationPlaying || _currentState == State.Sitting)
        {
            _navMeshAgent.destination = transform.position;
            return;
        }

        if (_targetPosition != Vector3.zero)
        {
            StartedMoving();
        }

        if (ShouldMove())
        {
            _currentState = State.Moving;
            _lastMoveTime = Time.time;

            if (HandleTimer(ref _timerToChangeLastPos, _timerToChangeLastPosMax))
            {
                _lastPosition = transform.position;
            }

            if (ShouldFollowEnemy())
            {
                MoveToTarget(_pawnToFollow.transform.position);
                _stayingPosition = _pawnToFollow.transform.position;
            }
            else if (ShouldReturnToStayingPos())
            {
                MoveToTarget(_stayingPosition);
            }
        }

        if (ShouldStop())
        {
            ResetDestination();
            UnsetNumOfPlatingAnimations();

            _isAnotherPawnOnTargetPosition = false;
            _currentState = State.Idle;
            OnEndedMoving?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UnsetNumOfPlatingAnimations()
    {
        _numOfPlatingAnimations = 0;
    }

    private void Pawn_OnEndedSitting(object sender, EventArgs e)
    {
        _timerToSit = 0f;
        _currentState = State.Idle;
    }

    private void MoveToTarget(Vector3 targetPos)
    {
        if (!IsCloseEnough(transform.position, targetPos, _minimumDistanceToMove) && _targetPosition != targetPos)
        {
            if (_currentState == State.Sitting)
            {
                OnEndedSitting?.Invoke(this, EventArgs.Empty);
            }

            _targetPosition = targetPos;
        }
    }

    private void StartedMoving()
    {
        _navMeshAgent.destination = _targetPosition;
        if (_currentState != State.Moving)
        {
            _lastPosition = transform.position;
            _lastMoveTime = Time.time;
            _targetPosition = Vector3.zero;
            _currentState = State.Moving;
            OnStartedMoving?.Invoke(this, EventArgs.Empty);
        }
    }

    private bool IsAnyEnemyCloseEnoughToApproach()
    {
        return IsCloseEnough(_navMeshAgent.destination, transform.position, _distanceIgnoreThreshold)
            && TryGetClosestEnemy(_distanceToSee, out _targetPawn);
    }

    private bool IsAnyEnemyCloseEnoughToFight()
    {
        if (_targetPawn == null)
        {
            return false;
        }
        else
        {
            return IsCloseEnough(_targetPawn.transform.position, transform.position, _distanceToAttack);
        }
    }

    private bool ShouldReturnToStayingPos()
    {
        return !IsCloseEnough(_stayingPosition, transform.position, _walkRadiusOnTarget);
    }

    private bool ShouldFollowEnemy()
    {
        return _pawnToFollow != null && !IsCloseEnough(_pawnToFollow.transform.position, transform.position, _distanceIgnoreThreshold);
    }

    private bool ShouldMove()
    {
        return !IsCloseEnough(_lastPosition, transform.position, _carvingMoveThreshold) 
            && _currentState == State.Moving;
    }

    private bool ShouldStop()
    {
        // Maybe because of detecting near pawn it makes bugs with sitting or attacking
        return _currentState == State.Moving && (_lastMoveTime + _carvingTime < Time.time || _navMeshAgent.destination == transform.position || _isAnotherPawnOnTargetPosition);
    }

    private void ResetDestination()
    {
        _navMeshAgent.destination = transform.position;
    }

    private bool TryGetClosestEnemy(float seeingDistance, out Pawn targetPawn)
    {
        targetPawn = null;

        if (_pawnToFollow != null)
        {
            targetPawn = _pawnToFollow;
        }
        else
        {
            foreach (Pawn pawn in PawnSelections.Instance.PawnList)
            {
                if (pawn.Team != Team && IsCloseEnough(pawn.transform.position, transform.position, seeingDistance))
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

    private void Setup()
    {
        MaxHealth = Team.TeamDataSO.Health;
        Damage = Team.TeamDataSO.Damage;
        Health = Team.TeamDataSO.Health;
        _stayingPosition = transform.position;

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
}