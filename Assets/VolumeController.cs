using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private TextMeshProUGUI volumeText;
    [SerializeField] private AudioMixer audioMixer;
    private const string VolumePrefKey = "MasterVolume";

    private void Start()
    {
        // Đọc volume từ PlayerPrefs (nếu có)
        float savedVolume = PlayerPrefs.GetFloat(VolumePrefKey, 1f);
        volumeSlider.value = savedVolume;
        SetVolume(savedVolume);

        // Gắn sự kiện khi thay đổi slider
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        // Set volume vào AudioMixer (log scale)
        float dB = Mathf.Log10(value) * 20;
        audioMixer.SetFloat("MasterVolume", dB);

        // Cập nhật UI
        int percent = Mathf.RoundToInt(value * 100);
        volumeText.text = percent + "%";

        // Lưu lại vào PlayerPrefs
        PlayerPrefs.SetFloat(VolumePrefKey, value);
    }

    private void ApplyVolume(float value)
    {
        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MasterVolume", dB);

        Debug.Log("Setting volume to: " + value + " → " + dB + " dB");

        if (volumeText != null)
            volumeText.text = Mathf.RoundToInt(value * 100) + "%";

        PlayerPrefs.SetFloat("MasterVolume", value);
    }
}
