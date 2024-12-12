using System.Collections; 
using System.Collections.Generic; 
using UnityEngine; 
using UnityEngine.UI;

public class Music : MonoBehaviour { 

public Slider musicSlider;

public GameObject musicObject;
private AudioSource AudioSource;

private float MusicVolume = 1f;

private void Start()
{
    musicObject = GameObject.FindWithTag("GameMusic");
    AudioSource = musicObject.GetComponent<AudioSource>();
    
    MusicVolume = PlayerPrefs.GetFloat("Volume");
    AudioSource.volume = MusicVolume;
    musicSlider.value = MusicVolume;
}

private void Update()
{
    AudioSource.volume = MusicVolume;
    PlayerPrefs.SetFloat("Volume", MusicVolume);
}

public void AdjustMusic(float volume)
{
    MusicVolume = volume;
}

public void MusicReset()
{
    PlayerPrefs.DeleteKey("Volume");
    AudioSource.volume = 1;
    musicSlider.value = 1;
}
}