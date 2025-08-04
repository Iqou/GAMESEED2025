using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeManager : MonoBehaviour
{
    public AudioMixer mainMixer;
    private string backsoundVolumeParam = "BacksoundVolume";
    private string voiceVolumeParam = "VoiceVolume";
    private string songVolumeParam = "SongVolume";
    
    public Slider backsoundSlider;
    public Slider voiceSlider;
    public Slider songSlider;

    void Start()
    {
        // PENTING: Atur nilai awal slider di sini
        if (backsoundSlider != null) backsoundSlider.value = 1f;
        if (voiceSlider != null) voiceSlider.value = 1f;
        if (songSlider != null) songSlider.value = 1f;

        // Panggil metode ini untuk mengatur nilai slider saat scene dimulai
        SetInitialSliderValues();
    }

    private void SetInitialSliderValues()
    {
        float backsoundVol;
        if (backsoundSlider != null && mainMixer != null && mainMixer.GetFloat(backsoundVolumeParam, out backsoundVol))
        {
            backsoundSlider.value = Mathf.InverseLerp(-80f, 0f, backsoundVol);
        }
        
        float voiceVol;
        if (voiceSlider != null && mainMixer != null && mainMixer.GetFloat(voiceVolumeParam, out voiceVol))
        {
            voiceSlider.value = Mathf.InverseLerp(-80f, 0f, voiceVol);
        }
        
        float songVol;
        if (songSlider != null && mainMixer != null && mainMixer.GetFloat(songVolumeParam, out songVol))
        {
            songSlider.value = Mathf.InverseLerp(-80f, 0f, songVol);
        }
    }

    public void SetBacksoundVolume(float value)
    {
        float volume = Mathf.Lerp(-80f, 0f, value);
        if (mainMixer != null) mainMixer.SetFloat(backsoundVolumeParam, volume);
    }

    public void SetVoiceVolume(float value)
    {
        float volume = Mathf.Lerp(-80f, 0f, value);
        if (mainMixer != null) mainMixer.SetFloat(voiceVolumeParam, volume);
    }

    public void SetSongVolume(float value)
    {
        float volume = Mathf.Lerp(-80f, 0f, value);
        if (mainMixer != null) mainMixer.SetFloat(songVolumeParam, volume);
    }
}