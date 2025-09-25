using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Rendering;

namespace fin.ui.avalonia.gl;

public abstract class BGlPanel : Panel, ICustomHitTest {
  public event Action? OnInit;

  protected BGlPanel() {
    var child = new OpenTkControl(
        () => {
          this.InitGl();
          this.OnInit?.Invoke();
        },
        this.RenderGl,
        this.TeardownGl);
    this.VisualChildren.Add(child);
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