/*  
    Field Of Judgement, Copyright (C) 2014 Tomasz Hachaj, Ph.D
    The source code is under MIT license. 
    All dlls are under licenses provided by theirs copyright holders.

    This file contains also a code taken from:
    Voxel Game project http://www.voxelgame.com/ (public domain)
  
    The MIT License (MIT)
    Copyright (c) 2014 Tomasz Hachaj, Ph.D
  
    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace FieldOfJudgement.Sound
{
public enum SoundType : byte
	{
		ForceField,
		IntoTheCube,
        SavageGround,
		MagicMissle,
		Fireball,
		WomanHit,
		WomanDeath,
		Hit,
        NoMana,
        SpellHitBorder
	}

internal static class Audio
{
    #region Properties
    public static String BackgroundMusic = null;
    private static AudioContext _audioContext;
    private static int[] _buffer;
    private static int[] _source;
    #endregion

    public static bool SoundEnabled = true;

    #region Load
    /// <summary>Load and buffer all the sounds for the game.</summary>
    internal static void LoadSounds()
    {
        if (!SoundEnabled) return;

        try
        {
            _audioContext = new AudioContext();
            //Debug.WriteLine("Audio device: " + _audioContext.CurrentDevice);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            //if we cant create an audio context then disable sounds in the config and return
            return;
        }

        int soundsCount = Enum.GetValues(typeof(SoundType)).Length;
        _buffer = new int[soundsCount];
        _source = new int[soundsCount];
        try
        {
            for (int i = 0; i < soundsCount; i++)
            {
                _source[i] = AL.GenSource();
                _buffer[i] = AL.GenBuffer();
            }
            Assembly a = Assembly.GetExecutingAssembly();
            String[] nombres = a.GetManifestResourceNames();
            Stream readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Sounds.forcefield.wav");
            BufferSound(readerStream, SoundType.ForceField);
            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Music.IntoTheCube.wav");
            BufferSound(readerStream, SoundType.IntoTheCube);
            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Music.SavageGround.wav");
            BufferSound(readerStream, SoundType.SavageGround);

            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Sounds.fireball.wav");
            BufferSound(readerStream, SoundType.Fireball);
            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Sounds.magicmissle.wav");
            BufferSound(readerStream, SoundType.MagicMissle);

            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Sounds.womanhit.wav");
            BufferSound(readerStream, SoundType.WomanHit);
            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Sounds.womandeath.wav");
            BufferSound(readerStream, SoundType.WomanDeath);

            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Sounds.hit.wav");
            BufferSound(readerStream, SoundType.Hit);

            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Sounds.nomana.wav");
            BufferSound(readerStream, SoundType.NoMana);

            readerStream = a.GetManifestResourceStream("FieldOfJudgement.Resources.Sounds.spellhitborder.wav");
            BufferSound(readerStream, SoundType.SpellHitBorder);
        }
        catch (Exception ex)
        {
            throw new Exception("Error buffering sounds. If the problem persists try running with sound disabled.\n" + ex.Message);
        }

        //wire sound events
        //PerformanceHost.OnSecondElapsed += PerformanceHost_OnSecondElapsed;
        //PerformanceHost.OnFiveSecondsElapsed += PerformanceHost_OnFiveSecondsElapsed;
    }

    /// <summary>Buffer the sound to OpenAL.</summary>
    private static void BufferSound(System.IO.Stream soundStream, SoundType type)
    {
        int channels, bitsPerSample, sampleRate;
        byte[] soundData = LoadWave(soundStream, out channels, out bitsPerSample, out sampleRate);
        for (int a = 0; a < 100; a++)
            soundData[soundData.Length - 100 + a] = 0;
        AL.BufferData(_buffer[(byte)type], GetSoundFormat(channels, bitsPerSample), soundData, soundData.Length, sampleRate);
    }

    /// <summary>Loads a wave/riff audio file from a stream. Resource file can be passed as the stream.</summary>
    private static byte[] LoadWave(System.IO.Stream stream, out int channels, out int bits, out int rate)
    {
        if (stream == null) throw new ArgumentNullException("stream");

        // ReSharper disable UnusedVariable
        using (var reader = new System.IO.BinaryReader(stream))
        {
            //RIFF header
            var signature = new string(reader.ReadChars(4));
            if (signature != "RIFF") throw new NotSupportedException("Specified stream is not a wave file.");

            int riffChunckSize = reader.ReadInt32();

            var format = new string(reader.ReadChars(4));
            if (format != "WAVE") throw new NotSupportedException("Specified stream is not a wave file.");

            //WAVE header
            var formatSignature = new string(reader.ReadChars(4));
            if (formatSignature != "fmt ") throw new NotSupportedException("Specified wave file is not supported.");

            int formatChunkSize = reader.ReadInt32();
            int audioFormat = reader.ReadInt16();
            int numChannels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int byteRate = reader.ReadInt32();
            int blockAlign = reader.ReadInt16();
            int bitsPerSample = reader.ReadInt16();

            var dataSignature = new string(reader.ReadChars(4));
            //check if the data dignature for this chunk is LIST headers, can happen with .wav files from web, or when converting other formats such as .mp3 to .wav using tools such as audacity
            //if this happens, the extra header info can be cleared using audacity, export to wave and select 'clear' when the extra header info window appears
            //see http://www.lightlink.com/tjweber/StripWav/WAVE.html
            if (dataSignature == "LIST") throw new NotSupportedException("Specified wave file contains LIST headers (author, copyright etc).");
            if (dataSignature != "data") throw new NotSupportedException("Specified wave file is not supported.");

            int dataChunkSize = reader.ReadInt32();

            channels = numChannels;
            bits = bitsPerSample;
            rate = sampleRate;

            return reader.ReadBytes((int)reader.BaseStream.Length);
        }
        // ReSharper restore UnusedVariable
    }

    private static ALFormat GetSoundFormat(int channels, int bits)
    {
        switch (channels)
        {
            case 1: return bits == 8 ? ALFormat.Mono8 : ALFormat.Mono16;
            case 2: return bits == 8 ? ALFormat.Stereo8 : ALFormat.Stereo16;
            default: throw new NotSupportedException("The specified sound format is not supported.");
        }
    }

    internal static void Dispose()
    {
        if (_audioContext == null) return;
        StopAllSounds();
        AL.DeleteSources(_source); //delete multiple sources
        AL.DeleteBuffers(_buffer); //delete multiple buffers
        _audioContext.Dispose();
    }
    #endregion

    #region Play
    /// <summary>Plays a sound.</summary>
    /// <param name="sound">sound type to play</param>
    internal static void PlaySound(SoundType sound)
    {
        if (!SoundEnabled) return;
        Play(sound);
    }

    /// <summary>Plays a sound only if the sound is not already playing. Some sounds would sound strange restarting without having played completely.</summary>
    /// <param name="sound">sound type to play</param>
    /// /// <param name="gain">volume, 0 none -> 1 full</param>
    internal static void PlaySoundIfNotAlreadyPlaying(SoundType sound, float gain = 1f)
    {
        //if (!Config.SoundEnabled) return;
        if (AL.GetSourceState(_source[(byte)sound]) == ALSourceState.Playing) return;
        Play(sound, gain);
    }

    /// <summary>Plays a sound at specified volume.</summary>
    /// <param name="sound">sound type to play</param>
    /// <param name="gain">volume, 0 none -> 1 full</param>
    internal static void PlaySound(SoundType sound, float gain)
    {
        if (!SoundEnabled) return;
        Play(sound, gain);
    }

    /// <summary>Plays a sound at specified volume that loops infinitely.</summary>
    /// <param name="sound">sound type to play</param>
    /// <param name="gain">volume, 0 none -> 1 full</param>
    internal static void PlaySoundLooping(SoundType sound, float gain)
    {
        if (!SoundEnabled) return;
        Play(sound, gain, true);
    }

    /// <summary>Plays a sound with a volume relative to how far from the listener it is.</summary>
    /// <param name="sound">sound type to play</param>
    /// <param name="sourceCoords">source coords of the sound</param>
    /// <param name="maxDistance">max distance the sound can be heard</param>
    internal static void PlaySound(SoundType sound, byte maxDistance = 25)
    {
        if (!SoundEnabled) return;
        float gain = 1;
        Play(sound, gain);
    }


    /// <summary>Plays a sound.</summary>
    /// <param name="sound">sound type to play</param>
    /// <param name="gain">volume, 0 none -> 1 full</param>
    /// <param name="looping">should sound loop infinitely</param>
    private static void Play(SoundType sound, float gain = 1, bool looping = false)
    {
        p pp = new p();
        pp.sound = sound;
        pp.gain = gain;
        pp.looping = looping;
        System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(pp.PlayerThread));
        t.Start();
    }
    class p
    {
        public SoundType sound;
        public float gain = 1;
        public bool looping = false;
        public void PlayerThread()
        {
            if (gain <= 0.05) return; //dont bother playing
            if (gain > 1) gain = 1; //play at max volume
            try
            {
                int source = _source[(byte)sound]; //id of the source
                int buffer = _buffer[(byte)sound]; //id of the buffer
                AL.Source(source, ALSourcei.Buffer, buffer);
                AL.Source(source, ALSourcef.Gain, gain); //volume
                if (looping) AL.Source(source, ALSourceb.Looping, true); //only make AL call if we actually need to loop as this is rare
                AL.SourcePlay(source);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }

    #endregion

    #region Stop
    internal static void StopSound(SoundType sound)
    {
        try
        {
            AL.SourceStop(_source[(byte)sound]);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    internal static void StopAllSounds()
    {
        AL.SourceStop(_source.Length, _source); //stop multiple sources
    }
    #endregion

}
}
