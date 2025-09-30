// Decompiled with JetBrains decompiler
// Type: QuickFont.TexturePage
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

#nullable disable
namespace QuickFont
{
  internal class TexturePage : IDisposable
  {
    private int _textureID;

    public int TextureID => this._textureID;

    public int Width { get; private set; }

    public int Height { get; private set; }

    public TexturePage(string filePath)
    {
      QBitmap qbitmap = new QBitmap(filePath);
      this.CreateTexture(qbitmap.BitmapData);
      qbitmap.Free();
    }

    public TexturePage(BitmapData dataSource) => this.CreateTexture(dataSource);

    private void CreateTexture(BitmapData dataSource)
    {
      this.Width = dataSource.Width;
      this.Height = dataSource.Height;

      GlUtil.AssertNoErrorsWhenDebugging();
      GL.GenTextures(1, out this._textureID);
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.BindTexture(TextureTarget.Texture2D, this.TextureID);
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.ClampToEdge);
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.ClampToEdge);
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Linear);
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Linear);
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, this.Width, this.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, dataSource.Scan0);
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
      GlUtil.AssertNoErrorsWhenDebugging();
    }

    private static byte[] ConvertBgraToRgba(BitmapData dataSource)
    {
      int length = dataSource.Stride * dataSource.Height;
      byte[] destination = new byte[length];
      Marshal.Copy(dataSource.Scan0, destination, 0, length);
      for (int index = 0; index < destination.Length; index += 4)
      {
        byte num = destination[index];
        destination[index] = destination[index + 2];
        destination[index + 2] = num;
      }
      return destination;
    }

    public void Dispose() {
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.DeleteTexture(this.TextureID);
      GlUtil.AssertNoErrorsWhenDebugging();
    }
  }
}
