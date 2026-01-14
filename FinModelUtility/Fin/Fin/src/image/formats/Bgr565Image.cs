using fin.color;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace fin.image.formats;

public sealed class Bgr565Image : BImage<Bgr565> {
  public Bgr565Image(int width, int height) : this(
      PixelFormat.BGR565,
      width,
      height) { }

  public Bgr565Image(PixelFormat format, int width, int height) : this(
      format,
      new Image<Bgr565>(FinImage.ImageSharpConfig, width, height)) { }

  internal Bgr565Image(PixelFormat format, Image<Bgr565> impl) : base(
      format) {
    this.Impl = impl;
  }

  protected override Image<Bgr565> Impl { get; }

  public override void Access(IImage.AccessHandler accessHandler)
    => FinImage.Access(
        this.Impl,
        getHandler => {
          void InternalGetHandler(
              int x,
              int y,
              out byte r,
              out byte g,
              out byte b,
              out byte a) {
            getHandler(x, y, out var pixel);
            ColorUtil.SplitBgr565(pixel.PackedValue, out r, out g, out b);
            a = 255;
          }

          accessHandler(InternalGetHandler);
        });

  public override bool HasAlphaChannel => false;
}