﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if RTVOICE
using Crosstales.RTVoice.Model;
using Crosstales.RTVoice;
#endif

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField]
    private GameObject 	_audioSourcePrefab 		= null;

    [SerializeField]
    private GameObject  _audioSourceMusicPrefab = null;

	#if RTVOICE
	[Header("Text To Speech (TTS")]
	[SerializeField]
	private GameObject 	_audioSourceTTSPrefab 	= null;
	[SerializeField]
	private Transform	_ttsContainer			= null;
	[SerializeField]
	private Voice 		_speakerVoice;
	#endif

    private List<AudioSource> _currentlyPlaying = new List<AudioSource>();

	public enum AppAudioClip
	{
		Explosion,
		AcquireWeapon,
		AcquireSpeedup,
		AcquireEnlarge,
		Shoot,
        IntroFanfare,
        AsteroidRumble
	}

    private bool _muted = false;
    public bool Muted {
        set {
            _muted = value;
        }
        get {
            return _muted;
        }
    }

    private string ClipPath(AppAudioClip clip) {

        string path = null;

		switch (clip)
		{
			case AppAudioClip.Explosion: 		return "Audio/explosion_player";
			case AppAudioClip.AcquireWeapon: 	return "Audio/pick_up_1";
			case AppAudioClip.AcquireSpeedup: 	return "Audio/pick_up_2";
			case AppAudioClip.AcquireEnlarge: 	return "Audio/pick_up_3";
			case AppAudioClip.Shoot: 			return "Audio/weapon_enemy_quieter";
            case AppAudioClip.IntroFanfare:     return "Audio/fanfare";
            case AppAudioClip.AsteroidRumble:   return "Audio/asteroid_rumble_mid";
        }

		return path;
    }

    protected AudioManager() {}

    private GameObject getAudioSourcePrefab(AppAudioClip clip) {
        //TODO add separate audio source for bg music, or use the same as fanfar
        if (clip == AppAudioClip.IntroFanfare)
            return _audioSourceMusicPrefab;
        else
            return _audioSourcePrefab;
    }

    public void playClip(AppAudioClip clip) {
        if (_muted) {
            Debug.Log("AudioManager Muted, not playing anything");
            return;
        }
		//Debug.Log("AudioManager play clip " + clip);

        string path = ClipPath(clip);
		Debug.Assert(path != null);

		if (path != null) {
			//AudioClip playMe = (AudioClip)Resources.Load(path, typeof(AudioClip));//Resources.Load(path) as AudioClip;
			AudioClip playMe = Resources.Load<AudioClip>(path);
			Debug.Assert(playMe != null);
			if (playMe != null) {
				//Debug.Log("AudioManager clip loaded " + playMe);

				//we have to use separate audiosources per clip, for polyphony
                GameObject audioSource = Instantiate(getAudioSourcePrefab(clip)) as GameObject;
				audioSource.transform.SetParent(this.transform);
				
				
				AudioSource source = audioSource.GetComponent<AudioSource>();
				Debug.Assert(source != null);
				_currentlyPlaying.Add(source);
				
				source.clip = playMe;
				
				source.Play();//or PlayOneShot
				
				//cleanup after finished
				float timeInSecs = playMe.length;
				Debug.Assert (timeInSecs > 0.0f);
				if (timeInSecs <= 0.0f) {
					//some safety programming. important is that the clip get's cleaned up at some point
					Debug.LogWarning("[AudioManager]: clip length reported " + timeInSecs);
					timeInSecs = 10.0f;
				}
				StartCoroutine(cleanUpFinished(audioSource, timeInSecs));
			}
        }

    }

    private IEnumerator cleanUpFinished(GameObject source, float secs) {
        yield return new WaitForSeconds(secs); 
        Destroy(source);
    }

	public void Speak (string speech)
	{
		#if RTVOICE
		// Silence all existing speakers - we are polite no talking on top of
		// our selves
		silence ();

		bool isNative = false;
		float rate = 1.0f;
		float vol = 1.0f;
		float pitch = 1.0f;
		if (isNative)
		{
			//Speaker.SpeakNative(speech, _speakerVoice, rate, vol, pitch);
		}
		else
		{
			// Create a new speaker
			GameObject audioSource = Instantiate (_audioSourceTTSPrefab, _ttsContainer) as GameObject;
			AudioSource source = audioSource.GetComponent<AudioSource> ();
			Speaker.Speak (speech, source, _speakerVoice, true, rate, vol, "", pitch);
		}
		#else
		Debug.LogWarning ("AudioManager Speak: RTVoice is not in use");
		#endif
	}

	public void Silence () 
	{
		#if RTVOICE
		Speaker.Silence ();
		_ttsContainer.DestroyChildren ();
		#else
		Debug.LogWarning ("AudioManager Silence: RTVoice is not in use");
		#endif
	}
}
