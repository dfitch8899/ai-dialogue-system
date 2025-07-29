// NPCDialogueTrigger.cs
// Handles triggering AI-generated dialogue with an NPC when the player enters range and presses a key.
//
// Author: Devin Fitch
// Date: 7/14/2025

using UnityEngine;
using UnityEngine.UI; // <--- IMPORTANT: Ensure this is included for UnityEngine.UI.Text
// using TMPro; // <--- REMOVE OR COMMENT OUT THIS LINE if you're not using TextMeshPro at all in this script

public class NPCDialogueTrigger : MonoBehaviour
{
    [Header("Dialogue System")]
    public AIDialogueUI dialogueUI;

    [Header("NPC Info")]
    public string npcName = "Elaric";
    [TextArea]
    public string npcPersona = "Wise old merchant who has seen many wars.";
    public string[] dialogueOptions = new string[]
    {
        "Tell me about this town.",
        "Do you trade goods?",
        "Have you seen any danger nearby?",
        "Why are you here?"
    };

    [Header("Interaction Prompt")]
    public Text interactPromptText; // <--- CORRECT: This should be UnityEngine.UI.Text

    private bool playerInRange = false;

    void Start()
    {
        if (interactPromptText != null)
        {
            interactPromptText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (!dialogueUI.gameObject.activeSelf)
            {
                dialogueUI.npcName = npcName;
                dialogueUI.npcPersona = npcPersona;
                dialogueUI.ShowOptions(dialogueOptions);
                Debug.Log("Dialogue triggered with NPC.");

                if (interactPromptText != null)
                {
                    interactPromptText.gameObject.SetActive(false);
                }
            }
            else
            {
                dialogueUI.gameObject.SetActive(false);
                Debug.Log("Dialogue UI hidden.");

                if (interactPromptText != null)
                {
                    interactPromptText.gameObject.SetActive(true);
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered NPC interaction range.");

            if (interactPromptText != null)
            {
                interactPromptText.text = "Press 'E' to Interact";
                interactPromptText.gameObject.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player exited NPC interaction range.");

            if (interactPromptText != null)
            {
                interactPromptText.gameObject.SetActive(false);
            }

            if (dialogueUI.gameObject.activeSelf)
            {
                dialogueUI.gameObject.SetActive(false);
                Debug.Log("Dialogue UI closed because player left range.");
            }
        }
    }
}