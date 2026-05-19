using System;
using System.Linq;

using fin.audio;
using fin.audio.io.importers.ogg;
using fin.ui.playback.al;

using marioartisttool.util;

namespace marioartisttool.view.games.ball;

public sealed class BallAudioManager {
  private readonly IAudioManager<short> impl_;

  private readonly IAudioBuffer<short>[] ballTickAudioBuffers_;
  private readonly IAudioBuffer<short> ballJuggleAudioBuffer_;

  public BallAudioManager(uint ballCount) {
    this.impl_ = AlAudioManager.TryToCreateOrStub();

    this.ballTickAudioBuffers_
        = Enumerable
          .Range(0, (int) ballCount)
          .Select(i => LoadAudioBufferFromAsset_(
                      this.impl_,
                      $"gawg/ball/tick_{i}.ogg"))
          .ToArray();
    this.ballJuggleAudioBuffer_ = LoadAudioBufferFromAsset_(
        this.impl_,
        "gawg/ball/juggle.ogg");
  }

  private static IAudioBuffer<short> LoadAudioBufferFromAsset_(
      IAudioManager<short> audioManager,
      string assetPath) {
    using var oggStream = AssetLoaderUtil.Open(assetPath);
    return OggAudioImporter.ImportAudio(audioManager, oggStream);
  }

  ~BallAudioManager() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() => this.impl_?.Dispose();

  public void PlayBallTickAudio(uint index)
    => this.impl_.AudioPlayer.CreatePlayback(this.ballTickAudioBuffers_[index]).Play();

  public void PlayBallJuggleAudio()
    => this.impl_.AudioPlayer.CreatePlayback(this.ballJuggleAudioBuffer_).Play();
}