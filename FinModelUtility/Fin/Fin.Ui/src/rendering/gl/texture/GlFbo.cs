using System.Runtime.CompilerServices;

using fin.data.disposables;

using OpenTK.Graphics.ES30;

using GL0 = OpenTK.Graphics.OpenGL.GL;
using PixelFormat = OpenTK.Graphics.ES30.PixelFormat;
using PixelInternalFormat = OpenTK.Graphics.OpenGL.PixelInternalFormat;
using TextureMagFilter = OpenTK.Graphics.ES30.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.ES30.TextureMinFilter;

namespace fin.ui.rendering.gl.texture;

public sealed class GlFbo : IFinDisposable {
  private int fboId_;
  private int colorTextureId_;
  private int depthTextureId_;

  public GlFbo(int width, int height) {
    this.Width = width;
    this.Height = height;

    GlUtil.AssertNoErrorsWhenDebugging();
    // Create Color Tex
    GL.GenTextures(1, out this.colorTextureId_);
    GL.BindTexture(TextureTarget.Texture2D, this.colorTextureId_);
    GL.TexImage2D(TextureTarget2d.Texture2D,
                  0,
                  TextureComponentCount.Rgba8,
                  width,
                  height,
                  0,
                  PixelFormat.Rgba,
                  PixelType.UnsignedByte,
                  IntPtr.Zero);
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Linear);
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapS,
                    (int) TextureWrapMode.ClampToBorder);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapT,
                    (int) TextureWrapMode.ClampToBorder);
    GlUtil.AssertNoErrorsWhenDebugging();

    // Create Depth Tex
    GL.GenTextures(1, out this.depthTextureId_);
    GL.BindTexture(TextureTarget.Texture2D, this.depthTextureId_);
    GL0.TexImage2D(OpenTK.Graphics.OpenGL.TextureTarget.Texture2D,
                   0,
                   PixelInternalFormat.DepthComponent,
                   width,
                   height,
                   0,
                   OpenTK.Graphics.OpenGL.PixelFormat.DepthComponent,
                   OpenTK.Graphics.OpenGL.PixelType.UnsignedInt,
                   IntPtr.Zero);
    // things go horribly wrong if DepthComponent's Bitcount does not match the main Framebuffer's Depth
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMinFilter,
                    (int) TextureMinFilter.Linear);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureMagFilter,
                    (int) TextureMagFilter.Linear);
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapS,
                    (int) TextureWrapMode.ClampToBorder);
    GL.TexParameter(TextureTarget.Texture2D,
                    TextureParameterName.TextureWrapT,
                    (int) TextureWrapMode.ClampToBorder);

    // Create a FBO and attach the textures
    GL.GenFramebuffers(1, out this.fboId_);
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.fboId_);
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.ColorAttachment0,
                            TextureTarget2d.Texture2D,
                            this.colorTextureId_,
                            0);
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.FramebufferTexture2D(FramebufferTarget.Framebuffer,
                            FramebufferAttachment.DepthAttachment,
                            TextureTarget2d.Texture2D,
                            this.depthTextureId_,
                            0);
    GlUtil.AssertNoErrorsWhenDebugging();
  }

  ~GlFbo() => this.ReleaseUnmanagedResources_();

  public bool IsDisposed { get; private set; }

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    this.IsDisposed = true;

    GlUtil.AssertNoErrorsWhenDebugging();
    GL.DeleteFramebuffers(1, ref this.fboId_);
    GL.DeleteTextures(1, ref this.colorTextureId_);
    GL.DeleteTextures(1, ref this.depthTextureId_);
    GlUtil.AssertNoErrorsWhenDebugging();
  }

  public int Width { get; }
  public int Height { get; }
  public int ColorTextureId => this.colorTextureId_;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int textureIndex = 0)
    => GlUtil.BindTexture(textureIndex, this.colorTextureId_);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void BindDepth(int textureIndex = 0)
    => GlUtil.BindTexture(textureIndex, this.depthTextureId_);

  public void TargetFbo() {
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.BindFramebuffer(FramebufferTarget.Framebuffer, this.fboId_);
    GlUtil.AssertNoErrorsWhenDebugging();
  }

  public void UntargetFbo()
    => GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
}