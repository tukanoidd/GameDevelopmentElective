using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sounds[] sounds;
    // Start is called before the first frame update
    void Awake()
    {
        foreach (Sounds s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
        }
    }

    // Update is called once per frame
    public void Play (string name)
    {
        Sounds s = Array.Find(sounds, Sounds => Sounds.name == name);
        s.source.Play();
    }
}
