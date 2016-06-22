using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SoundScheme : MonoBehaviour
{
    [Serializable]
    public class TrackInformation
    {
        public AudioClip clip;
        public AudioClip[] alternatives;
        [Range(0.0f, 1.0f)]
        public float volume = 1.0f;
        public GameEvents.EventType Event = GameEvents.EventType.None;
        public bool isMusic;
        public string args;

        public TrackInformation()
        {
            volume = 1.0f;
        }
    }

    public float masterVolume = 1.0f;

    public TrackInformation[] tracks;

    int prevTracksSize = -1;
    void OnValidate()
    {
        if(tracks.Length > prevTracksSize)
        {
            for (int i = Mathf.Max(prevTracksSize, 0); i < tracks.Length; i++)
            {
                tracks[i].volume = 1.0f;
            }
        }
        prevTracksSize = tracks.Length;
    }
}