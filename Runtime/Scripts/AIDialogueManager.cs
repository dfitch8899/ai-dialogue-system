// AIDialogueManager.cs
// Handles sending NPC dialogue prompts to a backend LLM and processing the AI response.
//
// Author: Devin Fitch
// Date: 7/14/2025

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using System.Text.RegularExpressions;

public class AIDialogueManager : MonoBehaviour
{
    private const string apiUrl = "https://backend-server-aurw.onrender.com/chat/gpt";

    [Serializable]
    private class RequestMessage
    {
        public string role;
        public string content;
    }

    [Serializable]
    private class RequestBody
    {
        public RequestMessage[] messages;
    }

    /// <summary>
    /// Sends player input and NPC context to the backend LLM server and returns the NPC's AI-generated reply.
    /// </summary>
    public IEnumerator GetAIResponse(string npcName, string persona, string playerInput, System.Action<string> onResponse)
    {
        // Create the system message with enhanced creativity instructions
        RequestMessage systemMsg = new RequestMessage
        {
            role = "system",
            content = $"You are {npcName}, an NPC with the following persona: {persona}. " +
                     "Give specific, creative responses that feel unique to each question. " +
                     "Include concrete details, names, locations, or small stories to make responses memorable. " +
                     "Feel free to invent minor details that fit your character and the fantasy setting. " +
                     "Vary your tone and focus based on what the player asks - be helpful for practical questions, " +
                     "mysterious for lore questions, worried about dangers, etc. " +
                     "CRITICAL: Use completely normal, modern English. Never use words like: aye, ye, thee, thou, hath, doth, 'tis, pray, good sir, m'lord, traveler, stranger, wanderer. " +
                     "Talk like a normal person having a conversation. Avoid all fantasy clichés and archaic language. " +
                     "Even though this is a fantasy setting, speak with contemporary, natural language patterns. " +
                     "Keep responses 3-4 sentences (under 250 characters). Make each response feel distinct and specific to YOUR character."
        };

        RequestMessage userMsg = new RequestMessage
        {
            role = "user",
            content = playerInput
        };

        // Create the request body
        RequestBody requestBody = new RequestBody
        {
            messages = new RequestMessage[] { systemMsg, userMsg }
        };

        // Convert to JSON
        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log("Sending request: " + jsonBody);

        // Create the web request
        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        // Check for success
        if (request.result == UnityWebRequest.Result.Success)
        {
            string rawResponse = request.downloadHandler.text;
            Debug.Log("Raw response: " + rawResponse);

            // Extract the actual response text using regex since Unity's JSON parsing has limitations
            try
            {
                // Use regex to extract the assistant's message content
                string pattern = "\"content\":\"([^\"]*?)\"";
                Match match = Regex.Match(rawResponse, pattern);
                
                if (match.Success && match.Groups.Count > 1)
                {
                    string aiResponse = match.Groups[1].Value;
                    // More thorough unescape handling
                    aiResponse = UnescapeJsonString(aiResponse);
                    
                    // Additional safety: truncate if still too long (increased limit)
                    if (aiResponse.Length > 300)
                    {
                        aiResponse = aiResponse.Substring(0, 297) + "...";
                    }
                    
                    // Final check - if response is just escape characters or very short, use fallback
                    if (string.IsNullOrWhiteSpace(aiResponse) || aiResponse.Length < 3 || aiResponse.Trim() == "\\")
                    {
                        aiResponse = "I'm not sure how to respond to that.";
                    }
                    
                    onResponse?.Invoke(aiResponse);
                }
                else
                {
                    Debug.LogError("Could not extract response content from: " + rawResponse);
                    onResponse?.Invoke("Sorry, I'm having trouble understanding right now.");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing response: {e.Message}");
                onResponse?.Invoke("...");
            }
        }
        else
        {
            Debug.LogError($"Request failed: {request.error}\nResponse: {request.downloadHandler.text}");
            onResponse?.Invoke("...");
        }
    }

    /// <summary>
    /// Generates dialogue options based on the NPC's response using AI.
    /// </summary>
    public IEnumerator GenerateDialogueOptions(string npcName, string persona, string npcResponse, System.Action<string[]> onOptionsGenerated)
    {
        RequestMessage systemMsg = new RequestMessage
        {
            role = "system",
            content = $"Based on {npcName}'s response, generate exactly 4 dialogue options for the player. " +
                     "Make them specific to what {npcName} just said, not generic. " +
                     "Include follow-up questions, requests for clarification, or new topics suggested by their response. " +
                     "Always include one polite way to end the conversation. " +
                     "CRITICAL: Use normal, modern English only. Never use: pray tell, good sir, m'lord, might I, prithee, fare thee well, etc. " +
                     "Make options sound like how a real person talks in normal conversation. " +
                     "Examples: 'What do you mean?', 'Where did that happen?', 'That's crazy', 'I should go', 'Thanks for the info' " +
                     "Format as a simple list separated by '|' with no numbers or bullets."
        };

        RequestMessage userMsg = new RequestMessage
        {
            role = "user",
            content = $"{npcName} just said: \"{npcResponse}\"\n\nGenerate 4 specific dialogue options for the player to respond with."
        };

        RequestBody requestBody = new RequestBody
        {
            messages = new RequestMessage[] { systemMsg, userMsg }
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log("Generating options with: " + jsonBody);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string rawResponse = request.downloadHandler.text;
            Debug.Log("Options response: " + rawResponse);

            try
            {
                string pattern = "\"content\":\"([^\"]*?)\"";
                Match match = Regex.Match(rawResponse, pattern);
                
                if (match.Success && match.Groups.Count > 1)
                {
                    string optionsText = match.Groups[1].Value;
                    optionsText = UnescapeJsonString(optionsText);
                    
                    // Split by | to get individual options
                    string[] options = optionsText.Split('|');
                    
                    // Clean up the options and ensure we have exactly 4
                    System.Collections.Generic.List<string> cleanOptions = new System.Collections.Generic.List<string>();
                    foreach (string option in options)
                    {
                        string cleanOption = option.Trim();
                        // Filter out empty, too short, or invalid options
                        if (!string.IsNullOrWhiteSpace(cleanOption) && 
                            cleanOption.Length > 2 && 
                            cleanOption != "\\" && 
                            !cleanOption.StartsWith("\\") &&
                            cleanOptions.Count < 4)
                        {
                            cleanOptions.Add(cleanOption);
                        }
                    }
                    
                    // If we don't have enough valid options, add fallbacks
                    string[] fallbackOptions = { "Tell me more.", "That's interesting.", "I see.", "I should be going." };
                    int fallbackIndex = 0;
                    
                    while (cleanOptions.Count < 4 && fallbackIndex < fallbackOptions.Length)
                    {
                        if (!cleanOptions.Contains(fallbackOptions[fallbackIndex]))
                        {
                            cleanOptions.Add(fallbackOptions[fallbackIndex]);
                        }
                        fallbackIndex++;
                    }
                    
                    onOptionsGenerated?.Invoke(cleanOptions.ToArray());
                }
                else
                {
                    Debug.LogError("Could not extract options from: " + rawResponse);
                    // Fallback to default options
                    onOptionsGenerated?.Invoke(new string[] { "Tell me more.", "That's interesting.", "I see.", "Goodbye." });
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing options: {e.Message}");
                onOptionsGenerated?.Invoke(new string[] { "Tell me more.", "That's interesting.", "I see.", "Goodbye." });
            }
        }
        else
        {
            Debug.LogError($"Options request failed: {request.error}");
            onOptionsGenerated?.Invoke(new string[] { "Tell me more.", "That's interesting.", "I see.", "Goodbye." });
        }
    }

    /// <summary>
    /// Generates an initial AI greeting based on the NPC's character.
    /// </summary>
    public IEnumerator GenerateInitialGreeting(string npcName, string persona, System.Action<string> onGreetingGenerated)
    {
        RequestMessage systemMsg = new RequestMessage
        {
            role = "system",
            content = $"You are {npcName}, an NPC with the following persona: {persona}. " +
                     "Generate a brief initial greeting (1-2 sentences) that a player would see when first approaching you. " +
                     "Make it reflect your personality and current situation. Keep it under 150 characters. " +
                     "CRITICAL: Use completely normal, modern English. Never say: aye, ye, greetings, salutations, hail, good day, traveler, stranger, wanderer, m'lord, good sir. " +
                     "Talk like a real person would when meeting someone new. Use contemporary, natural speech patterns. " +
                     "Examples of GOOD greetings: 'Hey there!', 'Oh, hi!', 'Can I help you?', 'You look lost.', 'Not many people come through here.' " +
                     "Make it interesting and character-specific, but keep the language completely natural and modern."
        };

        RequestMessage userMsg = new RequestMessage
        {
            role = "user",
            content = "A player is approaching you for the first time. Give them an initial greeting that fits your character."
        };

        RequestBody requestBody = new RequestBody
        {
            messages = new RequestMessage[] { systemMsg, userMsg }
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log("Generating greeting with: " + jsonBody);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string rawResponse = request.downloadHandler.text;
            Debug.Log("Greeting response: " + rawResponse);

            try
            {
                string pattern = "\"content\":\"([^\"]*?)\"";
                Match match = Regex.Match(rawResponse, pattern);
                
                if (match.Success && match.Groups.Count > 1)
                {
                    string greeting = match.Groups[1].Value;
                    greeting = UnescapeJsonString(greeting);
                    
                    // Ensure greeting isn't too long
                    if (greeting.Length > 200)
                    {
                        greeting = greeting.Substring(0, 197) + "...";
                    }
                    
                    // Check for invalid greeting (just backslashes or too short)
                    if (string.IsNullOrWhiteSpace(greeting) || greeting.Length < 3 || greeting.Trim() == "\\")
                    {
                        greeting = $"Greetings, traveler. I am {npcName}.";
                    }
                    
                    onGreetingGenerated?.Invoke(greeting);
                }
                else
                {
                    Debug.LogError("Could not extract greeting from: " + rawResponse);
                    onGreetingGenerated?.Invoke("Hey there, traveler!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error parsing greeting: {e.Message}");
                onGreetingGenerated?.Invoke("Hey there, traveler!");
            }
        }
        else
        {
            Debug.LogError($"Greeting request failed: {request.error}");
            onGreetingGenerated?.Invoke("Hey there, traveler!");
        }
    }

    /// <summary>
    /// Helper method to properly unescape JSON strings and handle malformed responses.
    /// </summary>
    private string UnescapeJsonString(string jsonString)
    {
        if (string.IsNullOrEmpty(jsonString))
            return "";

        // Handle common JSON escape sequences
        string result = jsonString
            .Replace("\\\"", "\"")      // Escaped quotes
            .Replace("\\\\", "\\")      // Escaped backslashes (do this after quotes)
            .Replace("\\n", "\n")       // New lines
            .Replace("\\r", "\r")       // Carriage returns
            .Replace("\\t", "\t")       // Tabs
            .Replace("\\/", "/");       // Forward slashes

        // Remove any remaining standalone backslashes that might be artifacts
        result = System.Text.RegularExpressions.Regex.Replace(result, @"(?<!\\)\\(?![\\""nrt/])", "");

        return result.Trim();
    }
}