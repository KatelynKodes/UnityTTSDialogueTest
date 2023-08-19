using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newCharacterObject", menuName = "CharacterObject")]
public class CharacterObject : ScriptableObject
{
    public string characterName;
    public string characterVoice;
    [Range(0,100)]
    public float characterVoicePitch;
    [Range(0, 100)]
    public float characterVoiceRange;
    [Range(80, 450)]
    public float characterVoiceRate;
}
