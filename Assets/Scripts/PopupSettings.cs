using UnityEngine;
using UnityEngine.UI;

public class PopupSettings : PopupBase
{
    [Header("Icons X")]
    public Image x_Sound;
    public Image x_Music;
    public Image x_Phone;

    [Header("Sliders (Optional)")]
    public Slider soundSlider;
    public Slider musicSlider;

    private bool isSoundOn;
    private bool isMusicOn;
    private bool isPhoneOn;

    private const string SOUND_KEY = "SOUND_ON";
    private const string MUSIC_KEY = "MUSIC_ON";
    private const string PHONE_KEY = "PHONE_ON";
    private const string SOUND_VOL_KEY = "SOUND_VOLUME";
    private const string MUSIC_VOL_KEY = "MUSIC_VOLUME";

    private void OnEnable()
    {
        LoadSettings();
        ApplySettings();
    }

    #region Toggle Buttons

    public void ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt(SOUND_KEY, isSoundOn ? 1 : 0);

        AudioManager.Instance.Btn_Click1();
        AudioManager.Instance.SetSoundOn(isSoundOn);

        x_Sound.gameObject.SetActive(!isSoundOn);
    }

    public void ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt(MUSIC_KEY, isMusicOn ? 1 : 0);

        AudioManager.Instance.Btn_Click1();
        AudioManager.Instance.SetMusicOn(isMusicOn);

        x_Music.gameObject.SetActive(!isMusicOn);
    }

    public void TogglePhone()
    {
        isPhoneOn = !isPhoneOn;
        PlayerPrefs.SetInt(PHONE_KEY, isPhoneOn ? 1 : 0);

        AudioManager.Instance.Btn_Click1();
        x_Phone.gameObject.SetActive(!isPhoneOn);
    }

    #endregion

    #region Slider Callbacks

    public void OnSoundSliderChanged(float value)
    {
        AudioManager.Instance.SetSoundVolume(value);
    }

    public void OnMusicSliderChanged(float value)
    {
        AudioManager.Instance.SetMusicVolume(value);
    }

    #endregion

    #region Load & Apply

    private void LoadSettings()
    {
        isSoundOn = PlayerPrefs.GetInt(SOUND_KEY, 1) == 1;
        isMusicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        isPhoneOn = PlayerPrefs.GetInt(PHONE_KEY, 1) == 1;
    }

    private void ApplySettings()
    {
        float soundVol = PlayerPrefs.GetFloat(SOUND_VOL_KEY, 0.7f);
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0.7f);

        if (soundSlider != null) soundSlider.value = soundVol;
        if (musicSlider != null) musicSlider.value = musicVol;

        AudioManager.Instance.SetSoundOn(isSoundOn);
        AudioManager.Instance.SetMusicOn(isMusicOn);

        x_Sound.gameObject.SetActive(!isSoundOn);
        x_Music.gameObject.SetActive(!isMusicOn);
        x_Phone.gameObject.SetActive(!isPhoneOn);
    }

    #endregion

    public void OnClickNewGame()
    {
        AdManager.Instance.ShowRewarded(() =>
        {
            if (UiGame.Instance != null)
            {
                UiGame.Instance.ResetScore();
            }

            Board board = FindObjectOfType<Board>();
            if (board != null)
            {
                board.ResetGameFromPopup();
            }

            PopupManager.Instance.isShowPopup = false;
            Destroy(gameObject);
        });
    }

  


}
