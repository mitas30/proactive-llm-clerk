using UnityEngine;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json; // JSONを扱うために必要

/// <summary>
/// ChatGPT とのやり取りを管理するクラスです。
/// 入力されたテキストを OpenAI の API に送信し、レスポンスを受け取ります。
/// </summary>
public class LlmManager : MonoBehaviour
{
    [SerializeField] string apiKey;

    [SerializeField]
    [Tooltip("[https://platform.openai.com/docs/models](https://platform.openai.com/docs/models) を見よう")]
    public string model;

    /// <summary>
    /// 初回にユーザーとして送信するプロンプトです。
    /// </summary>
    [TextArea(3, 10)][SerializeField] string initializationPrompt;

    private ChatClient client;

    /// <summary>
    /// 現在と関係のある会話文を保持するリスト
    /// </summary>
    private List<ChatMessage> pastMessages;

    private SystemChatMessage instructionMessage;

    // APIからのJSON応答をデシリアライズするためのクラス
    private class GptResponse
    {
        public List<string> candidate_question { get; set; }
        public string response { get; set; }
    }

    // JSON形式を指示するためのテンプレート文字列
    private const string ReactiveJsonInstruction = @"<instruction>
上記のコンテキストを元に、ユーザの発話に対する応答と、ユーザーが次にしてきそうな発話を3つ考えて、以下のJSON形式で出力してください。
JSONオブジェクトのみを出力し、説明やコードブロック（```）は含めないでください。

{
    ""candidate_question"": [""発話予測1"", ""発話予測2"", ""発話予測3""],
    ""response"": ""（ここに応答を生成）""
}
</instruction>";

    private const string ProactiveJsonInstruction = @"<instruction>
以下のコンテキストを元に発話文章を作成し、ユーザーが次に発話するのを助けるための想定質問文を3つ、以下のJSON形式で出力してください。
JSONオブジェクトのみを出力し、説明やコードブロック（```）は含めないでください。

{
    ""candidate_question"": [""質問候補1"", ""質問候補2"", ""質問候補3""],
    ""response"": ""（ここに応答本文を生成）""
}
</instruction>";
    public string InitializationPrompt => initializationPrompt;

    void Awake()
    {
        client = new(model: model, apiKey: apiKey);
        pastMessages = new();
        instructionMessage = new(initializationPrompt);
    }

    /// <summary>
    /// ChatGPTで能動的な発話内容を生成する。
    /// </summary>
    /// <returns>AIの応答本文と、次の質問候補リストをタプルで返します。</returns>
    async public Task<(string response, List<string> candidateQuestions)> ProactiveGPTRequest(string prompt)
    {
        // プロンプトにJSON形式の出力を指示する文章を追加
        var fullPrompt = ProactiveJsonInstruction + "\n\n" + prompt;
        var messages = new List<ChatMessage> { instructionMessage, new UserChatMessage(fullPrompt) };

        // ? 能動発話用のプロンプト
        var debugMessage = "[Requested : create proactive speaking content.]\n\n";
        for (int i = 0; i < messages.Count; i++)
        {
            debugMessage += $"[message{i}]\n{messages[i].Content[0].Text}\n\n";
        }
        Debug.Log(debugMessage);

        try
        {
            ChatCompletion openAIResponse = await client.CompleteChatAsync(messages);
            string jsonResponse = openAIResponse.Content[0].Text;

            // JSONをパースして、GptResponseオブジェクトに変換
            GptResponse parsedResponse = JsonSerializer.Deserialize<GptResponse>(jsonResponse);

            if (parsedResponse != null && !string.IsNullOrEmpty(parsedResponse.response))
            {
                // 応答本文を会話履歴に追加
                pastMessages.Add(new AssistantChatMessage(parsedResponse.response));

                Debug.Log($"[能動発話の内容]\nResponse: {parsedResponse.response}\nQuestions: {string.Join(", ", parsedResponse.candidate_question)}");

                // パースした応答本文と質問候補を返す
                // これにより、呼び出し元で変数に代入できる
                // 例: var (aiUtterance, nextQuestions) = await ProactiveGPTRequest("こんにちは");
                return (parsedResponse.response, parsedResponse.candidate_question);
            }
            else
            {
                Debug.LogWarning("Failed to parse GPT response or response is empty.");
            }
        }
        catch (JsonException jsonEx)
        {
            Debug.LogError($"GPT response JSON parse 失敗: {jsonEx.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"GPT request 失敗: {ex.Message}");
        }

        // 失敗した場合は空のデータを返す
        return (string.Empty, new List<string>());
    }

    /// <summary>
    /// ChatGPTでユーザの発話に応答する。
    /// </summary>
    /// <param name="prompt">LLMに渡すプロンプト</param>
    /// <param name="userSpeechText">ユーザの発話内容</param>
    /// <returns>AIの応答本文と、次の質問候補リストをタプルで返します。</returns>
    async public Task<(string response, List<string> candidateQuestions)> ResponseGPTRequest(string userSpeechText, string prompt)
    {
        var fullPrompt = $"{ReactiveJsonInstruction}" + "\n\n" + prompt;

        var messages = new List<ChatMessage> { instructionMessage };
        messages.AddRange(pastMessages);
        messages.Add(new UserChatMessage(fullPrompt));

        // ? AIの質問返信用のプロンプト
        var debugMessage = "[Requested : respond to user message.]\n\n";
        for (int i = 0; i < messages.Count; i++) debugMessage += $"[message{i}]\n{messages[i].Content[0].Text}\n\n";
        Debug.Log(debugMessage);

        try
        {
            ChatCompletion openAIResponse = await client.CompleteChatAsync(messages);
            if (openAIResponse.Content?.Count > 0)
            {
                string jsonResponse = openAIResponse.Content[0].Text;

                // JSONをパースして、GptResponseオブジェクトに変換
                GptResponse parsedResponse = JsonSerializer.Deserialize<GptResponse>(jsonResponse);

                if (parsedResponse != null && !string.IsNullOrEmpty(parsedResponse.response))
                {
                    // 会話履歴にユーザの発話とAIの応答を追加
                    pastMessages.Add(new UserChatMessage(userSpeechText));
                    pastMessages.Add(new AssistantChatMessage(parsedResponse.response));

                    Debug.Log($"[ユーザ発話への応答]\nResponse: {parsedResponse.response}\nQuestions: {string.Join(", ", parsedResponse.candidate_question)}");

                    return (parsedResponse.response, parsedResponse.candidate_question);
                }
                else
                {
                    Debug.LogWarning("Failed to parse GPT response or response is empty.");
                }
            }
            else
            {
                Debug.LogWarning("GPT does not any response.");
            }
        }
        catch (JsonException jsonEx)
        {
            Debug.LogError($"GPT response JSON parse 失敗: {jsonEx.Message}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"GPT request 失敗: {ex.Message}");
        }

        // 失敗した場合は空のデータを返す
        return (string.Empty, new List<string>());
    }
}