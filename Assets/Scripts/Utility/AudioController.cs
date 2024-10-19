using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;





public enum SoundType
{
    unset,
    MinionCommandFeedback,
    MinionSelectedFeedback
}

public class AudioController : MonoBehaviour
{
    //Declarations
    [SerializeField] private MinionAudioData _minionAudio;
    [SerializeField] private AudioSource _feetAudioSource;
    [SerializeField] private AudioSource _bodyAudioSource;
    [SerializeField] private AudioSource _voiceAudioSource;



    //Monobehaviours




    //Internals
    



    //Externals
    public void PlaySpawnSound()
    {
        AudioClip randomSpawnClip = _minionAudio.OnSpawnedClip();
        if (randomSpawnClip != null)
        {
            _bodyAudioSource.clip = randomSpawnClip;
            _bodyAudioSource.Play();
        }
    }

    public void PlayDeathSound()
    {
        AudioClip randomSpawnClip = _minionAudio.OnDeathClip();
        if (randomSpawnClip != null)
        {
            if (_voiceAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _voiceAudioSource.clip = randomSpawnClip;
            _voiceAudioSource.Play();
        }
    }

    public void PlaySelectedSound()
    {
        AudioClip randomSpawnClip = _minionAudio.OnSelectedClip();
        if (randomSpawnClip != null)
        {
            if (_voiceAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _voiceAudioSource.clip = randomSpawnClip;
            _voiceAudioSource.Play();
        }
    }
    public void PlayCommandedSound()
    {
        AudioClip randomSpawnClip = _minionAudio.OnCommandedClip();
        if (randomSpawnClip != null)
        {
            if (_voiceAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _voiceAudioSource.clip = randomSpawnClip;
            _voiceAudioSource.Play();
        }
    }

    public void PlayAttackingSound()
    {
        AudioClip randomSpawnClip = _minionAudio.OnAttackingClip();
        if (randomSpawnClip != null)
        {
            if (_voiceAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _voiceAudioSource.clip = randomSpawnClip;
            _voiceAudioSource.Play();
        }
    }

    public void PlayDamagedSound()
    {
        AudioClip randomSpawnClip = _minionAudio.OnDamagedClip();
        if (randomSpawnClip != null)
        {
            if (_bodyAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _bodyAudioSource.clip = randomSpawnClip;
            _bodyAudioSource.Play();
        }
    }

    /*
    public void PlayFootstepSound()
    {
        //...
    }
    */




}
