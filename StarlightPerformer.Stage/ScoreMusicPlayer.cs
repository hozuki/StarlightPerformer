﻿using System;
using System.Collections.Generic;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using StarlightPerformer.Core;
using AudioOut = NAudio.Wave.DirectSoundOut;

namespace StarlightPerformer.Stage {
    public sealed class ScoreMusicPlayer : DisposableBase {

        public ScoreMusicPlayer() {
            _syncObject = new object();
            _waveStream = new WaveMixerStream32();
            _soundPlayer = new AudioOut();
            _soundPlayer.Init(_waveStream);
            _channels = new Dictionary<WaveStream, WaveChannel32>();
        }

        public event EventHandler<StoppedEventArgs> PlaybackStopped {
            add {
                if (_soundPlayer != null) {
                    _soundPlayer.PlaybackStopped += value;
                }
            }
            remove {
                if (_soundPlayer != null) {
                    _soundPlayer.PlaybackStopped -= value;
                }
            }
        }

        public event EventHandler<EventArgs> PositionChanged;

        public void Play() {
            if (IsPlaying) {
                if (IsPaused) {
                    IsPaused = false;
                } else {
                    Stop();
                }
            }
            _soundPlayer?.Play();
            IsPlaying = true;
        }

        public void Stop() {
            _soundPlayer?.Stop();
            IsPlaying = false;
        }

        public void Pause() {
            if (!IsPlaying || IsPaused) {
                return;
            }
            _soundPlayer?.Pause();
            IsPaused = true;
        }

        public TimeSpan CurrentTime {
            get { return _waveStream.CurrentTime; }
            set {
                lock (_syncObject) {
                    var waveStream = _waveStream;
                    waveStream.CurrentTime = value;
                    var position = waveStream.Position;
                    var blockAlign = waveStream.BlockAlign;
                    if (position % blockAlign != 0) {
                        position = (long)(Math.Round(position / (double)blockAlign) * blockAlign);
                        if (position < 0) {
                            position = 0;
                        }
                        waveStream.Position = position;
                    }
                }
                PositionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public TimeSpan TotalTime => _waveStream.TotalTime;

        public bool IsPlaying {
            get {
                lock (_syncObject) {
                    return _isPlaying;
                }
            }
            private set {
                lock (_syncObject) {
                    _isPlaying = value;
                }
            }
        }

        public bool IsPaused {
            get {
                lock (_syncObject) {
                    return _isPaused;
                }
            }
            private set {
                lock (_syncObject) {
                    _isPaused = value;
                }
            }
        }

        public void PlayFile(string fileName) {
            if (_managedWaveReader != null) {
                _managedWaveReader.Dispose();
                _managedWaveReader = null;
            }
            _managedWaveReader = new WaveFileReader(fileName);
            AddInputStream(_managedWaveReader);
            Play();
        }

        public WaveChannel32 AddInputStream(WaveStream waveStream, float volume = 1f) {
            lock (_syncObject) {
                var rateConvertedStream = waveStream;
                if (NeedSampleRateConversion(waveStream.WaveFormat)) {
                    rateConvertedStream = new ResamplerDmoStream(waveStream, _waveStream.WaveFormat);
                }
                var addedStream = new WaveChannel32(rateConvertedStream, volume, 0f);
                if (_channels.Count == 0 && _musicChannel == null) {
                    // The first stream is always the music stream.
                    _musicChannel = addedStream;
                } else {
                    if (!_channels.ContainsKey(waveStream)) {
                        _channels.Add(waveStream, addedStream);
                    }
                }
                _waveStream.AddInputStream(addedStream);
                return addedStream;
            }
        }

        public void RemoveInputStream(WaveStream waveStream) {
            lock (_syncObject) {
                if (_channels.ContainsKey(waveStream)) {
                    _channels.Remove(waveStream);
                }
                _waveStream.RemoveInputStream(waveStream);
            }
        }

        protected override void Dispose(bool disposing) {
            _soundPlayer?.Stop();
            _soundPlayer?.Dispose();
            _waveStream?.Dispose();
            _managedWaveReader?.Dispose();
            _soundPlayer = null;
            _waveStream = null;
            _soundPlayer = null;
            _managedWaveReader = null;
        }

        private bool NeedSampleRateConversion(WaveFormat waveFormat) {
            if (_waveStream.InputCount == 0) {
                return false;
            }
            return waveFormat.SampleRate != _waveStream.WaveFormat.SampleRate;
        }

        private WaveFileReader _managedWaveReader;
        private WaveMixerStream32 _waveStream;
        private AudioOut _soundPlayer;
        private bool _isPlaying;
        private bool _isPaused;
        private readonly object _syncObject;
        private readonly Dictionary<WaveStream, WaveChannel32> _channels;
        private WaveChannel32 _musicChannel;

    }
}
