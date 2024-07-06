using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePlayer : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField] private float frequency = 261;
    [SerializeField] private int midiNumber = 3;

    AudioClip[] notes;

    private void Awake()
    {
        notes = new AudioClip[84 - 48 + 1];
        for (int i = 48; i <= 84; i++)
        {
            notes[i - 48] = CreateSineWaveAudioClip(MidiToFrequency(i), 0.5f);
        }
    }

    public static float MidiToFrequency(int midiNumber)
    {
        return 440.0f * Mathf.Pow(2.0f, (midiNumber - 69) / 12.0f);
    }

    private void OnValidate()
    {
        frequency = MidiToFrequency(midiNumber);
        /*if (notes != null)
            PlayNote(midiNumber);*/
    }

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        StartCoroutine(CogManager.Instance.AddPulse((counter) => { audioSource.PlayOneShot(notes[60 - 48]); return true; }, 0.8f, -1));
        StartCoroutine(CogManager.Instance.AddPulse((counter) => { audioSource.PlayOneShot(notes[65 - 48]); return true; }, 0.6f, -1));
    }

    private void Pulse()
    {
        PlayNote(frequency, 0.5f);
    }

    public void PlayNote(float frequency, float duration)
    {
        AudioClip audioClip = CreateSineWaveAudioClip(frequency, duration);
        audioSource.PlayOneShot(audioClip);
    }

    public void PlayNote(int midiNumber)
    {
        if (48 <= midiNumber && midiNumber <= 84)
            audioSource.PlayOneShot(notes[midiNumber - 48]);
    }

    public AudioClip CreateLoopedSineWaveClip(float frequency)
    {
        AudioClip sineWaveClip;
        int sampleRate = 44100;
        int sampleLength = sampleRate; // 1 second of samples for looping
        float[] samples = new float[sampleLength];

        for (int i = 0; i < sampleLength; i++)
        {
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
        }

        sineWaveClip = AudioClip.Create("SineWave", sampleLength, 1, sampleRate, false);
        sineWaveClip.SetData(samples, 0);

        return sineWaveClip;
    }

    private AudioClip CreateSineWaveAudioClip(float frequency, float duration)
    {
        int sampleRate = 44100;
        int sampleLength = Mathf.CeilToInt(sampleRate * duration);
        float[] samples = new float[sampleLength];

        // ADSR envelope parameters
        float attackTime = 0.1f; // in seconds
        float decayTime = 0.1f;  // in seconds
        float sustainLevel = 0.7f; // sustain level (0.0 to 1.0)
        float releaseTime = 0.2f; // in seconds

        int attackSamples = Mathf.CeilToInt(sampleRate * attackTime);
        int decaySamples = Mathf.CeilToInt(sampleRate * decayTime);
        int sustainSamples = sampleLength - attackSamples - decaySamples - Mathf.CeilToInt(sampleRate * releaseTime);
        int releaseSamples = Mathf.CeilToInt(sampleRate * releaseTime);

        for (int i = 0; i < sampleLength; i++)
        {
            float amplitude = 1.0f;

            // Apply ADSR envelope
            if (i < attackSamples) // Attack
            {
                amplitude = (float)i / attackSamples;
            }
            else if (i < attackSamples + decaySamples) // Decay
            {
                amplitude = 1.0f - (1.0f - sustainLevel) * (float)(i - attackSamples) / decaySamples;
            }
            else if (i < attackSamples + decaySamples + sustainSamples) // Sustain
            {
                amplitude = sustainLevel;
            }
            else if (i < sampleLength) // Release
            {
                amplitude = sustainLevel * (1.0f - (float)(i - attackSamples - decaySamples - sustainSamples) / releaseSamples);
            }

            samples[i] = amplitude * Mathf.Sin(2 * Mathf.PI * frequency * i / sampleRate);
        }

        AudioClip audioClip = AudioClip.Create("SineWave", sampleLength, 1, sampleRate, false);
        audioClip.SetData(samples, 0);

        return audioClip;
    }
}