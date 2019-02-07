using UnityEngine;

public class Voice : MonoBehaviour
{
    // Value types
    private bool isActive;
    public int noteNumber;
    public float velocity;
    private float frequency;
    private float gain;
    private float sampleRate;
    private double phase;
    private double increment;

    // Reference types
    private System.Random random = new System.Random();
    private WaveType waveType;

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
    /// Set settings of voice.
    /// </summary>
    /// <param name="waveType">Type of wave.</param>
    private Voice(WaveType waveType)
    {
        this.waveType = waveType;
        noteNumber = -1;
        velocity = 0;
        isActive = false;
    }

    /// <summary>
    /// Set settings by enabling note tone.
    /// </summary>
    /// <param name="noteNumber">Number of music note.</param>
    /// <param name="velocity">Velocity of the playing sound.</param>
    public void NoteOn(int noteNumber, float velocity)
    {
        this.noteNumber = noteNumber;
        this.velocity = velocity;
        this.frequency = Synthesizer.NoteToFrequency(noteNumber);

        phase = 0;
        sampleRate = AudioSettings.outputSampleRate;

        isActive = true;
    }

    /// <summary>
    /// Set settings by disabling note tone.
    /// </summary>
    /// <param name="noteNumber">Number of music note.</param>
    public void NoteOff(int noteNumber)
    {
        if (noteNumber == this.noteNumber)
            isActive = false;
    }

    /// <summary>
    /// Audio becomes writing buffer. 
    /// </summary>
    /// <param name="data">A array with frequencies.</param>
    /// <param name="channels">Audio channels.</param>
    public void WriteAudioBuffer(ref float[] data, int channels)
    {
        // Check if a music tone is active, if not, then quit the function.
        if (!isActive)
            return;

        // Increasing the frequence.
        increment = frequency * 2 * Mathf.PI / sampleRate;

        // Write audio buffer.
        for (int i = 0; i < data.Length; i += channels)
        {
            phase += increment;

            if (phase > (Mathf.PI * 2))
                phase -= Mathf.PI * 2;

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
                        data[i + c] = gain * ((float)random.NextDouble() * 2f) - 1f;
                        break;
                }
            }
        }
    }
}
