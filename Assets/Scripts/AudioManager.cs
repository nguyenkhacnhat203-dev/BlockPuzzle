using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{
    [Header("Audio Sources")]
    public AudioSource backgroundMusicSource;
    public AudioSource soundEffectSource;

    [Header("Clips")]
    public AudioClip backgroundMusicClip;
    public List<AudioClip> soundEffectsClips;

    private const string MUSIC_ON_KEY = "MUSIC_ON";
    private const string SOUND_ON_KEY = "SOUND_ON";
    private const string MUSIC_VOL_KEY = "MUSIC_VOLUME";
    private const string SOUND_VOL_KEY = "SOUND_VOLUME";

    private void Start()
    {
        LoadAudioSettings();
        PlayBackgroundMusic();
    }

    #region Play

    public void PlayBackgroundMusic()
    {
        if (backgroundMusicClip == null) return;

        backgroundMusicSource.clip = backgroundMusicClip;
        backgroundMusicSource.loop = true;

        if (!backgroundMusicSource.isPlaying)
            backgroundMusicSource.Play();
    }

    public void PlaySoundEffect(int index)
    {
        if (index < 0 || index >= soundEffectsClips.Count) return;

        soundEffectSource.PlayOneShot(soundEffectsClips[index]);
    }

    public void Btn_Click() => PlaySoundEffect(0);
    public void Btn_Click1() => PlaySoundEffect(1);
    public void DropBlock() => PlaySoundEffect(2);

    public void Reload()
    {
        StartCoroutine(ReloadRoutine());
    }

    private IEnumerator ReloadRoutine()
    {
        if (backgroundMusicSource.isPlaying)
            backgroundMusicSource.Pause();

        if (soundEffectsClips.Count > 3 && soundEffectsClips[3] != null)
        {
            AudioClip reloadClip = soundEffectsClips[3];
            soundEffectSource.PlayOneShot(reloadClip);

            yield return new WaitForSeconds(reloadClip.length);
        }

        bool musicOn = PlayerPrefs.GetInt(MUSIC_ON_KEY, 1) == 1;
        if (musicOn)
        {
            backgroundMusicSource.UnPause();
        }
    }

    #endregion

    #region Volume Control

    public void SetMusicVolume(float volume)
    {
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, volume);
        ApplyMusicVolume();
    }

    public void SetSoundVolume(float volume)
    {
        PlayerPrefs.SetFloat(SOUND_VOL_KEY, volume);
        ApplySoundVolume();
    }

    public void SetMusicOn(bool isOn)
    {
        PlayerPrefs.SetInt(MUSIC_ON_KEY, isOn ? 1 : 0);
        ApplyMusicVolume();
    }

    public void SetSoundOn(bool isOn)
    {
        PlayerPrefs.SetInt(SOUND_ON_KEY, isOn ? 1 : 0);
        ApplySoundVolume();
    }

    private void ApplyMusicVolume()
    {
        bool isOn = PlayerPrefs.GetInt(MUSIC_ON_KEY, 1) == 1;
        float vol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.5f);

        backgroundMusicSource.volume = isOn ? vol : 0f;

        if (!isOn && backgroundMusicSource.isPlaying)
            backgroundMusicSource.Pause();
        else if (isOn && !backgroundMusicSource.isPlaying)
            backgroundMusicSource.Play();
    }

    private void ApplySoundVolume()
    {
        bool isOn = PlayerPrefs.GetInt(SOUND_ON_KEY, 1) == 1;
        float vol = PlayerPrefs.GetFloat(SOUND_VOL_KEY, 0.5f);

        soundEffectSource.volume = isOn ? vol : 0f;
    }

    private void LoadAudioSettings()
    {
        ApplyMusicVolume();
        ApplySoundVolume();
    }

    #endregion
}
