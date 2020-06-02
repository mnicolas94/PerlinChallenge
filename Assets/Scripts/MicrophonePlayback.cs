using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MicrophonePlayback : MonoBehaviour
{
    public new AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        string micro = Microphone.devices[0];
        audio.clip = Microphone.Start(micro, true, 10, 44100);
        audio.timeSamples = Microphone.GetPosition(micro);
        audio.loop = true;
        while (!(Microphone.GetPosition(micro) > 0))
        {
        }
        audio.Play();
    }

}
