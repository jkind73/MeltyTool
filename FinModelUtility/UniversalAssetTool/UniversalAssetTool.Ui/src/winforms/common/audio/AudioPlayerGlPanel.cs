using System;
using System.Collections.Generic;
using System.Drawing;

using fin.audio;
using fin.audio.io;
using fin.data;
using fin.ui.playback.al;
using fin.ui.rendering.gl;
using fin.util.time;

using uni.api;

namespace uni.ui.winforms.common.audio;

public sealed class AudioPlayerGlPanel : BGlPanel, IAudioPlayerPanel {
  private IReadOnlyList<IAudioFileBundle>? audioFileBundles_;
  private ShuffledListView<IAudioFileBundle>? shuffledListView_;

  private readonly IAudioManager<short> audioManager_ =
      AlAudioManager.TryToCreateOrStub();

  private readonly IAudioPlayer<short> audioPlayer_;

  private readonly AotWaveformRenderer waveformRenderer_ = new();

  private readonly TimedCallback playNextCallback_;

  public AudioPlayerGlPanel() {
    this.Disposed += (_, _) => this.audioManager_.Dispose();

    this.audioPlayer_ = this.audioManager_.AudioPlayer;

    var playNextLock = new object();
    this.playNextCallback_ = new TimedCallback(() => {
                                                 lock (playNextLock) {
                                                   if (this.shuffledListView_ ==
                                                    null) {
                                                     return;
                                                   }

                                                   var activeSound
                                                       = this.waveformRenderer_
                                                           .ActivePlayback;
                                                   if (activeSound?.State ==
                                                    PlaybackState.PLAYING) {
                                                     return;
                                                   }

                                                   this.waveformRenderer_
                                                       .ActivePlayback = null;
                                                   activeSound?.Stop();
                                                   activeSound?.Dispose();

                                                   if (this.shuffledListView_
                                                    .TryGetNext(
                                                        out var
                                                            audioFileBundle)) {
                                                     var audioBuffer
                                                         = new
                                                             GlobalAudioReader()
                                                         .ImportAudio(
                                                             this.audioManager_,
                                                             audioFileBundle);

                                                     activeSound
                                                         = this
                                                                 .waveformRenderer_
                                                                 .ActivePlayback
                                                             =
                                                             this.audioPlayer_
                                                                 .CreatePlayback(
                                                                     audioBuffer
                                                                         [0]);
                                                     activeSound.Volume = .1f;
                                                     activeSound.Play();

                                                     this.OnChange(
                                                         audioFileBundle);
                                                   }
                                                 }
                                               },
                                               .1f);
  }

  /// <summary>
  ///   Sets the audio file bundles to play in the player.
  /// </summary>
  public IReadOnlyList<IAudioFileBundle>? AudioFileBundles {
    get => this.audioFileBundles_;
    set {
      var originalValue = this.audioFileBundles_;
      this.audioFileBundles_ = value;

      this.waveformRenderer_.ActivePlayback?.Stop();
      this.waveformRenderer_.ActivePlayback = null;

      this.shuffledListView_
          = value != null
              ? new ShuffledListView<IAudioFileBundle>(value)
              : null;

      if (value == null && originalValue != null) {
        this.OnChange(null);
      }
    }
  }

  public event Action<IAudioFileBundle?> OnChange = delegate { };

  protected override void InitGl() => GlUtil.InitGl();

  protected override void RenderGl() {
    var width = this.Width;
    var height = this.Height;
    GlUtil.SetViewport(new Rectangle(0, 0, width, height));

    GlUtil.ClearColorAndDepth();

    this.RenderOrtho_();
  }

  private void RenderOrtho_() {
    var width = this.Width;
    var height = this.Height;

    {
      GlTransform.MatrixMode(TransformMatrixMode.PROJECTION);
      GlTransform.LoadIdentity();
      GlTransform.Ortho2d(0, width, height, 0);

      GlTransform.MatrixMode(TransformMatrixMode.VIEW);
      GlTransform.LoadIdentity();

      GlTransform.MatrixMode(TransformMatrixMode.MODEL);
      GlTransform.LoadIdentity();
    }

    var amplitude = height * .45f;
    this.waveformRenderer_.Width = width;
    this.waveformRenderer_.Amplitude = amplitude;
    this.waveformRenderer_.MiddleY = height / 2f;
    this.waveformRenderer_.Render();
  }
}