using System.Collections.Generic;
using UnityEngine;


public enum CreatureType
{
    unset,
    minion
}

public interface ICreatureBehavior
{
    void ReadCreatureData(CreatureData data);

    void InterruptBehavior();
}


[CreateAssetMenu(fileName = "NewCreatureData", menuName = "Data/Creature")]
public class CreatureData : ScriptableObject
{
    [Header("General Data")]
    [SerializeField] private CreatureType _type = CreatureType.unset;

    [Header("Movement Data")]
    [SerializeField] private float _baseMoveSpeed = 10;
    [SerializeField] private float _baseTurnSpeed = 120;

    [Header("Combat Data")]
    [SerializeField] private int _baseHealth = 5;
    [SerializeField] private int _baseDamage = 1;
    [SerializeField] private float _baseAtkCooldown = .5f;

    [Header("Resource Data")]
    [SerializeField] private int _baseMeatValue = 1;

    [Header("Audio Data")]
    [SerializeField] private List<AudioClip> _spawnSounds;
    [SerializeField] private List<AudioClip> _deathSounds;
    [SerializeField] private List<AudioClip> _selectedSounds;
    [SerializeField] private List<AudioClip> _responseSounds;
    [SerializeField] private List<AudioClip> _hostileResponseSounds;
    [SerializeField] private List<AudioClip> _attackSounds;
    [SerializeField] private List<AudioClip> _damagedSounds;
    [SerializeField] private List<AudioClip> _footstepSounds;
    [SerializeField] private List<AudioClip> _pickupSounds;
    [SerializeField] private List<AudioClip> _dropItemSounds;
    [SerializeField] private List<AudioClip> _bodyFlopSounds;
    [SerializeField] private List<AudioClip> _ambienceSounds;
    [SerializeField] private List<AudioClip> _crunchButcherSounds;
    [SerializeField] private List<AudioClip> _meatButcherSounds;


    #region GeneralUtilities
    public CreatureType GetCreatureType() { return _type; }
    #endregion

    #region MovementUtilities
    public float GetBaseMoveSpeed() { return _baseMoveSpeed; }
    public float GetBaseTurnSpeed() { return _baseTurnSpeed; }
    #endregion

    #region CombatUtilities
    public int GetBaseHealth() { return _baseHealth; }
    public int GetBaseDamage() { return _baseDamage; }
    public float GetBaseAtkCooldown() { return _baseAtkCooldown; }
    #endregion

    #region ResourceUtilities
    public int GetBaseMeatValue() {  return _baseMeatValue; }
    #endregion

    #region AudioUtilities
    private AudioClip GetRandomClip(List<AudioClip> soundList)
    {
        //ignore empty lists
        if (soundList.Count < 1)
            return null;

        //default to the only item in the list if only 1 item is present
        if (soundList.Count == 1)
            return soundList[0];

        //return a random sound from the list
        int randomIndex = Random.Range(0, soundList.Count);
        return soundList[randomIndex];
    }

    public AudioClip GetSpawnAudioClip() { return GetRandomClip(_spawnSounds); }
    public AudioClip GetDeathAudioClip() { return GetRandomClip(_deathSounds); }
    public AudioClip GetSelectedAudioClip() { return GetRandomClip(_selectedSounds); }
    public AudioClip GetResponseAudioClip() { return GetRandomClip(_responseSounds); }
    public AudioClip GetAttackAudioClip() { return GetRandomClip(_attackSounds); }
    public AudioClip GetDamagedAudioClip() { return GetRandomClip(_damagedSounds); }
    public AudioClip GetHostileResponseAudioClip() { return GetRandomClip(_hostileResponseSounds); }
    public AudioClip GetFootstepAudioClip() { return GetRandomClip(_footstepSounds); }
    public AudioClip GetPickingUpAudioClip() { return GetRandomClip(_pickupSounds); }
    public AudioClip GetDroppingAudioClip() { return GetRandomClip(_dropItemSounds); }
    public AudioClip GetBodyFlopAudioClip() { return GetRandomClip(_bodyFlopSounds); }
    public AudioClip GetAmbienceAudioClip() { return GetRandomClip(_ambienceSounds); }
    public AudioClip GetCrunchButcherAudioClip() { return GetRandomClip(_crunchButcherSounds); }
    public AudioClip GetMeatAudioClip() { return GetRandomClip(_meatButcherSounds); }
    #endregion

}
