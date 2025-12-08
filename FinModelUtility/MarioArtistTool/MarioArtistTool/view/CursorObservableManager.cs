using System;
using System.Reactive.Subjects;

using Avalonia;
using Avalonia.Input;

using fin.ui.avalonia.observables;

using marioartisttool.util;

namespace marioartisttool.view;

public sealed class CursorObservableManager {
  private static readonly Cursor TSTLT_CURSOR_
      = AssetLoaderUtil.LoadCursor("thumb_out.png", new PixelPoint(2, 2));

  private static readonly Cursor MA3D1_CURSOR_
      = AssetLoaderUtil.LoadCursor("arrow.png", new PixelPoint(2, 2));

  private static readonly PixelPoint EYE_OFFSET_ = new PixelPoint(8, 8);

  private static readonly Cursor EYE_0_4_CURSOR_
      = AssetLoaderUtil.LoadCursor("eye_0_4.png", EYE_OFFSET_);

  private static readonly Cursor EYE_1_CURSOR_
      = AssetLoaderUtil.LoadCursor("eye_1.png", EYE_OFFSET_);

  private static readonly Cursor EYE_2_CURSOR_
      = AssetLoaderUtil.LoadCursor("eye_2.png", EYE_OFFSET_);

  private static readonly Cursor EYE_3_CURSOR_
      = AssetLoaderUtil.LoadCursor("eye_3.png", EYE_OFFSET_);

  private static readonly Cursor EYE_5_CURSOR_
      = AssetLoaderUtil.LoadCursor("eye_5.png", EYE_OFFSET_);

  private static readonly Cursor EYE_6_CURSOR_
      = AssetLoaderUtil.LoadCursor("eye_6.png", EYE_OFFSET_);

  private static readonly Cursor EYE_7_CURSOR_
      = AssetLoaderUtil.LoadCursor("eye_7.png", EYE_OFFSET_);

  private static readonly Cursor EYE_8_CURSOR_
      = AssetLoaderUtil.LoadCursor("eye_8.png", EYE_OFFSET_);

  private IDisposable? animationDisposable_;

  public ReplaySubject<Cursor> Cursor { get; } = new(1);

  public CursorObservableManager() {
    this.Cursor.OnNext(MA3D1_CURSOR_);
  }

public bool IsTstlt {
    get;
    set {
      field = value;
      this.UpdateState_();
    }
  }

  public bool IsMouseDown {
    get;
    set {
      field = value;
      this.UpdateState_();
    }
  }

  private void UpdateState_() {
    this.animationDisposable_?.Dispose();
    this.animationDisposable_ = null;

    if (!this.IsMouseDown) {
      this.Cursor.OnNext(this.IsTstlt ? TSTLT_CURSOR_ : MA3D1_CURSOR_);
      return;
    }

    var obs = new LoopingObservable<Cursor>(.1f, [
        EYE_0_4_CURSOR_,
        EYE_1_CURSOR_,
        EYE_2_CURSOR_,
        EYE_3_CURSOR_,
        EYE_0_4_CURSOR_,
        EYE_5_CURSOR_,
        EYE_6_CURSOR_,
        EYE_7_CURSOR_,
        EYE_8_CURSOR_
    ]);

    this.animationDisposable_ = obs.Subscribe(this.Cursor);
  }
}