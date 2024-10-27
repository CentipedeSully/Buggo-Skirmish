using Sirenix.OdinInspector;
using System;
using UnityEngine;



public enum CreatureState
{
    unset,
    idle,
    movingToDestination,
    followingLeader,
    pursuingEnemy,
    attackingEnemy,
    collectingPickup,
    depositingPickup,
    dead
}

public interface ICreatureController
{
    public GameObject GetGameObject();

    public int GetEntityID();

    public string GetControllerName();

    public Faction GetFaction();

    public CreatureType GetCreatureType();

    public CreatureState GetCurrentCreatureState();

    public bool IsDead();

    public void SignalDeath();

}



public abstract class AbstractCreatureBehavior : AbstractController, ICreatureController
{
    //Declarations
    [TabGroup("Creature", "Setup")]
    [SerializeField] protected CreatureData _creatureData;

    [TabGroup("Creature", "Info")]
    [SerializeField] protected CreatureState _state;

    [TabGroup("Creature", "Info")]
    [SerializeField] protected CreatureType _type;

    protected RagdollCommunicator _ragdollCommunicator;
    protected ICarriable _selfCarriableBehavior;


    //Monobehaviours




    //Internals
    protected override void InitializeOtherUtilities()
    {
        //Use the parent's Awake to initialize this object's references
        InitializeCreatureReferences();
        InitializeCreatureBehaviors();
    }
    protected virtual void InitializeCreatureReferences()
    {
        _type = _creatureData.GetCreatureType();

        _ragdollCommunicator = GetComponent<RagdollCommunicator>();

    }
    protected void InitializeCreatureBehaviors()
    {
        foreach (ICreatureBehavior behaviour in GetComponents<ICreatureBehavior>())
            behaviour.ReadCreatureData(_creatureData);
    }
    protected void InterruptCreatureBehaviors()
    {
        foreach (ICreatureBehavior creatureBehavior in GetComponents<ICreatureBehavior>())
            creatureBehavior.InterruptBehavior();
    }
    protected override void ApplyOtherReactionToDeath()
    {
        InterruptCreatureBehaviors();
        _ragdollCommunicator.ToggleRagdoll(true);

        //Become a valid carriable
        _selfCarriableBehavior?.EnableCarry(true);
    }

    protected override void ApplyOtherReactionToAwaken()
    {
        //pass
    }


    //Externals
    public CreatureType GetCreatureType() {return _type;}

    public CreatureState GetCurrentCreatureState() { return _state;}



    //Debug


}
