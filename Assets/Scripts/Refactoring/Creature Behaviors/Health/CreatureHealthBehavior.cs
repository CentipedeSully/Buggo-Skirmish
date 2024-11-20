using Sirenix.OdinInspector;
using UnityEngine;

public class CreatureHealthBehavior : AbstractHealthBehaviour, ICreatureBehavior
{
    //Declarations
    [BoxGroup("Additional Features")]
    [SerializeField] private bool _damageInterruptsAttacks = true;
    [BoxGroup("Additional Features")]
    [SerializeField] private bool _damageDropsItems = true;

    private ICreatureController _controller;
    private IAttackBehaviour _atkBehavour;
    private ICarryBehavior _carryBehavior;


    //Monobehaviours
    private void Awake()
    {
        InitializeReferences();
    }


    //Internals
    private void InitializeReferences()
    {
        _atkBehavour = GetComponent<IAttackBehaviour>();
        _controller = GetComponent<ICreatureController>();
        _carryBehavior = GetComponent<ICarryBehavior>();
    }

    private void InterruptAttack()
    {
        if (_damageInterruptsAttacks && _atkBehavour != null)
        {
            _atkBehavour.InterruptAttack();
        }
            
    }

    private void DropItems()
    {
        if (_damageDropsItems)
        {
            _carryBehavior.DropItem();
        }
    }

    protected override void ApplyOtherReactionsToDamage(DamageInfo dmgInfo)
    {
        InterruptAttack();
        DropItems();
    }

    protected override void ApplyOtherReactionsToDeath()
    {
        //Tell the controller we died
        _controller.SignalDeath();

        //throw body
        //...
    }




    //Externals
    public void ReadCreatureData(CreatureData data)
    {
        _maxHealth = data.GetBaseHealth();
        _currentHealth = _maxHealth;
    }

    public void InterruptBehavior()
    {
        //pass. Interrupting health does nothing atm
    }
}
