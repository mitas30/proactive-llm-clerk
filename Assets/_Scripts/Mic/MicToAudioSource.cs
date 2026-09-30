using UnityEngine;
using System.Linq;

[RequireComponent(typeof(AudioSource))]
class MicToAudioSource : MonoBehaviour
{
    static readonly int SAMPLE_RATE = 48000;

    static readonly float MOVING_AVE_TIME = 0.05f;

    static readonly int MOVING_AVE_SAMPLE = (int)(SAMPLE_RATE * MOVING_AVE_TIME);

    AudioSource micAS = null;

    private float _now_dB;
    public float now_dB { get { return _now_dB; } }

    private void OnEnable()
    {
        AudioSettings.OnAudioConfigurationChanged += OnListenAudioConfigurationChanged;
    }

    private void OnDisable()
    {
        AudioSettings.OnAudioConfigurationChanged -= OnListenAudioConfigurationChanged;
        Microphone.End(Microphone.devices[0]);
    }

    private void Awake()
    {
        micAS = GetComponent<AudioSource>();
    }

    void Start()
    {
        MicStart();
    }

    private void OnListenAudioConfigurationChanged(bool deviceWasChanged) { MicStart(); }

    public void MicStart()
    {

        if (Microphone.devices.Length == 0)
        {
            return;
        }


        micAS.clip = Microphone.Start(null, true, 1, SAMPLE_RATE);
        while (!(Microphone.GetPosition("") > 0)) { }

        micAS.Play();
    }

    void Update()
    {
        if (true || micAS.isPlaying)
        {
            float[] data = new float[MOVING_AVE_SAMPLE];

            micAS.GetOutputData(data, 0);

            float aveAmp = data.Average(s => Mathf.Abs(s));

            float dB = 20.0f * Mathf.Log10(aveAmp);

            _now_dB = dB;

        }
    }
}