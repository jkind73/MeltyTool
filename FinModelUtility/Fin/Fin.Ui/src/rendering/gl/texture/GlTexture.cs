using System.Buffers;
using System.Runtime.CompilerServices;

using fin.image;
using fin.image.formats;
using fin.math.floats;
using fin.model;
using fin.ui.rendering.gl.material;

using OpenTK.Graphics.OpenGL4;

using FinTextureMinFilter = fin.model.TextureMinFilter;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;
using TextureMagFilter = fin.model.TextureMagFilter;
using TextureMinFilter = OpenTK.Graphics.OpenGL4.TextureMinFilter;


namespace fin.ui.rendering.gl.texture;

public sealed class GlTexture : IGlTexture {
  private static readonly Dictionary<IReadOnlyTexture, GlTexture> cache_
      = new();


  private const int UNDEFINED_ID = -1;
  private readonly IReadOnlyTexture? texture_;

  public static GlTexture FromTexture(IReadOnlyTexture texture) {
    if (!cache_.TryGetValue(texture, out var glTexture)) {
      glTexture = new GlTexture(texture);
      cache_[texture] = glTexture;
    }

    return glTexture;
  }

  public GlTexture(IReadOnlyImage image) {
    GL.GenTextures(1, out int id);
    this.Id = id;

    var target = TextureTarget.Texture2D;
    GL.BindTexture(target, this.Id);
    {
      this.LoadImageIntoTexture_(image, 0);
    }
  }

  private GlTexture(IReadOnlyTexture texture) {
    this.texture_ = texture;

    FinTextureMinFilter minFilter;
    TextureMagFilter magFilter;
    if (!texture.ThreePointFiltering) {
      minFilter = texture.MinFilter;
      magFilter = texture.MagFilter;
    } else {
      // TODO: This is just an assumption for now, what should this be?
      minFilter = FinTextureMinFilter.NEAR;
      magFilter = TextureMagFilter.NEAR;
    }

    GL.GenTextures(1, out int id);
    this.Id = id;

    var target = TextureTarget.Texture2D;
    GL.BindTexture(target, this.Id);
    {
      var mipmapImages = texture.MipmapImages;

      this.LoadMipmapImagesIntoTexture_(mipmapImages);

      if (mipmapImages.Length == 1 &&
          minFilter is FinTextureMinFilter.NEAR_MIPMAP_NEAR
                       or FinTextureMinFilter.NEAR_MIPMAP_LINEAR
                       or FinTextureMinFilter.LINEAR_MIPMAP_NEAR
                       or FinTextureMinFilter.LINEAR_MIPMAP_LINEAR) {
        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
      } else {
        GL.TexParameter(target,
                        TextureParameterName.TextureMaxLevel,
                        mipmapImages.Length - 1);
      }

      var finBorderColor = texture.BorderColor;
      var hasBorderColor = finBorderColor != null;
      GL.TexParameter(target,
                      TextureParameterName.TextureWrapS,
                      (int) ConvertFinWrapToGlWrap_(
                          texture.WrapModeU,
                          hasBorderColor));
      GL.TexParameter(target,
                      TextureParameterName.TextureWrapT,
                      (int) ConvertFinWrapToGlWrap_(
                          texture.WrapModeV,
                          hasBorderColor));

      if (hasBorderColor) {
        var glBorderColor = new[] {
            finBorderColor.Rf,
            finBorderColor.Gf,
            finBorderColor.Bf,
            finBorderColor.Af
        };

        GL.TexParameter(target,
                        TextureParameterName.TextureBorderColor,
                        glBorderColor);
      }

      GL.TexParameter(
          target,
          TextureParameterName.TextureMinFilter,
          (int) (minFilter switch {
              FinTextureMinFilter.NEAR   => TextureMinFilter.Nearest,
              FinTextureMinFilter.LINEAR => TextureMinFilter.Linear,
              FinTextureMinFilter.NEAR_MIPMAP_NEAR => TextureMinFilter
                  .NearestMipmapNearest,
              FinTextureMinFilter.NEAR_MIPMAP_LINEAR => TextureMinFilter
                  .NearestMipmapLinear,
              FinTextureMinFilter.LINEAR_MIPMAP_NEAR => TextureMinFilter
                  .LinearMipmapNearest,
              FinTextureMinFilter.LINEAR_MIPMAP_LINEAR => TextureMinFilter
                  .LinearMipmapLinear,
          }));
      GL.TexParameter(
          target,
          TextureParameterName.TextureMagFilter,
          (int) (magFilter switch {
              TextureMagFilter.NEAR => OpenTK.Graphics.OpenGL.TextureMagFilter
                                             .Nearest,
              TextureMagFilter.LINEAR => OpenTK.Graphics.OpenGL
                                               .TextureMagFilter.Linear,
              _ => throw new ArgumentOutOfRangeException()
          }));
      GL.TexParameter(target,
                      TextureParameterName.TextureMinLod,
                      texture.MinLod);
      GL.TexParameter(target,
                      TextureParameterName.TextureMaxLod,
                      texture.MaxLod);
      if (!texture.LodBias.IsRoughly0()) {
        GL.TexParameter(target,
                        TextureParameterName.TextureLodBias,
                        texture.LodBias);
      }
    }
  }

  private static readonly MemoryPool<byte> pool_ = MemoryPool<byte>.Shared;

  private void LoadMipmapImagesIntoTexture_(IReadOnlyImage[] mipmapImages) {
    for (var i = 0; i < mipmapImages.Length; ++i) {
      this.LoadImageIntoTexture_(mipmapImages[i], i);
    }
  }

  private unsafe void LoadImageIntoTexture_(IReadOnlyImage image, int level) {
    var imageWidth = image.Width;
    var imageHeight = image.Height;

    switch (image) {
      case Rgba32Image rgba32Image: {
        using var fastLock = rgba32Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            PixelInternalFormat.Rgba8,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Rgba,
                            fastLock.byteScan0);
        break;
      }
      case Rgb24Image rgb24Image: {
        using var fastLock = rgb24Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            PixelInternalFormat.Rgb8,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Rgb,
                            fastLock.byteScan0);
        break;
      }
      // TODO: Luminance/LuminanceAlpha is not supported in OpenGL ES. Implement support for R/RG instead
      /*
      case La16Image la16Image: {
        using var fastLock = la16Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            TextureComponentCount.LuminanceAlpha,
                            imageWidth,
                            imageHeight,
                            PixelFormat.LuminanceAlpha,
                            fastLock.byteScan0);
        break;
      }
      case L8Image l8Image: {
        using var fastLock = l8Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            TextureComponentCount.Luminance,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Luminance,
                            fastLock.byteScan0);
        break;
      }
      case I8Image i8Image: {
        using var fastLock = i8Image.UnsafeLock();
        PassBytesIntoImage_(level,
                            TextureComponentCount.LuminanceAlpha,
                            imageWidth,
                            imageHeight,
                            PixelFormat.LuminanceAlpha,
                            fastLock.byteScan0);
        break;
      }*/
      default: {
        using var rentedBytes = pool_.Rent(4 * imageWidth * imageHeight);
        image.Access(getHandler => {
          var pixelBytes = rentedBytes.Memory.Span;
          for (var y = 0; y < imageHeight; y++) {
            for (var x = 0; x < imageWidth; x++) {
              getHandler(x,
                         y,
                         out var r,
                         out var g,
                         out var b,
                         out var a);

              var outI = 4 * (y * imageWidth + x);  
              pixelBytes[outI] = r;
              pixelBytes[outI + 1] = g;
              pixelBytes[outI + 2] = b;
              pixelBytes[outI + 3] = a;
            }
          }
        });
        PassBytesIntoImage_(level,
                            PixelInternalFormat.Rgba8,
                            imageWidth,
                            imageHeight,
                            PixelFormat.Rgba,
                            (byte*) rentedBytes.Memory.Pin().Pointer);
        break;
      }
    }
  }

  private static unsafe void PassBytesIntoImage_(
      int level,
      PixelInternalFormat internalFormat,
      int imageWidth,
      int imageHeight,
      PixelFormat pixelFormat,
      byte* scan0) {
    // This is required to fix a rare issue with alignment:
    // https://stackoverflow.com/questions/52460143/texture-not-showing-correctly
    GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
    GL.TexImage2D(TextureTarget.Texture2D,
                  level,
                  internalFormat,
                  imageWidth,
                  imageHeight,
                  0,
                  pixelFormat,
                  PixelType.UnsignedByte,
                  (IntPtr) scan0);
  }

  ~GlTexture() => this.ReleaseUnmanagedResources_();

  public bool IsDisposed { get; private set; }

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    if (GlMaterialConstants.IsCommonTexture(this)) {
      return;
    }

    this.IsDisposed = true;
    if (this.texture_ != null) {
      cache_.Remove(this.texture_);
    }

    var id = this.Id;
    GL.DeleteTextures(1, ref id);

    this.Id = UNDEFINED_ID;
  }

  public int Id { get; private set; } = UNDEFINED_ID;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void Bind(int textureIndex = 0)
    => GlUtil.BindTexture(textureIndex, this.Id);

  private static int ConvertFinWrapToGlWrap_(
      WrapMode wrapMode,
      bool hasBorderColor) =>
      wrapMode switch {
          WrapMode.CLAMP => hasBorderColor
              ? (int) TextureWrapMode.ClampToBorder
              : (int) TextureWrapMode.ClampToEdge,
          WrapMode.REPEAT        => (int) TextureWrapMode.Repeat,
          WrapMode.MIRROR_CLAMP  => (int) All.MirrorClampToEdge,
          WrapMode.MIRROR_REPEAT => (int) All.MirroredRepeat,
          _ => throw new ArgumentOutOfRangeException(
              nameof(wrapMode),
              wrapMode,
              null)
      };
}