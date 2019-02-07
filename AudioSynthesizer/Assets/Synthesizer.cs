using UnityEngine;

public class Synthesizer : MonoBehaviour
{
    // Constants
    private const int fixedNoteNumber = 69;
    private const float fixedFrequency = 440f;

    // Value types
    private bool isActive;
    private int activeNoteNumber = -1;
    private float sampleRate;
    [SerializeField, Range(220f, 880f)] private float frequency;
    [SerializeField, Range(0f, 1f)] private float gain;
    private double phase;
    private double increment;

    // Reference types
    [SerializeField] private WaveType waveType = WaveType.Sine;
    private System.Random random = new System.Random();
    private NoteInput noteInputScript;

    /// <summary>
    /// Various wave types for audio. 
    /// </summary>
    private enum WaveType
    {
        Sine,
        Square,
        Triangle,
        Sawtooth,
        Noise
    }

    /// <summary>
    /// Initialize variables.
    /// </summary>
    private void Awake()
    {
        // Initialize the sample rate setting from unity in variable.
        sampleRate = (float)AudioSettings.outputSampleRate;

        // Get the 'NoteInput'-Script.
        noteInputScript = GetComponent<NoteInput>();
    }

    /// <summary>
    /// Activate note tone.
    /// </summary>
    private void OnEnable()
    {
        if (noteInputScript != null)
        {
            noteInputScript.OnNoteOn -= Input_OnNoteOn;
            noteInputScript.OnNoteOn += Input_OnNoteOn;

            noteInputScript.OnNoteOff -= Input_OnNoteOff;
            noteInputScript.OnNoteOff += Input_OnNoteOff;
        }
    }

    /// <summary>
    /// Deactivate note tone.
    /// </summary>
    private void OnDisable()
    {
        if (noteInputScript != null)
        {
            noteInputScript.OnNoteOn -= Input_OnNoteOn;

            noteInputScript.OnNoteOff -= Input_OnNoteOff;
        }
    }

    /// <summary>
    /// Read the filter from audio source.
    /// </summary>
    /// <param name="data">A array with frequencies.</param>
    /// <param name="channels">Audio channels.</param>
    private void OnAudioFilterRead(float[] data, int channels)
    {
        Debug.LogFormat("Buffer size: {0} | Channel count: {1}", data.Length / channels, channels);

        // Reset the frequencies.
        for (int i = 0; i < data.Length; i++)
            data[i] = 0f;

        // Increasing the frequence.
        increment = (frequency * 2) * (Mathf.PI / sampleRate);

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += increment;

            if (phase > (Mathf.PI * 2))
                phase -= Mathf.PI * 2;

            // Come out from the playable frequence.
            if (!isActive)
                return;

            // Play the frequence by wave.
            for (int c = 0; c < channels; c++)
            {
                switch (waveType)
                {
                    case WaveType.Sine:
                        data[i + c] = gain * Mathf.Sin((float)phase);
                        break;
                    case WaveType.Square:
                        data[i + c] = Mathf.Sin((float)phase) >= 0 ? gain : -gain;
                        break;
                    case WaveType.Triangle:
                        // 2 * abs((frequency * time + phase) - 2 * round((frequency * time + phase) / 2) - 1 ) - 1
                        // data[i + c] = 2f * (float)Mathf.Abs(Mathf.Round((float)(frequency * Mathf.PI + phase)));
                        data[i + c] = gain * Mathf.PingPong((float)phase, 1f) * 2f - 1f;
                        break;
                    case WaveType.Sawtooth:
                        // 2 * ((frequency * time + phase) - round((frequency * time + phase) + 1 / 2))
                        // data[i + c] = data[i + c] >= 1f ? -1f : 2f * (float)Mathf.Abs(Mathf.Round((float)(frequency * Mathf.PI + phase))) + 1 / 2;
                        data[i + c] = gain * Mathf.InverseLerp(0f, Mathf.PI * 2f, (float)phase);
                        break;
                    case WaveType.Noise:
                        data[i + c] = gain * ((float)random.NextDouble() * 2f) -1f;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Play note tone.
    /// </summary>
    /// <param name="noteNumber">Number of music note.</param>
    /// <param name="velocity">Velocity of the playing sound.</param>
    private void Input_OnNoteOn(int noteNumber, float velocity)
    {
        Debug.LogFormat("NoteOn: {0}", noteNumber);
        frequency = NoteToFrequency(noteNumber);
        activeNoteNumber = noteNumber;
        isActive = true;
    }

    /// <summary>
    /// Play off note tone.
    /// </summary>
    /// <param name="noteNumber">Number of music tone.</param>
    private void Input_OnNoteOff(int noteNumber)
    {
        Debug.LogFormat("NoteOff: {0}", noteNumber);
        if (noteNumber == activeNoteNumber)
            isActive = false;
    }

    /// <summary>
    /// Convert note to frequence.
    /// </summary>
    /// <param name="noteNumber">The frequence of note.</param>
    /// <returns>The playable frequence.</returns>
    public static float NoteToFrequency(int noteNumber)
    {
        // Declare variables
        float twelfthRoot = Mathf.Pow(2f, (1f / 12f));

        return fixedFrequency * Mathf.Pow(twelfthRoot, noteNumber - fixedNoteNumber);
    }
}
