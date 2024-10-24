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
    }

    private void InterruptAttack()
    {
        if (_damageInterruptsAttacks && _atkBehavour != null)
        {
            Debug.Log("Attack Interruption Sent");
            _atkBehavour.InterruptAttack();
        }
            
    }

    private void DropItems()
    {
        if (_damageDropsItems)
        {
            Debug.Log("Carry Interruption Sent");
            //empty ya pockets!
            //...
        }
    }

    protected override void ApplyOtherReactionsToDamage(GameObject source, int damage)
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
