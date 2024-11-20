using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static AbstractAttackBehavior;



public enum AtkState
{
    unset,
    notAttacking,
    preparingAtk,
    castingAtk,
    recoveringAtk,
    coolingDownAtk
}

public interface IAttackBehaviour
{
    void PerformAttack();
    void InterruptAttack();
    bool IsAttacking();
    bool IsAttackReady();
    AtkState AtkState { get; }
    event AtkStateEvent OnAtkStateChanged;

    int GetDamage();
    void SetDamage(int damage);

    float GetAtkDuration();
    float GetAtkPrepDuration();
    float GetAtkCastDuration();
    float GetAtkRecoveryDuration();
    void SetAtkPrepDuration(float atkPrepDuration);
    void SetAtkCastDuration(float atkCastDuration);
    void SetAtkRecoveryDuration(float atkRecoveryDuration);
    float GetAtkCooldown();
    void SetAtkCooldown(float cooldown);

    void InitializeDamageInfoAndEntityID();
}


public abstract class AbstractAttackBehavior : MonoBehaviour, IAttackBehaviour
{
    //Declarations
    protected IEntityID _entityID;

    [TabGroup("General Settings", "State && Timing")]
    [SerializeField][ReadOnly] protected AtkState _atkState = AtkState.unset;
    [TabGroup("General Settings", "State && Timing")]
    [SerializeField] protected float _atkPreparationDuration;
    [TabGroup("General Settings", "State && Timing")]
    [SerializeField] protected float _atkCastDuration;
    [TabGroup("General Settings", "State && Timing")]
    [SerializeField] protected float _atkRecoveryDuration;
    [TabGroup("General Settings", "State && Timing")]
    [SerializeField] protected float _atkCooldownDuration;

    //keep only the general casting data.
    //Allow the children to play it their way
    [TabGroup("General Settings", "Damage & Casting")]
    [SerializeField] protected DamageInfo _damageInfo = new();
    [TabGroup("General Settings", "Damage & Casting")]
    [SerializeField] protected LayerMask _detectionMask;
    [TabGroup("General Settings", "Damage & Casting")]
    [SerializeField] protected bool _showCastGizmos = false;
    [TabGroup("General Settings", "Damage & Casting")]
    [SerializeField] protected Transform _atkOrigin;





    protected IEnumerator _attackSequencer;
    public delegate void AtkStateEvent(AtkState newState);
    public event AtkStateEvent OnStateChanged;

    event AtkStateEvent IAttackBehaviour.OnAtkStateChanged
    {
        add
        {
            OnStateChanged += value;
        }

        remove
        {
            OnStateChanged -= value;
        }
    }




    //Monobehaviours
    private void OnDrawGizmosSelected()
    {
        if (_showCastGizmos)
            DrawAttackGizmos();
    }

    private void Start()
    {
        InitializeDamageInfoAndEntityID();
    }


    //Internala
    protected virtual void DrawAttackGizmos() { }
    protected IEnumerator RunAttackSequence()
    {
        //update the state
        UpdateAtkState(AtkState.preparingAtk);

        //wait for the duration of the atkPrep state
        yield return new WaitForSeconds(_atkPreparationDuration);

        //update the state
        UpdateAtkState(AtkState.castingAtk);

        //wait for the duration of the atkCast state
        yield return new WaitForSeconds(_atkCastDuration);

        //update the state
        UpdateAtkState(AtkState.recoveringAtk);

        //wait for the duration of the atkRecovery state
        yield return new WaitForSeconds(_atkRecoveryDuration);

        //the attack has completed. reset this utility
        _attackSequencer = null;

        //update the state 
        UpdateAtkState(AtkState.coolingDownAtk); //this also indirectly triggers a separate cooldown timer
    }

    protected void UpdateAtkState(AtkState newState)
    {
        if (newState != _atkState && newState != AtkState.unset)
        {
            //set the new state
            _atkState = newState;

            //update our data FIRST
            RespondToStateChange(_atkState);

            //notify external dependents last
            OnStateChanged?.Invoke(_atkState);
        }
    }

    protected void RespondToStateChange(AtkState newState)
    {
        switch (newState)
        {
            case AtkState.notAttacking:
                //leave room for children
                PerformAdditionalUtilsOnNoAtkEntered();
                break;


            case AtkState.preparingAtk:
                //leave room for children
                PerformAdditionalUtilsOnAtkPrepEntered();
                break;


            case AtkState.castingAtk:
                //leave room for children
                PerformAdditionalUtilsOnAtkCastEntered();
                break;


            case AtkState.recoveringAtk:
                //leave room for children
                PerformAdditionalUtilsOnAtkRecoveryEntered();
                break;


            case AtkState.coolingDownAtk:
                //invoke a cooldown timer
                Invoke(nameof(ExitCooldown), _atkCooldownDuration);

                //leave room for children
                PerformAdditionalUtilsOnAtkCooldownEntered();
                break;

            default:
                Debug.LogError($"no definition for state '{newState}' currently exists");
                break;
        }
    }

    protected void ExitCooldown()
    {
        UpdateAtkState(AtkState.notAttacking);
    }


    protected virtual void PerformAdditionalUtilsOnNoAtkEntered() { }
    protected virtual void PerformAdditionalUtilsOnAtkPrepEntered() { }
    protected virtual void PerformAdditionalUtilsOnAtkCastEntered() { }
    protected virtual void PerformAdditionalUtilsOnAtkRecoveryEntered() { }
    protected virtual void PerformAdditionalUtilsOnAtkCooldownEntered() { }




    //Externals
    [BoxGroup("Debugging", CenterLabel = true)]
    [Button]
    public void PerformAttack()
    {
        if (IsAttackReady())
        {
            _attackSequencer = RunAttackSequence();
            StartCoroutine(_attackSequencer);
        }
    }

    [BoxGroup("Debugging")]
    [Button]
    public void InterruptAttack()
    {
        if (IsAttacking())
        {
            StopCoroutine(_attackSequencer);
            _attackSequencer = null;

            UpdateAtkState(AtkState.coolingDownAtk);
        }

    }

    public bool IsAttackReady() { return _atkState == AtkState.notAttacking || _atkState == AtkState.unset; }
    public bool IsAttacking()
    {
        return _atkState == AtkState.preparingAtk || _atkState == AtkState.castingAtk || _atkState == AtkState.recoveringAtk;
    }
    public AtkState AtkState => _atkState;



    public int GetDamage() { return _damageInfo.Damage; }
    public void SetDamage(int damage) { _damageInfo.Damage = damage; }

    public float GetAtkDuration() { return _atkPreparationDuration + _atkCastDuration + _atkRecoveryDuration; }
    public float GetAtkPrepDuration() { return _atkPreparationDuration; }
    public float GetAtkCastDuration() { return _atkCastDuration; }
    public float GetAtkRecoveryDuration() { return _atkRecoveryDuration; }
    public float GetAtkCooldown() { return _atkCooldownDuration; }

    public void SetAtkPrepDuration(float atkPrepDuration) { _atkPreparationDuration = Mathf.Max(atkPrepDuration, 0); }
    public void SetAtkCastDuration(float atkCastDuration) { _atkCastDuration = Mathf.Max(atkCastDuration, 0); }
    public void SetAtkRecoveryDuration(float atkRecoveryDuration) { _atkRecoveryDuration = Mathf.Max(atkRecoveryDuration, 0); }
    public void SetAtkCooldown(float cooldown) { _atkCooldownDuration = Mathf.Max(cooldown, 0); }

    public void InitializeDamageInfoAndEntityID()
    {
        _entityID = GetComponent<IEntityID>();

        if (_entityID != null)
        {
            _damageInfo.AttackerID = _entityID.GetEntityID();
            _damageInfo.AttackerFaction = _entityID.GetFaction();
        }
    }
}
