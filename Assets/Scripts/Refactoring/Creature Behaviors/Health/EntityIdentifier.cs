using UnityEngine;

public class EntityIdentifier : MonoBehaviour, IEntityID
{
    //Declarations
    [SerializeField] private int _entityID;
    private IHealthBehavior _healthBehavior;



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


        _healthBehavior = GetComponent<IHealthBehavior>();
    }


    //Externals
    public void SetID(int ID)
    {
        _entityID = ID;
    }

    public void SetHealthBehaviour(IHealthBehavior health)
    {
        _healthBehavior = health;
    }

    public int GetEntityID()
    {
        return _entityID;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public IHealthBehavior GetHealthBehavior()
    {
        return _healthBehavior;
    }
}
