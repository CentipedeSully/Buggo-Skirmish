using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(fileName = "NewMinionAudio", menuName = "Data/MinionAudio")]
public class MinionAudioData : ScriptableObject
{
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

    public AudioClip GetSpawnAudioClip() {return GetRandomClip(_spawnSounds);}
    public AudioClip GetDeathAudioClip() {return GetRandomClip(_deathSounds); }
    public AudioClip GetSelectedAudioClip() {return GetRandomClip(_selectedSounds); }
    public AudioClip GetResponseAudioClip() {return GetRandomClip(_responseSounds); }
    public AudioClip GetAttackAudioClip() {return GetRandomClip(_attackSounds); }
    public AudioClip GetDamagedAudioClip() {return GetRandomClip(_damagedSounds); }
    public AudioClip GetHostileResponseAudioClip() { return GetRandomClip(_hostileResponseSounds); }
    public AudioClip GetFootstepAudioClip() { return GetRandomClip(_footstepSounds); }
    public AudioClip GetPickingUpAudioClip() { return GetRandomClip(_pickupSounds); }
    public AudioClip GetDroppingAudioClip() { return GetRandomClip(_dropItemSounds); }
    public AudioClip GetBodyFlopAudioClip() { return GetRandomClip(_bodyFlopSounds); }
    public AudioClip GetAmbienceAudioClip() { return GetRandomClip(_ambienceSounds); }
    public AudioClip GetCrunchButcherAudioClip() { return GetRandomClip(_crunchButcherSounds); }
    public AudioClip GetMeatAudioClip() { return GetRandomClip(_meatButcherSounds); }

}
