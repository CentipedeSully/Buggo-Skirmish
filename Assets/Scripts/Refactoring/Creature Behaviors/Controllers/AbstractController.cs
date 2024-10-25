using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;




public interface IEntityController
{
    public GameObject GetGameObject();

    public int GetEntityID();

    public string GetControllerName();

    public Faction GetFaction();

    public bool IsDead();

    public bool IsAwake();

    public void SignalAwaken();

    public void SignalDeath();
}

public interface IEntityID
{ 
    public int GetEntityID();

    public GameObject GetGameObject();

    public IHealthBehavior GetHealthBehavior();
}


public abstract class AbstractController : MonoBehaviour, IEntityController, IEntityID
{
    //Declarations
    [TabGroup("Entity Controller", "Setup")]
    [SerializeField] private List<EntityIdentifier> _remoteColliderIdentifiers = new List<EntityIdentifier>();

    [TabGroup("Entity Controller", "Info")]
    [SerializeField] protected int _entityID;

    [TabGroup("Entity Controller", "Info")]
    [SerializeField] protected Faction _faction;

    [TabGroup("Entity Controller", "Info")]
    [SerializeField] protected bool _isAwake = false;

    [TabGroup("Entity Controller", "Info")]
    [SerializeField] protected bool _isDead = false;

    private IHealthBehavior _healthBehavior;


    //Monobehaviours
    private void Awake()
    {
        _entityID = GetInstanceID();
        _healthBehavior = GetComponent<IHealthBehavior>();
        InitializeRemoteColliderIdentifiers();
        InitializeOtherUtilities();
    }



    //Internals
    protected virtual void InitializeRemoteColliderIdentifiers()
    {
        foreach(EntityIdentifier identifier in _remoteColliderIdentifiers)
        {
            identifier.SetID(_entityID);
            identifier.SetHealthBehaviour(_healthBehavior);
        }
    }
    protected abstract void InitializeOtherUtilities();
    protected abstract void ApplyOtherReactionToDeath();
    protected abstract void ApplyOtherReactionToAwaken();




    //Externals
    public int GetEntityID() { return _entityID; }

    public string GetControllerName() { return name; }

    public Faction GetFaction() {  return _faction; }

    public GameObject GetGameObject() {  return GetGameObject(); }

    public bool IsDead() {  return _isDead; }
    public bool IsAwake() {  return _isAwake; }

    public void SignalDeath()
    {
        if (!_isDead)
        {
            _isDead = true;
            ApplyOtherReactionToDeath();
        }
    }

    public void SignalAwaken()
    {
        if (!_isAwake && !_isDead)
        {
            _isAwake = true;
            ApplyOtherReactionToAwaken();
        }
        
    }

    public virtual IHealthBehavior GetHealthBehavior()
    {
        return _healthBehavior;
    }
}
