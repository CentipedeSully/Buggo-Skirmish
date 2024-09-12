using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Rendering;



public interface IResourcePoint
{
    public bool IsResourcePointAvailable();

    public GameObject GetGameObject();
}


public class AiNestBehavior : MonoBehaviour, ITargetable, IResourcePoint
{
    //Declarations
    [SerializeField] private Transform _actorsContainer;
    [SerializeField] private Transform _pickupsContainer;
    [SerializeField] private InteractableType _interactableType = InteractableType.Nest;
    [SerializeField] private Faction _faction = Faction.Enemy;
    [SerializeField] private int _health = 60;
    [SerializeField] private bool _isDead = false;
    [SerializeField] private List<Transform> _minionSpawnPositions = new();
    [SerializeField] private Transform _pickupSpawnPosition;
    [SerializeField] private List<GameObject> _minionPrefabs = new();
    [SerializeField] private List<GameObject> _pickupPrefabs= new();
    [SerializeField] private Transform _rallyPoint;
    [SerializeField] private float _minionSpawnCooldown = 15f;
    [SerializeField] private bool _isMinionSpawnerReady = true;
    [SerializeField] private float _burstMinionSpawnDelay = 1f;
    [SerializeField] private int _burstMinionSpawnCountMin = 3;
    [SerializeField] private int _burstMinionSpawnCountMax = 7;
    [SerializeField] private float _pickupSpawnCooldown = 10f;
    [SerializeField] private bool _isPickupSpawnerReady = true;
    [SerializeField] private float _burstPickupSpawnDelay = .2f;
    [SerializeField] private int _burstPickupSpawnCountMin = 3;
    [SerializeField] private int _burstPickupSpawnCountMax = 4;
    [SerializeField] private bool _isSpawning = false;
    [SerializeField] private bool _isNestActive = false;
    [SerializeField] private float _activationTime = 20f;
    [SerializeField] private bool _autoActivateAfterTime = false;

    private IEnumerator _minionBurstSpawnManager;
    private IEnumerator _pickupBurstSpawnManager;


    //Monobehaviours
    private void Start()
    {
        if (_autoActivateAfterTime)
            Invoke(nameof(ActivateNest), _activationTime);
    }



    //Internals
    private void Die()
    {
        _isDead = true;

        //play death animation
        //...

        //stop spawning Minions
        StopMinionSpawning();

        //start Spawning pickups
        StartPickupBurst();

    }

    private void SpawnMinion()
    {
        if (_minionPrefabs.Count > 0)
        {
            //select a random minion to spawn
            GameObject minionPrefab = _minionPrefabs[Random.Range(0, _minionPrefabs.Count)];

            //select a random location to spawn the minion
            Transform spawnLocation = _minionSpawnPositions[Random.Range(0, _minionSpawnPositions.Count)];

            //create the minion
            GameObject newMinion = Instantiate(minionPrefab, spawnLocation.position,Quaternion.identity);

            //set the minion's parent
            newMinion.transform.SetParent(_actorsContainer,true);

            //get the minion's aiBehavior
            AiBehavior behavior = newMinion.GetComponent<AiBehavior>();

            //Setup the minion to approach the rally point
            behavior.MoveToPosition(_rallyPoint.position);
        }
        
    }

    private void SpawnPickup()
    {
        if (_pickupPrefabs.Count > 0)
        {
            //select a random pickup to spawn
            GameObject pickupPrefab = _pickupPrefabs[Random.Range(0, _pickupPrefabs.Count)];

            //create the pickup
            GameObject newPickup = Instantiate(pickupPrefab, _pickupSpawnPosition.position, Quaternion.identity);

            //set the pickup's parent
            newPickup.transform.SetParent(_pickupsContainer, true);

            //get the minion's aiBehavior
            PickupBehavior behavior = newPickup.GetComponent<PickupBehavior>();

            //make the pickup drop to the floor
            behavior.SetPickupState(false); //resets the pickup's states and temporarily enables its gravity
        }
    }

    private IEnumerator BurstSpawnMinions()
    {
        //enter our spawning state
        _isSpawning = true;

        //set the current burst's spawn count target
        int targetSpawnCount = Random.Range(_burstMinionSpawnCountMin, _burstMinionSpawnCountMax + 1);
        int currentSpawnCount = 0;

        //make sure no one makes any editor mistakes ;)
        if (currentSpawnCount > targetSpawnCount)
            targetSpawnCount = currentSpawnCount + 1;

        //are we below our spawn quota (and are we still alive)?
        while (currentSpawnCount < targetSpawnCount && !_isDead)
        {
            //spawn a minion
            SpawnMinion();

            //increment our count
            currentSpawnCount++;

            //wait for the delay
            yield return new WaitForSeconds(_burstMinionSpawnDelay);
        }

        //exit the spawn state
        _isSpawning = false;

        //clear the enemy spawn manager utility
        _minionBurstSpawnManager = null;

        //Cooldown the nest's spawning
        Invoke(nameof(ReadyMinionSpawner), _minionSpawnCooldown);
    }

    private void ReadyMinionSpawner()
    {
        _isMinionSpawnerReady = true;

        //if we aren't dead, start spawning minions again
        if (!_isDead)
            StartMinionBurst();
    }

    private void StartMinionBurst()
    {
        //is the spawner ready? Are we not dead? ARE WE NOT ALREADY SPAWNING MINIONS?!?
        if (_isMinionSpawnerReady && !_isDead && _minionBurstSpawnManager == null)
        {
            _isMinionSpawnerReady = false;

            //set the manager reference
            _minionBurstSpawnManager = BurstSpawnMinions();

            //Start the spawning!
            StartCoroutine(_minionBurstSpawnManager);
        }
    }

    private IEnumerator BurstSpawnPickups()
    {
        //enter our spawning state
        _isSpawning = true;

        //set the current burst's spawn count target
        int targetSpawnCount = Random.Range(_burstPickupSpawnCountMin, _burstPickupSpawnCountMax+ 1);
        int currentSpawnCount = 0;

        //make sure no one makes any editor mistakes ;)
        if (currentSpawnCount > targetSpawnCount)
            targetSpawnCount = currentSpawnCount + 1;

        //are we below our spawn quota
        while (currentSpawnCount < targetSpawnCount)
        {
            //spawn a pickup
            SpawnPickup();

            //increment our count
            currentSpawnCount++;

            //wait for the delay
            yield return new WaitForSeconds(_burstPickupSpawnDelay);
        }

        //exit the spawn state
        _isSpawning = false;

        //Cooldown the nest's spawning
        Invoke(nameof(ReadyPickupSpawner), _pickupSpawnCooldown);

        //clear the pickup spawn manager utility
        _pickupBurstSpawnManager = null;
    }

    private void ReadyPickupSpawner()
    {
        _isPickupSpawnerReady = true;

        //donw cooling down? spawn more pickups
        StartPickupBurst();
    }

    private void StopMinionSpawning()
    {
        //leave the spawn state
        _isSpawning = false;
        
        //Stop any active spawn manager
        if (_minionBurstSpawnManager != null)
        {
            StopCoroutine(_minionBurstSpawnManager);
            _minionBurstSpawnManager = null;
        }


        //also stop any cooldown calls, just in case
        CancelInvoke(nameof(ReadyMinionSpawner));

        //reset the cooldown state
        _isMinionSpawnerReady = true;
    }

    private void StartPickupBurst()
    {
        //is the spawner ready? Are we not already spawning?
        if (_isPickupSpawnerReady && _pickupBurstSpawnManager == null)
        {
            _isPickupSpawnerReady = false;

            //set the manager reference
            _pickupBurstSpawnManager = BurstSpawnPickups();

            //Start the spawning!
            StartCoroutine(_pickupBurstSpawnManager);
        }
    }





    //Externals
    public int GetBehaviorID()
    {
        return GetInstanceID();
    }

    public Faction GetFaction()
    {
        return _faction;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int GetHealth()
    {
        return _health;
    }

    public InteractableType GetInteractableType()
    {
        return _interactableType;
    }

    public bool IsDead()
    {
        return _isDead;
    }

    public bool IsPickedUp()
    {
        return false;
    }

    public bool IsReadyForPickup()
    {
        return false;
    }

    public int Nutrition()
    {
        return -1;
    }

    public void SetPickupState(bool state)
    {
        //ignore
    }

    public void TakeDamage(ITargetable aggressor, int damage)
    {
        if (!_isDead)
        {
            //damage this nest
            _health -= damage;

            //play the damaged animation
            //...

            if (_health <= 0)
                Die();
        }
    }

    public void ActivateNest()
    {
        if (!_isNestActive && !_isDead)
        {
            _isNestActive = true;

            //start spawning minions!
            StartMinionBurst();
        }
    }

    public void ForceSpawnPickups()
    {
        //Death forces the nest to become a resource point, stops minion spawning, and starts pickup spawns
        Die();
    }

    public bool IsResourcePointAvailable()
    {
        //if the nest is dead, then it's spawning pickups
        return _isDead;
    }
}
