using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteInput : MonoBehaviour
{
    // Value types
    [SerializeField] private int noteStart = 60;
    [SerializeField] private int step = 1;
    [SerializeField] private float deltaTime = 0.5f;

    // Reference types
    private Dictionary<KeyCode, int> virtualKeysDictionary;

    // Events
    public delegate void NoteOnDelegate(int noteNumber, float velocity);
    public event NoteOnDelegate OnNoteOn;
    public event System.Action<int> OnNoteOff;

    /// <summary>
    /// Add keys and frequencies in dictionary.
    /// </summary>
    private void Awake()
    {
        virtualKeysDictionary = new Dictionary<KeyCode, int>
        {
            { KeyCode.A, 51 }, // C
            { KeyCode.W, 52 }, // C#
            { KeyCode.S, 53 }, // D
            { KeyCode.E, 54 }, // D#
            { KeyCode.D, 55 }, // E
            { KeyCode.F, 56 }, // F
            { KeyCode.T, 57 }, // F#
            { KeyCode.G, 58 }, // G
            { KeyCode.Z, 59 }, // G#
            { KeyCode.H, 60 }, // A
            { KeyCode.U, 61 }, // A#
            { KeyCode.J, 62 }, // B
            { KeyCode.K, 63 } // C
        };
    }

    // Use this for initialization
    void Start()
    {
        // Start the playing arpeggiator.
        StartCoroutine(Arpeggiator());
    }

    // Update is called once per frame
    void Update()
    {
        // Enable/Disable note tone by key.
        foreach (KeyCode key in virtualKeysDictionary.Keys)
        {
            if (Input.GetKeyDown(key))
            {
                if (OnNoteOn != null)
                    OnNoteOn(virtualKeysDictionary[key], 1f);
            }
            else if (Input.GetKeyUp(key))
            {
                if (OnNoteOff != null)
                    OnNoteOff(virtualKeysDictionary[key]);
            }
        }
    }

    /// <summary>
    /// Start playing the arpeggiator.
    /// </summary>
    /// <returns>Seconds for waiting.</returns>
    private IEnumerator Arpeggiator()
    {
        // Declare variables
        int noteNumber = noteStart;

        for (; ;)
        {
            if (OnNoteOff != null)
                OnNoteOff(noteNumber);

            if (OnNoteOn != null)
                OnNoteOn(noteNumber += step, 1f);

            yield return new WaitForSeconds(deltaTime);
        }
    }
}
