using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Microsoft.CognitiveServices.Speech;

/// <summary>
/// Azure TTS の機能を管理するクラスです。
/// テキストを音声に変換し AudioSource で再生します。
/// </summary>
public class AzureTTSManager : MonoBehaviour
{
    [SerializeField] string SubscriptionKey;
    [SerializeField] string Region;

    /// <summary>
    /// 音声を再生するための AudioSource コンポーネントです。
    /// </summary>
    public AudioSource audioSource;

    const int SampleRate = 24000;
    SpeechSynthesizer synthesizer;

    /// <summary>
    /// 指定したテキストを音声合成し、AudioSource で再生します。
    /// </summary>
    /// <param name="text">音声合成するテキスト</param>
    public async Task TextToSpeech(string text)
    {
        var startTime = DateTime.Now;
        // ? @は、文字列リテラル内でエスケープシーケンス(\nice なら改行+iceとならないようにする)を無効にするためのプレフィックス pythonのr文字列と同じ
        // ? @をつけると、改行を含む複数行の文字列をそのまま書けるので、見たままのSSMLを書きやすくなる
        string ssmlText = $@"
        <speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis'
               xmlns:mstts='http://www.w3.org/2001/mstts' xml:lang='ja-JP'>
          <voice name='ja-JP-NanamiNeural'>
            <mstts:express-as style='cheerful'>
              {text}
            </mstts:express-as>
          </voice>
        </speak>";

        // ? using は、using(obj){..}において、{}で定義された処理を抜けるときにobj.Dispose()を呼び出す構文
        // ? try{}
        // ? finally{obj.Dispose();}と同じ意味
        using (var result = await synthesizer.StartSpeakingSsmlAsync(ssmlText))
        {
            var audioDataStream = AudioDataStream.FromResult(result);

            // 1. 全音声データをメモリ上のリストに溜め込む
            var audioBuffer = new List<byte>();
            var buffer = new byte[1024];
            uint bytesRead;
            while ((bytesRead = audioDataStream.ReadData(buffer)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    audioBuffer.Add(buffer[i]);
                }
            }

            if (audioBuffer.Count == 0) return;

            // 溜め込んだbyteデータをfloat配列に変換
            var floatData = new float[audioBuffer.Count / 2];
            for (int i = 0; i < floatData.Length; i++)
            {
                short sample = (short)(audioBuffer[i * 2 + 1] << 8 | audioBuffer[i * 2]);
                floatData[i] = sample / 32768.0f;
            }

            // 2. 非ストリーミングでAudioClipを作成
            var audioClip = AudioClip.Create(
                "Speech",
                floatData.Length, // サンプル数
                1,
                SampleRate,
                false // streamをfalseに
            );

            // 3. AudioClipにデータを一括でセット
            audioClip.SetData(floatData, 0);

            audioSource.clip = audioClip;
            audioSource.Play();

            // 再生完了待機は同じ
            while (audioSource.isPlaying)
            {
                await Task.Yield();
            }
        }
    }

    void Awake()
    {
        SpeechConfig speechConfig = SpeechConfig.FromSubscription(SubscriptionKey, Region);

        speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);

        synthesizer = new SpeechSynthesizer(speechConfig, null);

        synthesizer.SynthesisCanceled += (s, e) =>
        {
            var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
        };
    }

    private void OnDestroy()
    {
        if (synthesizer != null)
        {
            synthesizer.Dispose();
        }
    }
}
