using System;

using Avalonia.OpenGL;

using OpenTK;

namespace fin.ui.avalonia.gl {
  public sealed class AvaloniaOpenTkContext(GlInterface glInterface)
      : IBindingsContext {
    public IntPtr GetProcAddress(string procName)
      => glInterface.GetProcAddress(procName);
  }
}