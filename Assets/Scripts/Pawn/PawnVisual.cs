using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(Outline))]
public class PawnVisual : MonoBehaviour
{
    private const string IS_MOVING = "IsMoving";
    private const string IS_SITTING = "IsSitting";
    private readonly string[] ATTACK_TRIGGER_ARRAY = new string[] { "OnAttack_1" };

    [SerializeField] private Pawn _pawn;
    [SerializeField] private Transform[] _bonesArray;
    [SerializeField] private HealthBarUI _healthBarUI;

    public EventHandler OnAttack;

    private Outline _selectedOutline;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _selectedOutline = GetComponent<Outline>();
    }

    private void Start()
    {
        _pawn.OnStartedMoving += Pawn_OnStartedMoving;
        _pawn.OnEndedMoving += Pawn_OnEndedMoving;
        _pawn.OnSelected += Pawn_OnSelected;
        _pawn.OnDeselected += Pawn_OnDeselected;
        _pawn.OnStartedSitting += Pawn_OnStartedSitting;
        _pawn.OnEndedSitting += Pawn_OnEndedSitting;
        _pawn.OnAttack += Pawn_OnAttack;
        _pawn.OnHealthChanged += Pawn_OnHealthChanged;

        _selectedOutline.enabled = false;

        _healthBarUI.ChangeWidth(_pawn.MaxHealth);
        _healthBarUI.ChangeColor(_pawn.Team.Color);

        ChangeBonesMaterial();
    }

    private void Pawn_OnHealthChanged(object sender, Pawn.OnHealthChangedEventArgs e)
    {
        _healthBarUI.UpdateHealthBar(_pawn.MaxHealth, e.Health);
    }

    private void OnDestroy()
    {
        _pawn.OnStartedMoving -= Pawn_OnStartedMoving;
        _pawn.OnEndedMoving -= Pawn_OnEndedMoving;
        _pawn.OnSelected -= Pawn_OnSelected;
        _pawn.OnDeselected -= Pawn_OnDeselected;
        _pawn.OnStartedSitting -= Pawn_OnStartedSitting;
        _pawn.OnEndedSitting -= Pawn_OnEndedSitting;
        _pawn.OnAttack -= Pawn_OnAttack;
        _pawn.OnHealthChanged -= Pawn_OnHealthChanged;

    }

    protected void Attack()
    {
        OnAttack?.Invoke(this, EventArgs.Empty);
        _pawn.OnAttackTarget();
    }

    protected void AttackEnded()
    {
        _pawn.OnAttackEnd();
        AnimationStopped();
    }

    protected void AnimationStopped()
    {
        _pawn.OnAnimationStopped();
    }

    protected void AnimationStarted()
    {
        _pawn.OnAnimationStarted();
    }

    private void Pawn_OnAttack(object sender, System.EventArgs e)
    {
        AnimationStarted();
        _animator.SetTrigger(ATTACK_TRIGGER_ARRAY[UnityEngine.Random.Range(0, ATTACK_TRIGGER_ARRAY.Length)]);
    }

    private void Pawn_OnEndedSitting(object sender, System.EventArgs e)
    {
        AnimationStarted();
        _animator.SetBool(IS_SITTING, false);
    }

    private void Pawn_OnStartedSitting(object sender, System.EventArgs e)
    {
        AnimationStarted();
        _animator.SetBool(IS_SITTING, true);
    }

    private void Pawn_OnDeselected(object sender, System.EventArgs e)
    {
        _selectedOutline.enabled = false;
    }

    private void Pawn_OnSelected(object sender, Pawn.OnSelectedEventArgs e)
    {
        _selectedOutline.OutlineColor = e.Color;
        _selectedOutline.enabled = true;
    }

    private void Pawn_OnEndedMoving(object sender, System.EventArgs e)
    {
        _animator.SetBool(IS_MOVING, false);
    }

    private void Pawn_OnStartedMoving(object sender, System.EventArgs e)
    {
        _animator.SetBool(IS_MOVING, true);
    }


    // Getting each editable bone's transform and changing it's material as the team's one
    private void ChangeBonesMaterial()
    {
        foreach (Transform transform in _bonesArray)
        {
            if (transform.gameObject.TryGetComponent(out Renderer renderer))
            {
                renderer.material = _pawn.Team.Material;
            }
        }
    }
}
