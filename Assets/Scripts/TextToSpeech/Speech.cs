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

        public class Message {
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
            SetVoice(_voiceID);
            SetVoicePitch(_voicePitch);
            SetVoiceRange(_voiceRange);
            SetRate(_voiceRate);
            SetVolume(_voiceVolume);
            SetIntonation(_voiceInnotation);
            SetWordGap(_voiceWordGap);

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
                } 
                else if (HasInputMessage()) {
                    try
                    {
                        Message msg = _inputMessage;

                        Client.Speak(msg.message);
                        //Client.SpeakSSML(msg);

                        inputMessageRecieved = msg.message;
                        inputCallback = msg.callback;
                        waitingForOutput = true;
                        _inputMessage = null;
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }

                Thread.Sleep(8);
            }
            _isRunning = false;
        }

        /// <summary>
        /// Creates an inputMessage for the TTS to speak
        /// </summary>
        /// <param name="msg"> The message for the tts to say </param>
        /// <param name="callback"> The callback delegate for the audio source</param>
        public void Say(string msg, TTSCallback callback)
        {
            if (IsClosing || !IsRunning || string.IsNullOrEmpty(msg)) return;

            Message im = new Message();
            im.message = msg;
            im.callback = callback;
            _inputMessage = im;
        }

        /// <summary>
        /// Sets the rate of the voice
        /// </summary>
        /// <param name="rate">value of new voice rate</param>
        public void SetRate(int rate)
        {
            if (IsClosing || !IsRunning || rate < 80 || rate > 450) return;

            int defaultRate = _voiceRate;
            _voiceRate = rate;
            if (!Client.SetRate(_voiceRate))
            {
                Debug.Log("Could not set the rate volume to " + _voiceRate);
                _voiceRate = defaultRate;
                Client.SetRate(_voiceRate);
            }
        }

        /// <summary>
        /// Sets the volume of the voice
        /// </summary>
        /// <param name="volume"> value of new voice volume </param>
        public void SetVolume(int volume)
        {
            if (IsClosing || !IsRunning) return;

            int defualtVolume = _voiceVolume;
            _voiceVolume = volume;
            if (!Client.SetVolume(_voiceVolume))
            {
                Debug.Log("Could not set the voice volume to " + _voiceVolume);
                _voiceVolume = defualtVolume;
                Client.SetVolume(_voiceVolume);
            }
        }

        /// <summary>
        /// Sets the voice pitch
        /// </summary>
        /// <param name="pitch">value of new voice pitch</param>
        public void SetVoicePitch(int pitch)
        {
            if (IsClosing || !IsRunning) return;

            int defaultVoicePitch = _voicePitch;
            _voicePitch = pitch;
            if (!Client.SetPitch(_voicePitch))
            { 
                Debug.Log("Could not set voice pitch to " + _voicePitch);
                _voicePitch = defaultVoicePitch;
                Client.SetPitch(_voicePitch);
            }
        }

        /// <summary>
        /// Sets the voice range
        /// </summary>
        /// <param name="range">value of new voice range</param>
        public void SetVoiceRange(int range)
        {
            if (IsClosing || !IsRunning) return;

            int defaultRange = _voiceRange;
            _voiceRange = range;
            if (!Client.SetRange(_voiceRange))
            {
                Debug.Log("Could not set voice range to " + _voiceRange);
                _voiceRange = defaultRange;
                Client.SetRange(_voiceRange);
            }
        }

        /// <summary>
        /// Sets the word gap of the voice
        /// </summary>
        /// <param name="wordGap"> integer value of new word gap</param>
        public void SetWordGap(int wordGap)
        {
            if (IsClosing || !IsRunning) return;

            int defaultWordGap = _voiceWordGap;
            _voiceWordGap = wordGap;
            if (!Client.SetWordgap(_voiceWordGap))
            {
                Debug.Log("Could not set the word gap to " + _voiceWordGap);
                _voiceWordGap = defaultWordGap;
                Client.SetWordgap(_voiceWordGap);
            }
        }

        /// <summary>
        /// Sets the voice innotation
        /// </summary>
        /// <param name="intonation">integer value of new voice innotation</param>
        public void SetIntonation(int intonation)
        {
            if (IsClosing || !IsRunning || intonation < 0) return;

            int defaultInnotation = _voiceInnotation;
            _voiceInnotation = intonation;
            if (!Client.SetIntonation(_voiceInnotation))
            {
                Debug.Log("Could not set voice innotation to" + _voiceInnotation);
                _voiceInnotation = defaultInnotation;
                Client.SetIntonation(defaultInnotation);
            }
        }

        /// <summary>
        /// Sets the voice 
        /// </summary>
        /// <param name="voiceName"></param>
        public void SetVoice(string voiceName)
        {
            if (IsClosing || !IsRunning || string.IsNullOrEmpty(voiceName)) return;

            string defaultVoice = _voiceID;
            _voiceID = voiceName;
            if (!Client.SetVoiceByName(_voiceID))
            {
                Debug.Log("Could not set voice to " + _voiceID);
                _voiceID = defaultVoice;
                Client.SetVoiceByName(_voiceID);
            }
        }

        /// <summary>
        /// Checks if inputmessage is null
        /// </summary>
        /// <returns>true if inputmessage is not null</returns>
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