using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundSchemePlayback : MonoBehaviour
{
    private Transform soundsRoot;

    SoundScheme scheme;

    AudioSource[] musicPlayer = new AudioSource[2];
    float[] musicPlayerTargetVolume = new float[2];
    int currentMusicPlayer = 0;
    float currentMusicPlayerDefaultVolume = 0.0f;

    public static SoundSchemePlayback Initialize(Transform root)
    {
        SoundSchemePlayback already = root.GetComponentInChildren<SoundSchemePlayback>();
        if (already != null)
            return already;

        GameObject obj = new GameObject();
        obj.name = "SoundsPlayback";
        obj.transform.SetParent(root);
        return obj.AddComponent<SoundSchemePlayback>();
    }

    void Start()
    {
        _Init();
    }

    bool _inited = false;
    void _Init()
    {
        if (_inited) return;

        _inited = true;

        soundsRoot = transform.Find("Sounds");
        if (soundsRoot == null)
        {
            GameObject obj = new GameObject();
            obj.name = "Sounds";
            obj.transform.parent = transform;
            soundsRoot = obj.transform;
        }
        else
        {
            Widget.DeleteAllChildren(soundsRoot);
        }

        // add music players

        for (int i = 0; i < musicPlayer.Length; i++)
        {
            GameObject music = new GameObject();
            musicPlayer[i] = music.AddComponent<AudioSource>();
            music.name = "MusicPlayer" + (i+1).ToString();
            music.transform.SetParent(soundsRoot.transform);
            musicPlayer[i].volume = 0.0f;
            musicPlayerTargetVolume[i] = 0.0f;
            musicPlayer[i].playOnAwake = false;
        }

        scheme = GameObject.Instantiate<SoundScheme>(Desktop.main.theme.soundScheme);
        scheme.transform.SetParent(soundsRoot);

        GameEvents.OnGameEventTriggered += OnGameEvent;
    }

    void OnGameEvent(GameEvents.Event ev)
    {
        foreach(SoundScheme.TrackInformation ti in scheme.tracks)
        {
            if(ti.Event == ev.type)
            {
                if(ti.args != null && ti.args.Length > 0)
                {
                    if(ti.args == ev.args)
                        PlayTrack(ti);
                }
                else
                    PlayTrack(ti);
            }
        }
    }

    void PlayTrack(SoundScheme.TrackInformation ti)
    {
        int clipsCount = 1;
        if (ti.clip == null) clipsCount = 0;
        if (ti.alternatives != null) clipsCount += ti.alternatives.Length;

        if(clipsCount == 0) return;

        int clipIndex = 0;
        if (clipsCount > 1)
            clipIndex = Random.Range(0, 100) % clipsCount;

        AudioClip clip = clipIndex == 0 ? ti.clip : ti.alternatives[clipIndex - 1];

        if (ti.isMusic == false)
            PlaySfx(clip, ti.volume * scheme.masterVolume);
        else
            PlayMusic(clip, ti.volume * scheme.masterVolume);
    }

    void PlaySfx(AudioClip clip, float volume)
    {
        if(GameSession.GetSettingForSoundEffects())
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volume);
    }

    bool IsMusicPlaying(AudioClip clip)
    {
        for(int i = 0; i < musicPlayer.Length; i++)
        {
            if (musicPlayer[i].isPlaying && musicPlayerTargetVolume[i] > 0.0f && musicPlayer[i].clip == clip) return true;
        }

        return false;
    }

    void PlayMusic(AudioClip clip, float volume)
    {
        if (IsMusicPlaying(clip)) return;

        for(int i = 0; i < musicPlayer.Length; i++)
        {
            if(musicPlayer[i].isPlaying == false) // is free
            {
                currentMusicPlayer = i;
                currentMusicPlayerDefaultVolume = volume;
                musicPlayer[i].clip = clip;
                musicPlayer[i].loop = true;
                musicPlayerTargetVolume[i] = volume;
                musicPlayerTargetVolume[(i + 1) % musicPlayer.Length] = 0.0f; // stop the other
                break;
            }
        }
    }

    void Update()
    {
        for (int k = 0; k < 2; k++)
        {
            if (musicPlayer[k].isPlaying == false && musicPlayerTargetVolume[k] > 0.0f)
            {
                musicPlayer[k].Play();
            }

            if (musicPlayer[k].isPlaying)
            {
                if(Mathf.Abs(musicPlayerTargetVolume[k] - musicPlayer[k].volume) < 0.05f)
                {
                    musicPlayer[k].volume = musicPlayerTargetVolume[k];
                    if(musicPlayerTargetVolume[k] < 0.01f)
                        musicPlayer[k].Stop();
                }
                else
                {
                    musicPlayer[k].volume = Mathf.Lerp(musicPlayer[k].volume, musicPlayerTargetVolume[k], Time.deltaTime * 8.0f);
                }
            }
        }
    }

    public void UpdateFromSettings()
    {
        if(GameSession.GetSettingForMusic() == false)
        {
            musicPlayerTargetVolume[currentMusicPlayer] = 0.0f;
        }
    }

    bool musicIsPaused = false;

    public void PauseMusic()
    {
        if (musicIsPaused) return;

        musicPlayerTargetVolume[currentMusicPlayer] = 0.0f;
        musicIsPaused = true;
    }

    public void ResumeMusic()
    {
        if (musicIsPaused == false) return;

        musicPlayerTargetVolume[currentMusicPlayer] = currentMusicPlayerDefaultVolume;
        musicIsPaused = false;
    }

    public void SetMusicPlaybackPitch(float pitch)
    {
        musicPlayer[currentMusicPlayer].pitch = pitch;

        Debug.Log("current music pitch is " + pitch.ToString());
    }

    public void SetMusicVolume(float volume)
    {
        musicPlayerTargetVolume[currentMusicPlayer] = currentMusicPlayerDefaultVolume * volume;
    }

    public void SetMusicVolumeLow()
    {
        SetMusicVolume(0.15f);
    }
    public void SetMusicDefaultVolume()
    {
        SetMusicVolume(1.0f);
    }
}
