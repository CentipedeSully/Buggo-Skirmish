using Sirenix.OdinInspector;
using UnityEngine;

public class RemoteHealthExtension : MonoBehaviour, IHealthBehavior
{
    //Declarations
    private IHealthBehavior _healthBehavior;
    [SerializeField] private LimbRebounder _limbRebounder;
    [SerializeField] private LimbVibrator _limbVibrator;


    //Monobehaviours



    //Internals





    //Externals
    public void SetHealthBehavior(IHealthBehavior behaviour) { _healthBehavior = behaviour; }

    public int CurrentHealth()
    {
        if (_healthBehavior != null)
            return _healthBehavior.CurrentHealth();
        else return -1;
    }

    public GameObject GetGameObject()
    {
        if (_healthBehavior != null)
            return _healthBehavior.GetGameObject();
        else return gameObject;
    }

    public bool IsDead()
    {
        if (_healthBehavior != null)
            return _healthBehavior.IsDead();
        else return true;
    }

    public int MaxHealth()
    {
        if (_healthBehavior != null)
            return _healthBehavior.MaxHealth();
        else return -1;
    }

    [Button]
    public void TakeDamage(DamageInfo dmgInfo)
    {
        if (_healthBehavior != null)
        {
            //apply localized limbReaction
            _limbVibrator?.Vibrate();
            _limbRebounder?.ApplyBounceDirection(dmgInfo.SourceDirection, .5f);

            //take the damage
            _healthBehavior.TakeDamage(dmgInfo);
        }
    }

    public bool IsInInvincRecovery()
    {
        if ( _healthBehavior != null)
            return _healthBehavior.IsInInvincRecovery();
        else return false;
    }
}
