using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "newDialogue", menuName = "New Dialogue Object")]
public class Dialogue : ScriptableObject
{
    public CharacterObject character;
    public lines[] dialogue;
    public Dialogue branchingDialogue;

    [System.Serializable]
    public struct lines
    {
        [TextArea(5,7)]
        public string line;
        [TextArea(5, 7)]
        public string pronunciation;
    }
}
