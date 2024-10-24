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
    [BoxGroup("Creature Setup")]
    [SerializeField] protected CreatureData _creatureData;

    [BoxGroup("Creature Info")]
    [SerializeField] protected CreatureState _state;

    [BoxGroup("Creature Info")]
    [SerializeField] protected CreatureType _type;

    [BoxGroup("Debug")]
    [SerializeField] protected bool _isDebugActive = false;




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
    }


    //Externals
    public CreatureType GetCreatureType() {return _type;}

    public CreatureState GetCurrentCreatureState() { return _state;}



    //Debug


}
