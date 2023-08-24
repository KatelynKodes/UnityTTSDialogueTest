using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityLibrary;

public class DialogueManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _dialogueText;
    [SerializeField]
    private TextMeshProUGUI _nameText;

    private Speech _SpeechManager;
    private AudioSource _audioSource;

    private Dialogue _currentConversation;
    private int _currentDialogueLine;

    private void Awake()
    {
        _SpeechManager = FindObjectOfType<Speech>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (_currentConversation != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NextLine();
            }
        }
    }

    public void StartDialogue(Dialogue conversation)
    {
        //Set the voice settings
        _SpeechManager.SetVoice(conversation.character.characterVoice);
        _SpeechManager.SetVoicePitch(conversation.character.characterVoicePitch);
        _SpeechManager.SetVoiceRange(conversation.character.characterVoiceRange);
        _SpeechManager.SetRate(conversation.character.characterVoiceRate);

        //Set the currentConversation
        _currentConversation = conversation;

        _currentDialogueLine = 0;
        NextLine();
    }

    private void NextLine()
    {
        if (_currentDialogueLine > _currentConversation.dialogue.Length)
        {
            //End the conversation
            EndConversation();
            return;
        }

        Debug.Log(_currentConversation.dialogue[_currentDialogueLine].line);
        _SpeechManager.Say(_currentConversation.dialogue[_currentDialogueLine].pronunciation, TTSCallback);
        _currentDialogueLine++;
    }

    private void EndConversation()
    {
        if (_currentConversation.branchingDialogue != null)
        {
            StartDialogue(_currentConversation.branchingDialogue);
        }
        else
        {
            //End Conversation
            Debug.Log("Conversation Ended");

            //Set the conversationlines to be null
            _currentConversation = null;
        }
    }

    void TTSCallback(string message, AudioClip audio)
    {
        _audioSource.clip = audio;
        _audioSource.Play();
    }
}
