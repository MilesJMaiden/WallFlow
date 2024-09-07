using UnityEngine;
using TMPro;
using OpenAI;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OpenAI
{
    public class ChatGPTHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text outputText; // For displaying the ChatGPT response

        private OpenAIApi openai = new OpenAIApi();
        private List<ChatMessage> messages = new List<ChatMessage>(); // To hold conversation context

        public void SetTextAndRequestResponse(string transcription)
        {
            Debug.Log("ChatGPTHandler: Text received from VoiceManager: " + transcription);

            // Initiate the GPT request with the given transcription
            SendChatGPTRequest(transcription);
        }

        private async void SendChatGPTRequest(string transcription)
        {
            outputText.text = ""; // Clear any previous output
            Debug.Log("Sending request to ChatGPT...");

            // Append user message to the conversation history
            var userMessage = new ChatMessage
            {
                Role = "user",
                Content = transcription
            };
            messages.Add(userMessage);

            try
            {
                // Correctly using CreateChatCompletion to send a request to ChatGPT
                var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest
                {
                    Model = "gpt-4", // Ensure you're using the correct model version
                    Messages = messages,
                    MaxTokens = 150 // Adjust the max tokens as needed
                });

                if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
                {
                    var message = completionResponse.Choices[0].Message;
                    message.Content = message.Content.Trim();

                    messages.Add(message); // Add assistant's response to the conversation history
                    outputText.text = message.Content; // Display the response
                    Debug.Log("ChatGPT response received: " + message.Content);
                }
                else
                {
                    Debug.LogWarning("No text was generated from this prompt.");
                    outputText.text = "No response received.";
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"An error occurred while sending the request: {ex.Message}");
                outputText.text = "An error occurred.";
            }
        }
    }
}
