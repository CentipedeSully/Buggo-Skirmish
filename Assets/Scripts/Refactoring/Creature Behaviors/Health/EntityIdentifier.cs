using UnityEngine;

public class EntityIdentifier : MonoBehaviour, IEntityID
{
    //Declarations
    [SerializeField] private int _entityID;
    [SerializeField] private Faction _faction;
    [SerializeField] private IHealthBehavior _healthBehavior;
    private bool _isDead = false;
    private bool _isAwake = false;

    //Monobehaviours
    private void Awake()
    {
        InitializeDefaultReferencesIfPossible();
    }


    //Internals
    private void InitializeDefaultReferencesIfPossible()
    {
        //use this script's Instance ID as the Entity's ID.
        //Useful for simple objects with no big controller scripts
        _entityID = GetInstanceID();

        //Attempt to find a health behaviour on this object
        _healthBehavior = GetComponent<IHealthBehavior>();
    }


    //Externals
    public void SetID(int ID)
    {
        _entityID = ID;
    }

    public int GetEntityID()
    {
        return _entityID;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public Faction GetFaction() {return _faction; }

    public bool IsDead()
    {
        if (_healthBehavior != null)
            return _healthBehavior.IsDead();
        else return _isDead;
    }

    public bool IsAwake()
    {
        return _isAwake;
    }

    public void SetDeathState(bool newState) { _isDead = newState; }
    public void SetAwakeState(bool newState) { _isAwake = newState; }
    public void SetFaction(Faction newFaction) {  _faction = newFaction; }
}
