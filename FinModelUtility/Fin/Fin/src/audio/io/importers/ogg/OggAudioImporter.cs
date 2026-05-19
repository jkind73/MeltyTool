using System;
using System.IO;

using fin.util.sets;

using NVorbis;

namespace fin.audio.io.importers.ogg;

public sealed class OggAudioImporter : IAudioImporter<OggAudioFileBundle> {
  public ILoadedAudioBuffer<short>[] ImportAudio(
      IAudioManager<short> audioManager,
      OggAudioFileBundle audioFileBundle) {
    using var oggStream = audioFileBundle.OggFile.OpenRead();
    return [ImportAudio(audioManager, audioFileBundle, oggStream)];
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

  public static IAudioBuffer<short> ImportAudio(
      IAudioManager<short> audioManager,
      Stream oggStream) {
    var mutableBuffer = audioManager.CreateAudioBuffer();
    ImportAudioImpl_(mutableBuffer, oggStream);
    return mutableBuffer;
  }

  private static void ImportAudioImpl_(
      IAudioBuffer<short> mutableBuffer,
      Stream oggStream) {
    using var ogg = new VorbisReader(oggStream);
    mutableBuffer.Frequency = ogg.SampleRate;

    {
      var sampleCount = (int) ogg.TotalSamples;

      var channelCount = ogg.Channels;
      var floatCount = channelCount * sampleCount;
      var floatPcm = new float[floatCount];
      ogg.ReadSamples(floatPcm);

      var channels = new short[channelCount][];
      for (var c = 0; c < channelCount; ++c) {
        channels[c] = new short[sampleCount];
      }

      for (var i = 0; i < sampleCount; ++i) {
        for (var c = 0; c < channelCount; ++c) {
          var floatSample = floatPcm[channelCount * i + c];

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