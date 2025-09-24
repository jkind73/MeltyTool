// Copied and adapted from https://github.com/AvaloniaUI/Avalonia/blob/release/11.3.0/samples/GpuInterop

using System;
using System.Linq;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Platform;
using Avalonia.Rendering;
using Avalonia.Rendering.Composition;

using fin.ui.rendering.gl;

using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics.Wgl;
using OpenTK.Platform.Windows;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace fin.ui.avalonia.gl;

/// <summary>
///   Shamelessly stolen from:
///   https://github.com/Dragorn421/DragoStuff/blob/aa1ac3434f2701739570adf77377c29e1e3171c1/SharpDXInteropControl.cs
/// </summary>
public abstract class SharpDxInteropControl : Control {
  private CompositionSurfaceVisual? visual_;
  private Compositor? compositor_;
  private string info_ = string.Empty;
  private bool updateQueued_;
  private bool initialized_;

  public event Action? OnInit;

  protected abstract void InitGl();
  protected abstract void RenderGl();
  protected abstract void TeardownGl();

  protected CompositionDrawingSurface? Surface { get; private set; }

  public SharpDxInteropControl() {
    this.SizeChanged += (sender, e) => {
      this.QueueNextFrame_();
    };
  }

  protected override void OnAttachedToVisualTree(
      VisualTreeAttachmentEventArgs e) {
    base.OnAttachedToVisualTree(e);
    this.Initialize_();
  }

  protected override void OnDetachedFromLogicalTree(
      LogicalTreeAttachmentEventArgs e) {
    if (this.initialized_) {
      this.Surface?.Dispose();
      this.FreeGraphicsResources();
    }

    this.initialized_ = false;
    base.OnDetachedFromLogicalTree(e);
  }

  private async void Initialize_() {
    var selfVisual = ElementComposition.GetElementVisual(this)!;
    this.compositor_ = selfVisual.Compositor;

    this.Surface = this.compositor_.CreateDrawingSurface();
    this.visual_ = this.compositor_.CreateSurfaceVisual();
    this.visual_.Size = new(this.Bounds.Width, this.Bounds.Height);
    this.visual_.Surface = this.Surface;
    ElementComposition.SetElementChildVisual(this, this.visual_);
    var interop = await this.compositor_.TryGetCompositionGpuInterop();
    bool res;
    string info;
    if (interop == null)
      (res, info) = (
          false, "Compositor doesn't support interop for the current backend");
    else
      (res, info) = this.InitializeGraphicsResources(this.Surface, interop);
    this.info_ = info;
    this.initialized_ = res;
    
    var bindingsContext = new GLFWBindingsContext();
    Wgl.LoadBindings(bindingsContext);
    GL.LoadBindings(bindingsContext);
    
    GlUtil.SwitchContext(this.openTkWindow_!.Context);

    IntPtr hDc = wglGetCurrentDC();
    if (hDc == IntPtr.Zero)
      throw new InvalidOperationException(
          "No current hDC. Make sure OpenGL context is current."
      );
    string[] extensions = Wgl
                          .Arb.GetExtensionsString(hDc)
                          .Split(' ', StringSplitOptions.RemoveEmptyEntries);
    bool hasInterop = extensions.Contains("WGL_NV_DX_interop");
    if (!hasInterop)
      throw new PlatformNotSupportedException(
          "NV_DX_interop not available on this device."
      );

    this.InitGl();
    this.OnInit?.Invoke();

    this.QueueNextFrame_();
  }

  private void QueueNextFrame_() {
    if (this.initialized_ && !this.updateQueued_ && this.compositor_ != null) {
      this.updateQueued_ = true;
      this.compositor_?.RequestCompositionUpdate(this.UpdateFrame_);
    }
  }

  private void UpdateFrame_() {
    this.updateQueued_ = false;
    var root = this as IRenderRoot ?? this.VisualRoot;
    if (root == null)
      return;

    this.visual_!.Size = new(this.Bounds.Width, this.Bounds.Height);
    var size = PixelSize.FromSize(this.Bounds.Size, root.RenderScaling);
    this.RenderFrame(size);

    this.QueueNextFrame_();
  }

  private Device? device_;
  private D3d11Swapchain? swapchain_;
  private DeviceContext? context_;
  private PixelSize lastSize_;

  private NativeWindow? openTkWindow_;

  protected (bool success, string info) InitializeGraphicsResources(
      CompositionDrawingSurface surface,
      ICompositionGpuInterop interop
  ) {
    if (
        interop.SupportedImageHandleTypes.Contains(
            KnownPlatformGraphicsExternalImageHandleTypes
                .D3D11TextureGlobalSharedHandle
        ) !=
        true
    )
      return (
          false,
          "DXGI shared handle import is not supported by the current graphics backend"
      );

    var factory = new SharpDX.DXGI.Factory1();
    using var adapter = factory.GetAdapter1(0);
    this.device_ = new Device(
        adapter,
        DeviceCreationFlags.None,
        new[] {
            FeatureLevel.Level_12_1,
            FeatureLevel.Level_12_0,
            FeatureLevel.Level_11_1,
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_0,
            FeatureLevel.Level_9_3,
            FeatureLevel.Level_9_2,
            FeatureLevel.Level_9_1,
        }
    );
    this.swapchain_ = new D3d11Swapchain(this.device_, interop, surface);
    this.context_ = this.device_.ImmediateContext;

    this.openTkWindow_ = new NativeWindow(
        new() { StartVisible = false, ClientSize = new(100, 100) }
    );

    return (
        true,
        $"D3D11 ({this.device_.FeatureLevel}) {adapter.Description1.Description}");
  }

  protected void FreeGraphicsResources() {
    if (this.swapchain_ is not null) {
      this.swapchain_.DisposeAsync().GetAwaiter().GetResult();
      this.swapchain_ = null;
    }

    Utilities.Dispose(ref this.context_);
    Utilities.Dispose(ref this.device_);

    this.openTkWindow_?.Dispose();
    this.openTkWindow_ = null;
  }

  [DllImport("opengl32.dll")]
  private static extern IntPtr wglGetCurrentDC();

  protected void RenderFrame(PixelSize pixelSize) {
    if (pixelSize == default)
      return;
    if (pixelSize != this.lastSize_) {
      this.lastSize_ = pixelSize;
      this.Resize_(pixelSize);
    }

    using (this.swapchain_!.BeginDraw(pixelSize, out var image)) {
      this.device_!.ImmediateContext.OutputMerger.SetTargets(image.RenderTargetView);

      GlUtil.SwitchContext(this.openTkWindow_!.Context);

      var hDevice = Wgl.DXOpenDeviceNV(this.device_.NativePointer);
      if (hDevice == IntPtr.Zero) {
        throw new Exception("DXOpenDeviceNV failed");
      }

      GL.GenTextures(1, out uint glName);

      var hCfb = Wgl.DXRegisterObjectNV(
          hDevice,
          image.Texture.NativePointer, // wrong?
          glName,
          (int) TextureTarget2d.Texture2D,
          WGL_NV_DX_interop.AccessReadWrite
      );

      if (hCfb == IntPtr.Zero) {
        throw new Exception("DXRegisterObjectNV failed");
      }

      var lockResult = Wgl.DXLockObjectsNV(hDevice, 1, [hCfb]);
      if (!lockResult) {
        throw new Exception($"DXLockObjectsNV failed {GetLastError()}");
      }

      var framebufferName = GL.GenFramebuffer();
      GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferName);
      GL.FramebufferTexture(
          FramebufferTarget.Framebuffer,
          FramebufferAttachment.ColorAttachment0,
          glName,
          0
      );
      GL.DrawBuffer(DrawBufferMode.ColorAttachment0);
      var fbStatus = GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
      if (fbStatus != FramebufferErrorCode.FramebufferComplete) {
        throw new Exception($"incomplete framebuffer: {fbStatus}");
      }

      this.RenderGl();

      var unlockResult = Wgl.DXUnlockObjectsNV(hDevice, 1, [hCfb]);
      if (!unlockResult) {
        throw new Exception($"DXUnlockObjectsNV failed {GetLastError()}");
      }

      Wgl.DXUnregisterObjectNV(hDevice, hCfb);
      Wgl.DXCloseDeviceNV(hDevice);

      this.openTkWindow_.Context.MakeNoneCurrent();
      this.context_!.Flush();
    }
  }

  [DllImport("Kernel32.dll")]
  public static extern int GetLastError();

  private void Resize_(PixelSize size) {
    if (this.device_ is null)
      return;

    // Setup targets and viewport for rendering
    this.device_.ImmediateContext.Rasterizer.SetViewport(
        0,
        0,
        size.Width,
        size.Height);

    this.openTkWindow_!.ClientSize = (size.Width, size.Height);
    this.openTkWindow_.Context.MakeCurrent();
    GL.Viewport(0, 0, size.Width, size.Height);
    this.openTkWindow_.Context.MakeNoneCurrent();
  }
}