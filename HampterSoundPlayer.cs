using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Hampter2
{
    public sealed class HampterSoundPlayer
    {
        private long position;

        private float[] samples;
        private WaveFormat waveFormat;

        private WaveOutEvent waveOut;
        private CallbackSampleProvider sampleProvider;

        private Thread playerThread;

        private object playPositionsLock = new object();
        private LinkedList<long> playPositions = new LinkedList<long>();
        public HampterSoundPlayer()
        {
            bool halt = true;
            playerThread = new Thread(() =>
            {
                AudioFileReader fileReader = new AudioFileReader("Hampter.wav");
                waveFormat = fileReader.WaveFormat;
                samples = new float[(fileReader.Length * 8) / waveFormat.BitsPerSample];
                fileReader.Read(samples, 0, samples.Length);
                fileReader.Dispose();

                sampleProvider = new CallbackSampleProvider(Read, () => { return waveFormat; });

                waveOut = new WaveOutEvent();
                waveOut.Volume = 1.0f;
                waveOut.DeviceNumber = -1;
                waveOut.DesiredLatency = 100;
                waveOut.Init(sampleProvider);

                halt = false;
                waveOut.Play();
            });
            playerThread.Start();
            while (halt) { }
        }
        public void Hampter()
        {
            lock (playPositionsLock)
            {
                playPositions.AddLast(position);
            }
        }
        private int Read(float[] buffer, int offset, int count)
        {
            if (offset < 0 || count < 0 || buffer is null || offset + count >= buffer.Length)
            {
                return 0;
            }

            for (int i = 0; i < count; i++)
            {
                buffer[offset + i] = 0.0f;
            }

            LinkedListNode<long> currentNode = playPositions.First;
            while (currentNode != null)
            {
                if (currentNode.Value + samples.Length < position)
                {
                    LinkedListNode<long> deleteMe = currentNode;
                    currentNode = currentNode.Next;
                    playPositions.Remove(deleteMe);
                }
                else
                {
                    int sourceIndex = (int)(position - currentNode.Value);
                    int length = count;
                    if (count > samples.Length - sourceIndex)
                    {
                        length = samples.Length - sourceIndex;
                    }
                    for (int i = 0; i < length; i++)
                    {
                        buffer[offset + i] += samples[sourceIndex + i];
                    }
                    currentNode = currentNode.Next;
                }
            }

            position += count;
            return count;
        }
        // Adds length elements from source starting at sourceIndex to destination starting at destinationIndex.
        private static void ArrayAdd(float[] source, int sourceIndex, float[] destination, int destinationIndex, int length)
        {
            if (source is null)
            {
                throw new Exception("source may not be null.");
            }
            if (sourceIndex < 0 || sourceIndex > source.Length)
            {
                throw new Exception("sourceIndex must be within the bounds of source.");
            }
            if (destination is null)
            {
                throw new Exception("destination may not be null.");
            }
            if (destinationIndex < 0 || destinationIndex > destination.Length)
            {
                throw new Exception("destinationIndex must be within the bounds of destination.");
            }
            if (length < 0)
            {
                throw new Exception("length must be greater than or equal to 0.");
            }
            if (sourceIndex + length > source.Length)
            {
                throw new Exception("sourceIndex + length must be within the bounds of source.");
            }
            if (destinationIndex + length > destination.Length)
            {
                throw new Exception("destinationIndex + length must be within the bounds of destination.");
            }
            for (int i = 0; i < length; i++)
            {
                destination[destinationIndex + i] += source[sourceIndex + i];
            }
        }
        private sealed class CallbackSampleProvider : ISampleProvider
        {
            public delegate int ReadDelegate(float[] buffer, int offset, int count);
            public delegate WaveFormat GetWaveFormatDelegate();
            private ReadDelegate ReadCallback = null;
            private GetWaveFormatDelegate GetWaveFormatCallback = null;
            public CallbackSampleProvider(ReadDelegate readCallback, GetWaveFormatDelegate getWaveFormatCallback)
            {
                if (readCallback is null)
                {
                    throw new Exception("readCallback may not be null.");
                }
                if (getWaveFormatCallback is null)
                {
                    throw new Exception("getWaveFormatCallback may not be null.");
                }
                ReadCallback = readCallback;
                GetWaveFormatCallback = getWaveFormatCallback;
            }
            public WaveFormat WaveFormat
            {
                get
                {
                    return GetWaveFormatCallback();
                }
            }
            public int Read(float[] buffer, int offset, int count)
            {
                return ReadCallback(buffer, offset, count);
            }
        }
    }
}