using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private Dialogue _exampleDialogue;

    private DialogueManager _dialogueManager;

    // Runs before start
    private void Awake()
    {
        _dialogueManager = FindObjectOfType<DialogueManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _dialogueManager.StartDialogue(_exampleDialogue);
    }
}
