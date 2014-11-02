using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour 
{
    [SerializeField] private AudioClip [] clips;
    private List<AudioSource> audioList = new List<AudioSource>();
    private Dictionary<string, AudioClip> clipDict = new Dictionary<string, AudioClip>();
    private static AudioManager instance;
    public static AudioManager Instance {
        get {
            return instance;
        }
    }
    void Awake() 
    {
        if (instance == null)
        {
            instance = this;
        }
        for (int i = 0; i < 16; i++)
        {
            var audio = gameObject.AddComponent<AudioSource>();
            audioList.Add(audio);
        }
        foreach(var clip in clips)
        {
            clipDict.Add(clip.name, clip);
        }
    }

    public void PlayAudio(string clipName, float volume = 1.0f, float pitch = 1.0f, bool loop = false, bool forcePlay = false) 
    {
        
        if (string.IsNullOrEmpty(clipName))
        {
            return;
        }
        
        if (clipDict.ContainsKey(clipName) == false)
        {
            return;
        }
        AudioSource audioSource = GetAudioSource(forcePlay);
        if (audioSource == null)
        {
            return;
        }
        var audioClip = clipDict[clipName];
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.pitch = pitch;
        audioSource.Play();
    }
    public void StopAllAudio() 
    {
        foreach (var audio in audioList)
        {
            audio.Stop();
        }
    }
    private AudioSource GetAudioSource(bool forcePlay)
    {
        AudioSource audio = null;
        int i = 0;     
        for (; i < audioList.Count; i++)
        {
            audio = audioList[i];
            if (!audio.isPlaying)
            {
                break;
            }
        }
        if (i == audioList.Count)
        {
            if (forcePlay)
            {
                var finalAudio = audioList[0];
                PushAudioSourceToEnd(0);
                return finalAudio;
            }
            return null;
        }
        PushAudioSourceToEnd(i);
        return audio;
    }
    private void PushAudioSourceToEnd(int index)
    {
        var audio = audioList[index];
        audioList.RemoveAt(index);
        audioList.Add(audio);
    }
    void OnDestroy() 
    {
        instance = null;
    }
}
