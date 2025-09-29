using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering;
using Avalonia.Threading;

using fin.config;

namespace fin.ui.avalonia.gl;

public abstract class BGlPanel : Panel, ICustomHitTest {
  public event Action? OnInit;

  protected BGlPanel() {
    Dispatcher.UIThread.InvokeAsync(async () => {
      var initGl = () => {
        this.InitGl();
        this.OnInit?.Invoke();
      };
      var renderGl = this.RenderGl;
      var teardownGl = this.TeardownGl;

      if (FinConfig.PreferGlNativeInterop) {
        if (await SharpDxInteropControl.TryToAddTo(this, initGl, renderGl, teardownGl)) {
          return;
        }
      }

      this.Children.Add(new OpenTkControl(initGl, renderGl, teardownGl));
    });

  }

  protected abstract void InitGl();
  protected abstract void RenderGl();
  protected abstract void TeardownGl();

  public bool HitTest(Point point) => this.Bounds.Contains(point);

  protected void GetBoundsForGlViewport(out int width, out int height) {
    var scaling = 1f;
    if (TopLevel.GetTopLevel(this) is Window window) {
      scaling = (float) window.RenderScaling;
    }

    var bounds = this.Bounds;
    width = (int) (scaling * bounds.Width);
    height = (int) (scaling * bounds.Height);
  }
}