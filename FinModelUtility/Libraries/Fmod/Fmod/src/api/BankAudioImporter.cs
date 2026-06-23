using fin.audio;
using fin.audio.io;
using fin.audio.io.importers;
using fin.audio.io.importers.ogg;
using fin.audio.io.importers.wav;
using fin.io;
using fin.util.asserts;
using fin.util.sets;

namespace fmod.api;

public record BankAudioFileBundle(IReadOnlyTreeFile BankFile)
    : IAudioFileBundle {
  public IReadOnlyTreeFile? MainFile => this.BankFile;
}

public sealed class BankAudioImporter : IAudioImporter<BankAudioFileBundle> {
  public ILoadedAudioBuffer<short>[] ImportAudio(
      IAudioManager<short> audioManager,
      BankAudioFileBundle fileBundle) {
    var bankFile = fileBundle.BankFile;
    var fmodReader = FModBankParser.FModBankParser.LoadSoundBank(
        new FileInfo(bankFile.FullPath));

    var fmodSamples = fmodReader.SoundBankData.SelectMany(bank => bank.Samples);

    return fmodSamples.Select(fmodSample => {
      var finBuffer = audioManager.CreateLoadedAudioBuffer(fileBundle,
        bankFile.AsFileSet());

      var fmodMetadata = fmodSample.Metadata;
      finBuffer.Frequency = fmodMetadata.Frequency;

      Asserts.True(
          fmodSample.RebuildAsStandardFileFormat(
              out var data,
              out var fileExtension), "Failed to rebuild FMOD as standard file format");

      using var ms = new MemoryStream(data!);

      return fileExtension.ToLower() switch {
          "wav" => WavAudioImporter.ImportAudio(audioManager, fileBundle, ms),
          "ogg" => OggAudioImporter.ImportAudio(audioManager, fileBundle, ms),
      };
    }).ToArray();
  }
}