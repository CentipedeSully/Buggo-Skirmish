using Sirenix.OdinInspector;
using UnityEngine;




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
}


public abstract class AbstractController : MonoBehaviour, IEntityController, IEntityID
{
    //Declarations
    [BoxGroup("Entity Controller Data")]
    [SerializeField] protected int _entityID;

    [BoxGroup("Entity Controller Data")]
    [SerializeField] protected Faction _faction;

    [BoxGroup("Entity Controller Data")]
    [SerializeField] protected bool _isAwake = false;

    [BoxGroup("Entity Controller Data")]
    [SerializeField] protected bool _isDead = false;


    //Monobehaviours
    private void Awake()
    {
        _entityID = GetInstanceID();
        InitializeOtherUtilities();
    }



    //Internals
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
}
