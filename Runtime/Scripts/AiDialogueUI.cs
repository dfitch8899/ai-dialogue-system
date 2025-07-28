// AIDialogueUI.cs
// Manages the user interface for AI-generated NPC dialogue, including displaying the NPC's response
// and presenting the player with multiple dialogue options to choose from.
//
// Author: Devin Fitch
// Date: 7/14/2025

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AIDialogueUI : MonoBehaviour
{
    [Header("Dependencies")]
    public AIDialogueManager aiManager;

    [Header("UI References")]
    public TextMeshProUGUI npcNameText;
    public TextMeshProUGUI npcText;
    public Transform optionsContainer;
    public GameObject buttonPrefab;

    [Header("NPC Data")]
    public string npcName;
    [TextArea]
    public string npcPersona;

    [Header("Dialogue Options")]
    [SerializeField]
    private string[] defaultOptions = new string[]
    {
        "Tell me about this place.",
        "Do you sell anything?", 
        "Have you noticed anything strange lately?",
        "What brings you here?",
        "What's your story?",
        "Heard any interesting news?"
    };

    [SerializeField]
    private string[] genericFollowUpOptions = new string[]
    {
        "Tell me more about that.",
        "That sounds interesting.",
        "I see. What else should I know?",
        "Interesting. Any other details?",
        "That's quite a story.",
        "Thanks for telling me.",
        "I should get going now.",
        "See you around."
    };

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Destroy all dynamically created buttons when the UI is disabled
        ClearOptions();
    }

    /// <summary>
    /// Called when a player selects a dialogue option. Sends input to AI.
    /// </summary>
    public void OnOptionSelected(string playerInput)
    {
        // Show loading state (optional)
        npcText.text = "...";
        
        // Disable buttons during AI response to prevent multiple clicks
        SetButtonsInteractable(false);

        StartCoroutine(aiManager.GetAIResponse(npcName, npcPersona, playerInput, OnAIResponseReceived));
    }

    /// <summary>
    /// Called when AI response is received. Displays response and generates new options.
    /// </summary>
    void OnAIResponseReceived(string response)
    {
        DisplayResponse(response);
        
        // Use AI to generate contextual options based on the response
        StartCoroutine(aiManager.GenerateDialogueOptions(npcName, npcPersona, response, OnOptionsGenerated));
    }

    /// <summary>
    /// Called when AI-generated dialogue options are received.
    /// </summary>
    void OnOptionsGenerated(string[] options)
    {
        // Clear existing options
        ClearOptions();

        // Create buttons for the new AI-generated options
        foreach (string optionText in options)
        {
            CreateOptionButton(optionText);
        }
        
        // Re-enable buttons
        SetButtonsInteractable(true);
    }

    /// <summary>
    /// Displays the AI-generated response.
    /// </summary>
    void DisplayResponse(string response)
    {
        // Ensure text wrapping is enabled
        if (npcText != null)
        {
            npcText.enableWordWrapping = true;
            npcText.overflowMode = TextOverflowModes.Overflow; // or TextOverflowModes.Truncate
            npcText.text = response;
        }
    }

    /// <summary>
    /// Generates new dialogue options based on the NPC's response context.
    /// Now uses AI to generate contextual options.
    /// </summary>
    void GenerateContextualOptions(string npcResponse)
    {
        // This method is now replaced by AI-generated options
        // but keeping it as fallback for initial conversation
        ClearOptions();

        // For initial conversation, use default options
        string[] options = defaultOptions;
        
        // Create buttons for the options
        foreach (string optionText in options)
        {
            CreateOptionButton(optionText);
        }
    }

    /// <summary>
    /// Generates smart dialogue options based on NPC response content.
    /// </summary>
    string[] GenerateSmartOptions(string npcResponse)
    {
        // Convert to lowercase for easier keyword matching
        string response = npcResponse.ToLower();
        
        // Create a list to store contextual options
        System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>();

        // Add contextual options based on keywords in the response
        if (response.Contains("trade") || response.Contains("merchant") || response.Contains("goods") || response.Contains("sell"))
        {
            string[] tradeOptions = {
                "What's your most valuable item?",
                "Do you have any rare goods?",
                "Where do you get your merchandise?",
                "Are your prices negotiable?",
                "What's selling well these days?"
            };
            options.Add(GetRandomFromArray(tradeOptions));
        }
        
        if (response.Contains("danger") || response.Contains("monster") || response.Contains("threat") || response.Contains("attack"))
        {
            string[] dangerOptions = {
                "How recent was this threat?",
                "What exactly did you see?",
                "Is there a safe path around it?",
                "Have others been hurt?", 
                "Should I be worried?"
            };
            options.Add(GetRandomFromArray(dangerOptions));
        }
        
        if (response.Contains("town") || response.Contains("city") || response.Contains("place") || response.Contains("here"))
        {
            string[] locationOptions = {
                "Who's in charge around here?",
                "What's the local gossip?",
                "Any notable landmarks nearby?",
                "How long have you lived here?",
                "What do visitors usually come for?"
            };
            options.Add(GetRandomFromArray(locationOptions));
        }
        
        if (response.Contains("quest") || response.Contains("task") || response.Contains("help") || response.Contains("need"))
        {
            string[] questOptions = {
                "What kind of help do you need?",
                "What's the reward for helping?",
                "How dangerous is this task?",
                "When do you need this done?",
                "Can you provide any equipment?"
            };
            options.Add(GetRandomFromArray(questOptions));
        }
        
        if (response.Contains("story") || response.Contains("past") || response.Contains("history") || response.Contains("ago"))
        {
            string[] loreOptions = {
                "What happened before that?",
                "Who else was involved?",
                "How did that change things?",
                "Is there more to this tale?",
                "What lesson did you learn?"
            };
            options.Add(GetRandomFromArray(loreOptions));
        }

        if (response.Contains("magic") || response.Contains("spell") || response.Contains("wizard") || response.Contains("enchant"))
        {
            string[] magicOptions = {
                "Tell me about the magic here.",
                "Are there wizards nearby?",
                "Is magic common in these parts?",
                "Have you seen any spells cast?",
                "Do you know any magical secrets?"
            };
            options.Add(GetRandomFromArray(magicOptions));
        }

        if (response.Contains("war") || response.Contains("battle") || response.Contains("fight") || response.Contains("soldier"))
        {
            string[] warOptions = {
                "Which side were you on?",
                "How did the battle end?",
                "Do conflicts still rage?",
                "What was your role in it?",
                "Any heroes from that time?"
            };
            options.Add(GetRandomFromArray(warOptions));
        }

        if (response.Contains("family") || response.Contains("wife") || response.Contains("child") || response.Contains("parent"))
        {
            string[] familyOptions = {
                "Tell me about your family.",
                "Do they live nearby?",
                "How do you support them?",
                "Are they safe?",
                "What are they like?"
            };
            options.Add(GetRandomFromArray(familyOptions));
        }

        // Add varied generic follow-up options
        string[] contextualFollowUps = {
            "That's quite interesting.",
            "I never would have guessed.",
            "You seem to know a lot.",
            "That explains some things.",
            "Fascinating perspective.",
            "Tell me something else."
        };
        
        if (options.Count < 2)
        {
            options.Add(GetRandomFromArray(contextualFollowUps));
        }

        // Always include at least one farewell option, but vary it
        string[] farewellOptions = {
            "I should get going.",
            "Thanks for your time.",
            "Farewell for now.",
            "I'll be on my way.",
            "Until we meet again."
        };
        options.Add(GetRandomFromArray(farewellOptions));

        // Fill remaining slots with varied generic options
        while (options.Count < 4)
        {
            string[] remainingOptions = {
                "What's your opinion on that?",
                "Have you always felt this way?",
                "What would you recommend?",
                "Is there anything else?",
                "What's most important to know?",
                "Any final thoughts?"
            };
            
            string newOption = GetRandomFromArray(remainingOptions);
            if (!options.Contains(newOption))
            {
                options.Add(newOption);
            }
            else
            {
                break; // Prevent infinite loop
            }
        }

        // Ensure exactly 4 options
        while (options.Count > 4)
        {
            options.RemoveAt(options.Count - 1);
        }

        return options.ToArray();
    }

    /// <summary>
    /// Helper method to get a random string from an array.
    /// </summary>
    string GetRandomFromArray(string[] array)
    {
        return array[UnityEngine.Random.Range(0, array.Length)];
    }

    /// <summary>
    /// Creates a single option button.
    /// </summary>
    void CreateOptionButton(string optionText)
    {
        GameObject newButton = Instantiate(buttonPrefab, optionsContainer);
        
        TextMeshProUGUI label = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = optionText;
        else
            Debug.LogWarning("Button prefab is missing a TextMeshProUGUI child.");
        
        Button btn = newButton.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() =>
            {
                OnOptionSelected(optionText);
            });
        }
        else
        {
            Debug.LogWarning("Button prefab is missing a Button component.");
        }
    }

    /// <summary>
    /// Enables or disables all option buttons.
    /// </summary>
    void SetButtonsInteractable(bool interactable)
    {
        Button[] buttons = optionsContainer.GetComponentsInChildren<Button>();
        foreach (Button btn in buttons)
        {
            btn.interactable = interactable;
        }
    }

    /// <summary>
    /// Clears all dynamically generated option buttons from the container.
    /// </summary>
    private void ClearOptions()
    {
        foreach (Transform child in optionsContainer)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Creates buttons for each dialogue option dynamically.
    /// </summary>
    public void ShowOptions(string[] options = null)
    {
        // FIRST: Activate the GameObject so coroutines can run
        gameObject.SetActive(true);

        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }
        else
        {
            Debug.LogWarning("npcNameText TextMeshProUGUI reference is missing in AIDialogueUI.");
        }

        // Clear any existing buttons before generating new ones
        ClearOptions();

        // Use provided options or default ones for initial conversation
        string[] optionsToShow = options ?? defaultOptions;

        foreach (string optionText in optionsToShow)
        {
            CreateOptionButton(optionText);
        }

        // NOW generate AI greeting after GameObject is active
        if (npcText != null)
        {
            npcText.text = "..."; // Show loading while generating greeting
            Debug.Log($"Generating greeting for {npcName} with persona: {npcPersona}");
            
            // Check if we have the required components
            if (aiManager != null && !string.IsNullOrEmpty(npcPersona))
            {
                StartCoroutine(aiManager.GenerateInitialGreeting(npcName, npcPersona, OnInitialGreetingReceived));
            }
            else
            {
                Debug.LogWarning("Missing aiManager or npcPersona - using fallback greeting");
                npcText.text = $"Greetings, traveler. I am {npcName}.";
            }
        }
        else
        {
            Debug.LogError("npcText is null! Make sure it's assigned in the inspector.");
        }
    }

    /// <summary>
    /// Called when the AI-generated initial greeting is received.
    /// </summary>
    void OnInitialGreetingReceived(string greeting)
    {
        Debug.Log($"Received greeting: {greeting}");
        if (npcText != null)
        {
            npcText.text = greeting;
        }
        else
        {
            Debug.LogError("npcText is null when trying to set greeting!");
        }
    }

    /// <summary>
    /// Manually trigger greeting generation (for debugging).
    /// </summary>
    [ContextMenu("Generate Greeting")]
    public void GenerateGreeting()
    {
        if (aiManager != null && !string.IsNullOrEmpty(npcName) && !string.IsNullOrEmpty(npcPersona))
        {
            Debug.Log("Manually generating greeting...");
            npcText.text = "...";
            StartCoroutine(aiManager.GenerateInitialGreeting(npcName, npcPersona, OnInitialGreetingReceived));
        }
        else
        {
            Debug.LogError("Cannot generate greeting - missing aiManager, npcName, or npcPersona");
        }
    }
}