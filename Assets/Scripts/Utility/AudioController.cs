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
    [SerializeField] private AudioSource _ambienceAudioSource;
    [SerializeField] private float _randomizedPitch;



    //Monobehaviours
    private void Start()
    {
        _randomizedPitch = Random.Range(.9f, 1.1f);
        _feetAudioSource.pitch = _randomizedPitch;
        _bodyAudioSource.pitch = _randomizedPitch;
        _voiceAudioSource.pitch = _randomizedPitch;
        _ambienceAudioSource.pitch = _randomizedPitch;
    }



    //Internals




    //Externals
    public void PlaySpawnSound()
    {
        AudioClip clip = _minionAudio.GetSpawnAudioClip();

        if (clip != null)
        {
            _bodyAudioSource.PlayOneShot(clip);
        }
    }
    public void PlayDeathSound()
    {
        AudioClip clip = _minionAudio.GetDeathAudioClip();

        if (clip != null)
        {
            if (_voiceAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _voiceAudioSource.clip = clip;
            _voiceAudioSource.Play();
        }
    }
    public void PlaySelectedSound()
    {
        AudioClip clip = _minionAudio.GetSelectedAudioClip();

        if (clip != null)
        {
            if (_voiceAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _voiceAudioSource.clip = clip;
            _voiceAudioSource.Play();
        }
    }
    public void PlayResponseSound()
    {
        AudioClip clip = _minionAudio.GetResponseAudioClip();

        if (clip != null)
        {
            if (_voiceAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _voiceAudioSource.clip = clip;
            _voiceAudioSource.Play();
        }
    }
    public void PlayHostileResponseSound()
    {
        AudioClip clip = _minionAudio.GetHostileResponseAudioClip();

        if (clip != null)
        {
            if (_voiceAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _voiceAudioSource.clip = clip;
            _voiceAudioSource.Play();
        }
    }
    public void PlayAttackingSound()
    {
        AudioClip clip = _minionAudio.GetAttackAudioClip();

        if (clip != null)
        {
            if (_voiceAudioSource.isPlaying)
                _voiceAudioSource.Stop();

            _voiceAudioSource.clip = clip;
            _voiceAudioSource.Play();
        }
    }
    public void PlayDamagedSound()
    {
        AudioClip clip = _minionAudio.GetDamagedAudioClip();

        if (clip != null)
        {
            _bodyAudioSource.PlayOneShot(clip);
        }
    }
    public void PlayFootstepSound()
    {
        AudioClip clip = _minionAudio.GetFootstepAudioClip();

        if (clip != null)
        {
            _feetAudioSource.PlayOneShot(clip);
        }
    }
    public void PlayAmbienceSound()
    {
        AudioClip clip = _minionAudio.GetAmbienceAudioClip();

        if (clip != null)
        {
            _ambienceAudioSource.PlayOneShot(clip);
        }
    }

    public void PlayPickupSound()
    {
        AudioClip clip = _minionAudio.GetPickingUpAudioClip();

        if (clip != null)
        {
            _voiceAudioSource.PlayOneShot(clip);
        }
    }

    public void PlayBodyFlopSound()
    {
        AudioClip clip = _minionAudio.GetBodyFlopAudioClip();

        if (clip != null)
        {
            _bodyAudioSource.PlayOneShot(clip);
        }
    }




}
