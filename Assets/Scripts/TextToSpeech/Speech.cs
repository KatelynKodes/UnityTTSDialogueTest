// https://github.com/unitycoder/UnityRuntimeTextToSpeech

using UnityEngine;
using System.IO;
using ESpeakWrapper;
using System.Threading;
using System.Collections.Generic;
using System;

namespace UnityLibrary
{
    // run before regular scripts
    [DefaultExecutionOrder(-100)]
    public class Speech : MonoBehaviour
    {
        [SerializeField]
        private string _voiceID = "Tweaky";

        [Range(0, 100), SerializeField]
        private int _voicePitch = 3;

        [Range(0, 100), SerializeField]
        private int _voiceRange = 4;

        [Range(80, 450), SerializeField]
        private int _voiceRate = 80;

        [Range(0, 200), SerializeField]
        private int _voiceVolume = 100;

        [SerializeField]
        private int _voiceInnotation = 9;

        [SerializeField]
        private int _voiceWordGap = 7;

        // singleton isntance
        public static Speech instance;

        public delegate void TTSCallback(string message, AudioClip audio);

        public enum MessageType {
            Say,
            SetRate,
            SetVolume,
            SetPitch,
            SetRange,
            SetWordGap,
            SetCapitals,
            SetIntonation,
            SetVoice,
            VoiceLineFinished,
        }

        public class Message {
            public MessageType type;
            public int pitchParam;
            public int rangeParam;
            public int rateParam;
            public int wordGapParam;
            public int capitalsParam;
            public int volumeParam;
            public int intonationParam;
            public float[] voicedata;
            public string message;
            public TTSCallback callback;
        }


        // queue for tts strings
        Mutex message_mutex = new Mutex();
        private Message _inputMessage = null;
        private Message _outgoingMessage = null;

        bool _isClosing = false;
        bool _isRunning = false;

        bool IsClosing
        {
            get
            {
                bool val = false;
                message_mutex.WaitOne();
                val = _isClosing;
                message_mutex.ReleaseMutex();
                return val;
            }
            set
            {
                message_mutex.WaitOne();
                _isClosing = value;
                message_mutex.ReleaseMutex();
            }
        }

        bool IsRunning
        {
            get
            {
                bool val = false;
                message_mutex.WaitOne();
                val = _isRunning;
                message_mutex.ReleaseMutex();
                return val;
            }
            set
            {
                message_mutex.WaitOne();
                _isRunning = value;
                message_mutex.ReleaseMutex();
            }
        }

        void Awake()
        {
            instance = this;

            // initialize with espeak voices folder
            string datafolder = Path.Combine(Application.streamingAssetsPath, "espeak-ng-data/");
            datafolder = datafolder.Replace("\\", "/");
            Client.Initialize(datafolder);

            // Set Default voice data
            if (!Client.SetVoiceByName(_voiceID)) Debug.Log("Failed settings voice: " + _voiceID);
            if (!Client.SetPitch(_voicePitch)) Debug.Log("Failed to set voice pitch to: " + _voicePitch);
            if (!Client.SetRange(_voiceRange)) Debug.Log("Failed to set voice range to: " + _voiceRange);
            if (!Client.SetRate(_voiceRate)) Debug.Log("Failed to set voice range to: " + _voiceRate);
            if (!Client.SetVolume(_voiceVolume)) Debug.Log("Failed to set voice range to: " + _voiceVolume);
            if (!Client.SetIntonation(_voiceInnotation)) Debug.Log("Failed to set voice innotation to:" + _voiceInnotation);
            if (!Client.SetWordgap(_voiceWordGap)) Debug.Log("Failed to set voice word gap to" + _voiceWordGap);

            // start thread for processing received TTS strings
            Thread thread = new Thread(new ThreadStart(SpeakerThread));
            thread.Start();
        }

        void SpeakerThread()
        {
            bool waitingForOutput = false;
            string inputMessageRecieved = "";
            TTSCallback inputCallback = null;
            IsRunning = true;
            while (IsClosing == false) {
                if (waitingForOutput) {
                    if (Client.VoiceFinished()) {
                        byte[] new_voice = Client.PopVoice();
                        float[] voice_float = new float[new_voice.Length / 2];

                        for (int i = 0; i < voice_float.Length; i++) {
                            //if(BitConverter.IsLittleEndian) 
                            voice_float[i] = (float)BitConverter.ToInt16(new_voice, i * 2) / (float)short.MaxValue;
                        }

                        Message om = new Message();
                        om.type = MessageType.VoiceLineFinished;
                        om.voicedata = voice_float;
                        om.message = inputMessageRecieved;
                        om.callback = inputCallback;

                        message_mutex.WaitOne();
                        _outgoingMessage = om;
                        message_mutex.ReleaseMutex();
                        waitingForOutput = false;
                        inputMessageRecieved = "";
                        inputCallback = null;
                    }
                } else if (HasInputMessage()) {
                    try
                    {
                        Message msg = _inputMessage;
                        Debug.Log("Input Message Recieved:" + _inputMessage.message);

                        switch (msg.type) {
                            case MessageType.Say:
                                Client.Speak(msg.message);
                                //Client.SpeakSSML(msg);

                                inputMessageRecieved = msg.message;
                                inputCallback = msg.callback;
                                waitingForOutput = true;
                                _inputMessage = null;
                                break;
                            case MessageType.SetPitch:
                                _voicePitch = msg.pitchParam;
                                Client.SetPitch(_voicePitch);
                                _inputMessage = null;
                                break;
                            case MessageType.SetRange:
                                _voiceRange = msg.rangeParam;
                                Client.SetRange(_voiceRange);
                                _inputMessage = null;
                                break;
                            case MessageType.SetRate:
                                _voiceRate = msg.rateParam;
                                Client.SetRate(_voiceRate);
                                _inputMessage = null;
                                break;
                            case MessageType.SetVolume:
                                _voiceVolume = msg.volumeParam;
                                Client.SetVolume(_voiceVolume);
                                _inputMessage = null;
                                break;
                            case MessageType.SetWordGap:
                                _voiceWordGap = msg.wordGapParam;
                                Client.SetWordgap(_voiceWordGap);
                                _inputMessage = null;
                                break;
                            case MessageType.SetCapitals:
                                Client.SetCapitals(msg.capitalsParam);
                                _inputMessage = null;
                                break;
                            case MessageType.SetIntonation:
                                Client.SetIntonation(msg.intonationParam);
                                _inputMessage = null;
                                break;
                            case MessageType.SetVoice:
                                _voiceID = msg.message;
                                Debug.Log(_voiceID);
                                if (!Client.SetVoiceByName(_voiceID))
                                {
                                    Debug.Log("Could not set voice to " + _voiceID);
                                }
                                else
                                {
                                    Debug.Log("Set Voice to" + _voiceID);
                                }
                                _inputMessage = null;
                                break;

                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                Thread.Sleep(8);
            }
            _isRunning = false;
        }

        // adds string to TTS queue
        public void Say(string msg, TTSCallback callback)
        {
            if (IsClosing || !IsRunning || string.IsNullOrEmpty(msg)) return;

            Message im = new Message();
            im.type = MessageType.Say;
            im.message = msg;
            im.callback = callback;
            _inputMessage = im;
        }

        public void SetRate(int rate)
        {
            if (IsClosing || !IsRunning || rate < 0) return;

            Message im = new Message();
            im.type = MessageType.SetRate;
            im.rateParam = rate;
            _inputMessage = im;
        }

        public void SetVolume(int volume)
        {
            if (IsClosing || !IsRunning || volume < 0) return;

            Message im = new Message();
            im.type = MessageType.SetVolume;
            im.volumeParam = volume;
            _inputMessage = im;
        }

        public void SetVoicePitch(int pitch)
        {
            if (IsClosing || !IsRunning || pitch < 0) return;

            Message im = new Message();
            im.type = MessageType.SetPitch;
            im.pitchParam = pitch;
            _inputMessage = im;
        }

        public void SetVoiceRange(int range)
        {
            if (IsClosing || !IsRunning || range < 0) return;
            Message im = new Message();
            im.type = MessageType.SetRange;
            im.rangeParam = range;
            _inputMessage = im;
        }

        public void SetWordGap(int wordGap)
        {
            if (IsClosing || !IsRunning || wordGap < 0) return;
            Message im = new Message();
            im.type = MessageType.SetWordGap;
            im.wordGapParam = wordGap;
            _inputMessage = im;
        }

        public void SetCapitals(int capitals)
        {
            if (IsClosing || !IsRunning || capitals < 0) return;
            Message im = new Message();
            im.type = MessageType.SetCapitals;
            im.capitalsParam = capitals;
            _inputMessage = im;
        }

        public void SetIntonation(int Intonation)
        {
            if (IsClosing || !IsRunning || Intonation < 0) return;
            Message im = new Message();
            im.type = MessageType.SetIntonation;
            im.intonationParam = Intonation;
            _inputMessage = im;
        }

        public void SetVoice(string voiceName)
        {
            if (IsClosing || !IsRunning || string.IsNullOrEmpty(voiceName)) return;

            Message im = new Message();
            im.type = MessageType.SetVoice;
            im.message = voiceName;
            _inputMessage = im;
        }

        private bool HasInputMessage()
        {
            bool ret = false;
            message_mutex.WaitOne();
            if(_inputMessage != null) {
                ret = true;
            }
            message_mutex.ReleaseMutex();
            return ret;
        }

        public void Update()
        {
            Message om = null;
            message_mutex.WaitOne();
            if(_outgoingMessage != null) {
                om = _outgoingMessage;
                _outgoingMessage = null;
            }
            message_mutex.ReleaseMutex();
            if (om != null)
            {
                AudioClip ac = AudioClip.Create("voice", om.voicedata.Length, 1, Client.sampleRate, false);
                ac.SetData(om.voicedata, 0);
                om.callback(om.message, ac); 
            }
        }

        private void OnDestroy()
        {
            Client.Stop();
            _isClosing = true;

            int wait_counter = 2000;
            // NOTE this will hang unity, until speech has stopped (otherwise crash)
            while (IsRunning) { 
                Thread.Sleep(1); 
                if(wait_counter-- < 0) {
                    Debug.LogError("Sound system dindn't shut down in time.");
                    break;
                }
            };
        }

    } // class
} // namespace