using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(fileName = "NewMinionAudio", menuName = "Data/MinionAudio")]
public class MinionAudioData : ScriptableObject
{
    [SerializeField] private List<AudioClip> _onSpawnSounds;
    [SerializeField] private List<AudioClip> _onDeathSounds;
    [SerializeField] private List<AudioClip> _onSelectedSounds;
    [SerializeField] private List<AudioClip> _onCommandSounds;
    [SerializeField] private List<AudioClip> _onAttackingSounds;
    [SerializeField] private List<AudioClip> _onDamagedSounds;

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

    public AudioClip OnSpawnedClip() {return GetRandomClip(_onSpawnSounds);}
    public AudioClip OnDeathClip() {return GetRandomClip(_onDeathSounds); }
    public AudioClip OnSelectedClip() {return GetRandomClip(_onSelectedSounds); }
    public AudioClip OnCommandedClip() {return GetRandomClip(_onCommandSounds); }
    public AudioClip OnAttackingClip() {return GetRandomClip(_onAttackingSounds); }
    public AudioClip OnDamagedClip() {return GetRandomClip(_onDamagedSounds); }

}
