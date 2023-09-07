using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehavior : MonoBehaviour
{
    [SerializeField]
    Dialogue startingDialogue;

    // Start is called before the first frame update
    void Start()
    {
        DialogueManager.instance.StartDialogue(startingDialogue);
    }
}
