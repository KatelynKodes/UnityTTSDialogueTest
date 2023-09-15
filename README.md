# Unity TTS Dialogue
## About
This repository attempts to utilize a runtime text-to-speech plugin for a dialogue system made similarly to my [Unreal Dialogue System](https://github.com/KatelynKodes/DialogueSystemUnreal) only this time using Unity's Scriptable objects (which are basically the same thing as Data Assets). This time I attempted to make it so that the text to speech would read the dialogue aloud to the viewer and also give developers the ability to give different voices to different characters.

## Changes Made
Though most code in the Speech.cs script from the original repository remains the same, some things were changed:
- The Speech.cs script no longer uses a queue to take in an input message, it only takes in one input message and one output message.
- Message types have been removed, if the user wishes to change the voice, pitch, range, etc. they can just do so via a single method rather than forcing the input to go through a switch statement in the thread trying to find what kind of message was taken in.
  - It also allows the output message variable to JUST be an output message type, as that was the only type it could be anyways.
- OutputMessage and InputMessage enums have been merged into one message enum, they were using the same variables anyway.
- There are new parameters in the Speech.cs script allowing the developer to change other aspects of the voice before the project loads up. Before, the developer could only set the default name of the voice before the game loaded. Now they can set:
  - A default voice
  - A default voice pitch (using a slider from 0-100)
  - A default voice range (using a slider from 0-100)
  - A default voice rate (using a slider from 80-450)
  - A default voice volume (using a slider from 0-200, defaulted at 100)
  - A default voice word gap
  - A default voice innotation
## How to use in your own unity project
In order to use this in your own unity project, please fork or copy the repository from github using the git client, gitkraken, or whatever other tool you use to obtain repositories from github. Pull requests will be reviewed before acceptance.
## Special Thanks
Special thanks to the original repository made by [Mika here on Github](https://github.com/unitycoder) allowing access to E-Speak's tools for unity, this repository was made with the intent to experiment with the code provided and potentially improve upon it.
