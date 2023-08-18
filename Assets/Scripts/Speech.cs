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
        public string voiceID = "Tweaky";

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

            // select voice
            var setvoice = Client.SetVoiceByName(voiceID);
            if (setvoice == false) Debug.Log("Failed settings voice: " + voiceID);

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
                                Client.SetPitch(msg.pitchParam);
                                break;
                            case MessageType.SetRange:
                                Client.SetRange(msg.rangeParam);
                                break;
                            case MessageType.SetRate:
                                Client.SetRate(msg.rateParam);
                                break;
                            case MessageType.SetVolume:
                                Client.SetVolume(msg.volumeParam);
                                break;
                            case MessageType.SetWordGap:
                                Client.SetWordgap(msg.wordGapParam);
                                break;
                            case MessageType.SetCapitals:
                                Client.SetCapitals(msg.capitalsParam);
                                break;
                            case MessageType.SetIntonation:
                                Client.SetIntonation(msg.intonationParam);
                                break;
                            case MessageType.SetVoice:
                                Client.SetVoiceByName(msg.message);
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
            //QueueMessage(im);
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