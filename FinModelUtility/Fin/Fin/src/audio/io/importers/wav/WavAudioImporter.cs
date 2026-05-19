using System;
using System.IO;

using fin.io;
using fin.util.sets;

using NAudio.Wave;

namespace fin.audio.io.importers.wav;

public sealed class WavAudioFileBundle(IReadOnlyTreeFile oggFile)
    : IAudioFileBundle {
  public string? GameName { get; init; }
  public IReadOnlyTreeFile MainFile => this.WavFile;

  public IReadOnlyTreeFile WavFile { get; } = oggFile;
}

public sealed class WavAudioImporter : IAudioImporter<WavAudioFileBundle> {
  public ILoadedAudioBuffer<short>[] ImportAudio(
      IAudioManager<short> audioManager,
      WavAudioFileBundle audioFileBundle) {
    using var wavStream = audioFileBundle.WavFile.OpenRead();
    return [ImportAudio(audioManager, audioFileBundle, wavStream)];
  }

  public static ILoadedAudioBuffer<short> ImportAudio(
      IAudioManager<short> audioManager,
      IAudioFileBundle audioFileBundle,
      Stream oggStream) {
    var mutableBuffer = audioManager.CreateLoadedAudioBuffer(
        audioFileBundle,
        audioFileBundle.MainFile.AsFileSet());
    ImportAudioImpl_(mutableBuffer, oggStream);
    return mutableBuffer;
  }

  private static void ImportAudioImpl_(
      IAudioBuffer<short> mutableBuffer,
      Stream wavStream) {
    using var wavFileReader = new WaveFileReader(wavStream);

    mutableBuffer.Frequency = wavFileReader.WaveFormat.SampleRate;

    var channelCount = wavFileReader.WaveFormat.Channels;

    {
      var sampleCount = (int) wavFileReader.SampleCount;

      var channels = new short[channelCount][];
      for (var c = 0; c < channelCount; ++c) {
        channels[c] = new short[sampleCount];
      }

      for (var i = 0; i < sampleCount; ++i) {
        // TODO: This is awful, wish it used spans
        var nextSampleFrame = wavFileReader.ReadNextSampleFrame();

        for (var c = 0; c < channelCount; ++c) {
          var floatSample = nextSampleFrame[c];

          var floatMin = -1f;
          var floatMax = 1f;

          var normalizedFloatSample =
              (MathF.Max(floatMin, Math.Min(floatSample, floatMax)) -
               floatMin) / (floatMax - floatMin);

          float shortMin = short.MinValue;
          float shortMax = short.MaxValue;

          var shortSample = (short) (shortMin +
                                     normalizedFloatSample *
                                     (shortMax - shortMin));

          channels[c][i] = shortSample;
        }
      }

      mutableBuffer.SetPcm(channels);
    }
  }
}