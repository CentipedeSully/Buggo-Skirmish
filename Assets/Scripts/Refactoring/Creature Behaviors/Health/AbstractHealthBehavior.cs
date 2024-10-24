using UnityEngine;
using Sirenix;
using Sirenix.OdinInspector;

public interface IHealthBehavior
{
    int CurrentHealth();
    int MaxHealth();
    bool IsDead();
    void TakeDamage(GameObject source, int damage);
    GameObject GetGameObject();
}

public abstract class AbstractHealthBehaviour: MonoBehaviour, IHealthBehavior
{
    //Declarations
    [TabGroup("Health Behavior","Info")]
    [SerializeField] protected int _maxHealth;
    [TabGroup("Health Behavior", "Info")]
    [SerializeField] protected int _currentHealth;
    [TabGroup("Health Behavior", "Info")]
    [SerializeField] protected bool _isDead = false;
    [TabGroup("Health Behavior", "Info")]
    [SerializeField] protected float _invincTimeAfterHit;
    protected bool _isInInvincRecovery = false;



    //Monobehaviours




    //Internals
    protected virtual void EnterInvincRecoveryAfterHit()
    {
        _isInInvincRecovery = true;

        Invoke(nameof(ExitInvincRecovery), _invincTimeAfterHit);
    }

    protected virtual void ExitInvincRecovery()
    {
        _isInInvincRecovery = false;
    }

    protected virtual void Die()
    {
        _isDead = true;
        ApplyOtherReactionsToDeath();
    }

    protected abstract void ApplyOtherReactionsToDeath();

    protected virtual void ApplyDamage(int damage)
    {
        _currentHealth -= damage;
    }

    protected abstract void ApplyOtherReactionsToDamage(GameObject source, int damage);

    protected virtual void EitherDieOrEnterRecovery()
    {
        if (_currentHealth <= 0)
        {
            Die();
            return;
        }
        else
        {
            //enter invinc to avoid too-frequent frame-after-frame hits
            EnterInvincRecoveryAfterHit();
        }
    }



    //Externals
    [TabGroup("Health Behavior", "Debug")]
    [Button]
    public virtual void TakeDamage(GameObject source, int damage)
    {
        if (!_isDead && !_isInInvincRecovery)
        {
            ApplyDamage(damage);
            ApplyOtherReactionsToDamage(source, damage);
            EitherDieOrEnterRecovery();
        }
    }

    public GameObject GetGameObject() { return gameObject; }
    public int CurrentHealth() {  return _currentHealth; }
    public int MaxHealth() { return _maxHealth; }
    public bool IsDead() { return _isDead; }


}


