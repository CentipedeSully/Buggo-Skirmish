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

    public int GetControllerID();

    public string GetControllerName();

    public Faction GetFaction();

    public CreatureType GetCreatureType();

    public CreatureState GetCurrentCreatureState();

    public bool IsDead();

}



public abstract class AbstractCreatureBehavior : MonoBehaviour, ICreatureController
{
    //Declarations
    [TabGroup("Creature Behavior","Setup")]
    [SerializeField] protected CreatureData _creatureData;

    [TabGroup("Creature Behavior", "Info")]
    [SerializeField] protected CreatureState _state;

    [TabGroup("Creature Behavior", "Info")]
    [SerializeField] protected Faction _faction;

    [TabGroup("Creature Behavior", "Info")]
    [SerializeField] protected CreatureType _type;

    [TabGroup("Creature Behavior", "Info")]
    [SerializeField] protected bool _isDead = false;

    [TabGroup("Creature Behavior", "Debug")]
    [SerializeField] protected bool _isDebugActive = false;




    //Monobehaviours
    private void Awake()
    {
        InitializeReferences();
        InitializeCreatureBehaviors();
    }




    //Internals
    protected virtual void InitializeReferences()
    {
        _type = _creatureData.GetCreatureType();
    }
    protected void InitializeCreatureBehaviors()
    {
        foreach (ICreatureBehavior behaviour in GetComponents<ICreatureBehavior>())
        {
            behaviour.ReadCreatureData(_creatureData);
        }
    }



    //Externals
    public int GetControllerID() {return GetInstanceID();}

    public string GetControllerName() {return name;}

    public Faction GetFaction() { return _faction;}

    public CreatureType GetCreatureType() {return _type;}

    public GameObject GetGameObject() { return GetGameObject();}

    public bool IsDead() { return _isDead;}

    public CreatureState GetCurrentCreatureState() { return _state;}



    //Debug


}
