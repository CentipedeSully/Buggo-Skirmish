using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using TMPro;




public class PlayerNestBehavior : MonoBehaviour, ITargetable
{
    //Declarations
    [SerializeField] private int _currentNutrition;
    [SerializeField] private int _eggCost = 100;
    [SerializeField] private float _hatchTime = 3f;
    [SerializeField] private int _eggsIncubating = 0;
    [SerializeField] private GameObject _minionPrefab;
    [SerializeField] private Transform _entitiesContainer;
    [SerializeField] private Faction _faction = Faction.Ally;
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private Transform _minionSpawnPosition;
    [SerializeField] private int _health = 60;
    private bool _isDead;
    private GameManager _gameManager;

    [SerializeField] private MaterialController _materialController;
    [SerializeField] private bool _isHovered = false;
    [SerializeField] private List<MeshRenderer> _nestMeshRenderers = new();
    [SerializeField] private List<Material> _playerNestMaterials = new();
    [SerializeField] private List<Material> _nestKilledMaterials = new();
    [SerializeField] private ParticleSystem _idleParticles;
    [SerializeField] private ParticleSystem _onDamagedParticles;
    [SerializeField] private ParticleSystem _onMinionSpawnedParticles;
    [SerializeField] private ParticleSystem _onPickupDepositParticles;
    [SerializeField] private Animator _animator;




    //Monobehaviours
    private void Start()
    {
        UpdateNestAppearance();
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_isDead) 
        {
            ITargetable behavior = other.GetComponent<ITargetable>();

            if (behavior != null)
            {
                if (behavior.GetInteractableType() == InteractableType.Pickup)
                {
                    //wait for the pickup to be dropped!
                    if (behavior.IsPickedUp() == false)
                    {
                        //increase the nest's nutrition value
                        _currentNutrition += behavior.Nutrition();

                        //destroy the pickup
                        Destroy(other.gameObject);

                        //play the deposit plarticles
                        _onPickupDepositParticles.Play();
                        StopDepositParticlesAfterDelay();

                        //can we create an egg?
                        if (_currentNutrition >= _eggCost)
                        {
                            //enter the incubating animation if we aren't there already
                            if (!_animator.GetBool("isIncubating"))
                                _animator.SetBool("isIncubating", true);

                            int incubations = _currentNutrition / _eggCost;

                            //Begin Incubating as many eggs as possible!
                            while (incubations > 0)
                            {
                                _currentNutrition -= _eggCost;
                                IncubateEgg();
                                incubations--;
                            }
                        }
                    }
                }
            }
        }
    }





    //Internals
    private void UpdateNestAppearance()
    {
        if (!_isDead)
        {
            //reflect the player's color
            foreach (MeshRenderer meshRenderer in _nestMeshRenderers)
                meshRenderer.SetMaterials(_playerNestMaterials);

            //play the idle particles
            _idleParticles.Play();
        }

        else
        {
            //reflect the death color
            foreach (MeshRenderer meshRenderer in _nestMeshRenderers)
                meshRenderer.SetMaterials(_nestKilledMaterials);

            //stop the idle particles
            _idleParticles.Stop();
        }
    }

    private void StopDepositParticlesAfterDelay()
    {
        CancelInvoke(nameof(StopDepositParticles));
        Invoke(nameof(StopDepositParticles), 0.1f);
    }

    private void StopDepositParticles()
    {
        _onPickupDepositParticles.Stop();
        
    }

    private void StopMinionSpawnedParticlesAfterDelay()
    {
        CancelInvoke(nameof(StopMinionSpawnedParticles));
        Invoke(nameof(StopMinionSpawnedParticles), 0.1f);
    }

    private void StopMinionSpawnedParticles()
    {
        _onMinionSpawnedParticles.Stop();
    }

    private void StopDamagedParticlesAfterDelay()
    {
        CancelInvoke(nameof(StopDamagedParticles));
        Invoke(nameof(StopDamagedParticles), 0.1f);
    }

    private void StopDamagedParticles()
    {
        _onDamagedParticles.Stop();

    }

    private void IncubateEgg()
    {
        if (!_isDead)
        {
            _eggsIncubating++;
            Invoke(nameof(HatchMinion), _hatchTime);
        }
        
    }

    private void HatchMinion()
    {
        if (!_isDead)
        {
            //decrement the number of eggs being incubated
            _eggsIncubating--;

            //SpawnMinion
            GameObject newMinionObject = Instantiate(_minionPrefab, _minionSpawnPosition.position, Quaternion.identity, _entitiesContainer);

            //setup the minion's utils
            newMinionObject.GetComponent<AiBehavior>().SetNest(gameObject);

            //play minion spawned particles
            _onMinionSpawnedParticles.Play();
            StopMinionSpawnedParticlesAfterDelay();

            if (_eggsIncubating == 0)
            {
                //exit the incubation state
                _animator.SetBool("isIncubating", false);
            }
        }
        
    }

    private void Die()
    {
        //set the death state
        _isDead = true;
        UpdateNestAppearance();

        if (_eggsIncubating > 0)
        {
            _eggsIncubating = 0;

            //kill any eggs being incubated
            CancelInvoke(nameof(HatchMinion));

            //exit the incubation state
            _animator.SetBool("isIncubating", false);
        }
        
    }

    private void ResetDamagedAnimAfteerDelay()
    {
        CancelInvoke(nameof(ResetDamagedAnim));
        Invoke(nameof(ResetDamagedAnim), .1f);
    }

    private void ResetDamagedAnim()
    {
        _animator.SetBool("isDamaged", false);
    }




    //Externals
    public int GetBehaviorID()
    {
        return GetInstanceID();
    }

    public InteractableType GetInteractableType()
    {
        return InteractableType.Nest;
    }

    public Faction GetFaction()
    {
        return _faction;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public int Nutrition()
    {
        return 0;
    }

    public void SetPickupState(bool state)
    {
        //ignore
    }

    public bool IsPickedUp()
    {
        return false;
    }

    public bool IsReadyForPickup()
    {
        return false;
    }

    public void TakeDamage(ITargetable aggressor, int damage)
    {
        _health -= damage;

        //play damage particles
        _onDamagedParticles.Play();
        StopDamagedParticlesAfterDelay();

        //play animation
        _animator.SetBool("isDamaged", true);
        ResetDamagedAnimAfteerDelay();

        if (_health <= 0)
        {
            Die();
            _gameManager.TriggerGameLose("Home Nest Destroyed");
        }
    }

    public bool IsDead()
    {
        return _isDead;
    }

    public int GetHealth()
    {
        return _health;
    }

    public void SetGameManager(GameManager gm)
    {
        _gameManager = gm;
    }

    public void OnHoverEntered()
    {
        if (!_isHovered)
        {
            _isHovered = true;
            _materialController.ToggleBlinkingVisual(true);
        }

    }

    public void OnHoverExited()
    {
        if (_isHovered)
        {
            _isHovered = false;
            _materialController.ToggleBlinkingVisual(false);
        }
    }

    public bool IsHovered()
    {
        return _isHovered;
    }

    public bool IsSelectable()
    {
        return true;
    }

    public void PlaySound(SoundType type)
    {
        //...
    }
}
