// Copied and adapted from https://github.com/AvaloniaUI/Avalonia/blob/release/11.3.0/samples/GpuInterop/D3DDemo/D3D11Swapchain.cs

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;

using SharpDX.Direct3D11;
using SharpDX.DXGI;

using D3DDevice = SharpDX.Direct3D11.Device;
using DxgiResource = SharpDX.DXGI.Resource;

namespace fin.ui.avalonia.gl;

class D3d11Swapchain {
  protected ICompositionGpuInterop Interop { get; }
  protected CompositionDrawingSurface Target { get; }
  private readonly List<D3D11SwapchainImage> pendingImages_ = [];
  private readonly D3DDevice device_;

  public D3d11Swapchain(
      D3DDevice device,
      ICompositionGpuInterop interop,
      CompositionDrawingSurface target
  ) {
    this.Interop = interop;
    this.Target = target;
    this.device_ = device;
  }

  D3D11SwapchainImage? CleanupAndFindNextImage_(PixelSize size) {
    D3D11SwapchainImage? firstFound = null;
    var foundMultiple = false;

    for (var c = this.pendingImages_.Count - 1; c > -1; c--) {
      var image = this.pendingImages_[c];
      var ready =
          image.LastPresent == null ||
          image.LastPresent.Status == TaskStatus.RanToCompletion;
      var matches = image.Size == size;
      if (image.LastPresent?.IsFaulted == true || (!matches && ready)) {
        _ = image.DisposeAsync();
        this.pendingImages_.RemoveAt(c);
      }

      if (matches && ready) {
        if (firstFound == null)
          firstFound = image;
        else
          foundMultiple = true;
      }
    }

    // We are making sure that there was at least one image of the same size in flight
    // Otherwise we might encounter UI thread lockups
    return foundMultiple ? firstFound : null;
  }

  public async ValueTask DisposeAsync() {
    foreach (var img in this.pendingImages_)
      await img.DisposeAsync();
  }

  class AnonymousDisposable : IDisposable {
    private volatile Action? dispose_;

    public AnonymousDisposable(Action dispose) {
      this.dispose_ = dispose;
    }

    public void Dispose() {
      Interlocked.Exchange(ref this.dispose_, null)?.Invoke();
    }
  }

  public IDisposable BeginDraw(PixelSize size, out D3D11SwapchainImage image) {
    var img = this.CleanupAndFindNextImage_(size) ??
              new(this.device_, size, this.Interop, this.Target);

    this.pendingImages_.Remove(img);
    var rv = new AnonymousDisposable(() => {
      this.pendingImages_.Add(img);
    });
    image = img;
    return rv;
  }
}

public sealed class D3D11SwapchainImage {
  public PixelSize Size { get; }
  private readonly ICompositionGpuInterop interop_;
  private readonly CompositionDrawingSurface target_;
  private readonly Texture2D texture_;
  public Texture2D Texture => this.texture_;
  private readonly KeyedMutex mutex_;
  private readonly PlatformHandle platformHandle_;
  private PlatformGraphicsExternalImageProperties properties_;
  private ICompositionImportedGpuImage? imported_;
  public Task? LastPresent { get; private set; }
  public RenderTargetView RenderTargetView { get; }

  public D3D11SwapchainImage(
      D3DDevice device,
      PixelSize size,
      ICompositionGpuInterop interop,
      CompositionDrawingSurface target
  ) {
    this.Size = size;
    this.interop_ = interop;
    this.target_ = target;
    this.texture_ = new Texture2D(
        device,
        new Texture2DDescription {
            Format = Format.B8G8R8A8_UNorm,
            Width = size.Width,
            Height = size.Height,
            ArraySize = 1,
            MipLevels = 1,
            SampleDescription
                = new SampleDescription { Count = 1, Quality = 0 },
            CpuAccessFlags = default,
            OptionFlags = ResourceOptionFlags.SharedKeyedmutex,
            BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
        }
    );
    this.mutex_ = this.texture_.QueryInterface<KeyedMutex>();
    using (var res = this.texture_.QueryInterface<DxgiResource>()) {
      var handle = res.SharedHandle;
      this.platformHandle_ = new PlatformHandle(
          handle,
          KnownPlatformGraphicsExternalImageHandleTypes
              .D3D11TextureGlobalSharedHandle
      );
    }
    this.properties_ = new PlatformGraphicsExternalImageProperties {
        Width = size.Width,
        Height = size.Height,
        Format = PlatformGraphicsExternalImageFormat.B8G8R8A8UNorm
    };

    this.RenderTargetView = new RenderTargetView(device, this.texture_);
  }

  public void BeginDraw() {
    this.mutex_.Acquire(0, int.MaxValue);
  }

  public void Present() {
    this.mutex_.Release(1);
    this.imported_ ??= this.interop_.ImportImage(
        this.platformHandle_,
        this.properties_
    );
    this.LastPresent = this.target_.UpdateWithKeyedMutexAsync(this.imported_, 1, 0);
  }

  public async ValueTask DisposeAsync() {
    if (this.LastPresent != null)
      try {
        await this.LastPresent;
      } catch {
        // Ignore
      }

    this.RenderTargetView.Dispose();
    this.mutex_.Dispose();
    this.texture_.Dispose();
  }
}