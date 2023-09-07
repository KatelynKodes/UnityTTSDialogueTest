using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityLibrary;

public class DialogueManager : MonoBehaviour
{
    //private TextMeshProUGUI _dialogueText;
    //private TextMeshProUGUI _nameText;
    private AudioSource _audioSource;

    private Dialogue _currentConversation;
    private int _currentDialogueLine;
    public static DialogueManager instance;

    private void Awake()
    {
        instance = this;
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
        Speech.instance.SetVoice(conversation.character.characterVoice);
        Speech.instance.SetVoicePitch(conversation.character.characterVoicePitch);
        Speech.instance.SetVoiceRange(conversation.character.characterVoiceRange);
        Speech.instance.SetRate(conversation.character.characterVoiceRate);

        //Set the currentConversation
        _currentConversation = conversation;

        _currentDialogueLine = 0;
        NextLine();
    }

    private void NextLine()
    {
        if (_currentDialogueLine >= _currentConversation.dialogue.Length)
        {
            //End the conversation
            EndConversation();
            return;
        }

        Speech.instance.Say(_currentConversation.dialogue[_currentDialogueLine].pronunciation, TTSCallback);
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
