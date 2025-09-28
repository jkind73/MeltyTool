// Decompiled with JetBrains decompiler
// Type: QuickFont.QVertexArrayObject
// Assembly: Wayfinder.QuickFont, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0B734F03-6CB9-4E1A-B817-4DAA44B7F881
// Assembly location: C:\Users\Ryan\AppData\Local\Temp\Ramumib\5e47dbd843\lib\net5.0\Wayfinder.QuickFont.dll

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

#nullable disable
namespace QuickFont
{
  public class QVertexArrayObject : IDisposable
  {
    private const int INITIAL_SIZE = 1000;
    private int _bufferSize;
    private int _bufferMaxVertexCount;
    public int VertexCount;
    private int _VAOID;
    private int _VBOID;
    public readonly QFontSharedState QFontSharedState;
    private List<QVertex> _vertices;
    private QVertex[] _vertexArray;
    private static readonly int QVertexStride = Marshal.SizeOf<QVertex>(new QVertex());
    private bool _disposedValue;

    public QVertexArrayObject(QFontSharedState state)
    {
      this.QFontSharedState = state;
      this._vertices = new List<QVertex>(1000);
      this._bufferMaxVertexCount = 1000;
      this._bufferSize = this._bufferMaxVertexCount * QVertexArrayObject.QVertexStride;
      GlUtil.AssertNoErrorsWhenDebugging();
      this._VAOID = GL.GenVertexArray();
      GL.UseProgram(this.QFontSharedState.ShaderVariables.ShaderProgram);
      GL.BindVertexArray(this._VAOID);
      GL.GenBuffers(1, out this._VBOID);
      GL.BindBuffer(BufferTarget.ArrayBuffer, this._VBOID);
      this.EnableAttributes();
      GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) this._bufferSize, IntPtr.Zero, BufferUsageHint.StreamDraw);
      GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
      GL.BindVertexArray(0);
      GlUtil.AssertNoErrorsWhenDebugging();
    }

    internal void AddVertexes(IList<QVertex> vertices)
    {
      this.VertexCount += vertices.Count;
      this._vertices.AddRange((IEnumerable<QVertex>) vertices);
    }

    public void AddVertex(Vector3 position, Vector2 textureCoord, Vector4 colour)
    {
      ++this.VertexCount;
      this._vertices.Add(new QVertex()
      {
        Position = position,
        TextureCoord = textureCoord,
        VertexColor = colour
      });
    }

    public void Load()
    {
      if (this.VertexCount == 0)
        return;
      this._vertexArray = this._vertices.ToArray();
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.BindBuffer(BufferTarget.ArrayBuffer, this._VBOID);
      GL.BindVertexArray(this._VAOID);
      if (this.VertexCount > this._bufferMaxVertexCount)
      {
        while (this.VertexCount > this._bufferMaxVertexCount)
        {
          this._bufferMaxVertexCount += 1000;
          this._bufferSize = this._bufferMaxVertexCount * QVertexArrayObject.QVertexStride;
        }
        GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) this._bufferSize, IntPtr.Zero, BufferUsageHint.StreamDraw);
      }
      GL.BufferSubData<QVertex>(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr) (this.VertexCount * QVertexArrayObject.QVertexStride), this._vertexArray);
      GlUtil.AssertNoErrorsWhenDebugging();
    }

    public void Reset()
    {
      this._vertices.Clear();
      this.VertexCount = 0;
    }

    public void Bind() {
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.BindVertexArray(this._VAOID);
      GlUtil.AssertNoErrorsWhenDebugging();
    }

    public void DisableAttributes()
    {
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.DisableVertexAttribArray(this.QFontSharedState.ShaderVariables.PositionCoordAttribLocation);
      GL.DisableVertexAttribArray(this.QFontSharedState.ShaderVariables.TextureCoordAttribLocation);
      GL.DisableVertexAttribArray(this.QFontSharedState.ShaderVariables.ColorCoordAttribLocation);
      GlUtil.AssertNoErrorsWhenDebugging();
    }

    private void EnableAttributes()
    {
      GlUtil.AssertNoErrorsWhenDebugging();
      int qvertexStride = QVertexArrayObject.QVertexStride;
      GL.EnableVertexAttribArray(this.QFontSharedState.ShaderVariables.PositionCoordAttribLocation);
      GL.EnableVertexAttribArray(this.QFontSharedState.ShaderVariables.TextureCoordAttribLocation);
      GL.EnableVertexAttribArray(this.QFontSharedState.ShaderVariables.ColorCoordAttribLocation);
      GL.VertexAttribPointer(this.QFontSharedState.ShaderVariables.PositionCoordAttribLocation, 3, VertexAttribPointerType.Float, false, qvertexStride, IntPtr.Zero);
      GL.VertexAttribPointer(this.QFontSharedState.ShaderVariables.TextureCoordAttribLocation, 2, VertexAttribPointerType.Float, false, qvertexStride, new IntPtr(12));
      GL.VertexAttribPointer(this.QFontSharedState.ShaderVariables.ColorCoordAttribLocation, 4, VertexAttribPointerType.Float, false, qvertexStride, new IntPtr(20));
      GlUtil.AssertNoErrorsWhenDebugging();
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this._disposedValue)
        return;
      if (disposing)
      {
        GlUtil.AssertNoErrorsWhenDebugging();
        GL.DeleteBuffers(1, ref this._VBOID);
        GL.DeleteVertexArrays(1, ref this._VAOID);
        GlUtil.AssertNoErrorsWhenDebugging();
        if (this._vertices != null)
        {
          this._vertices.Clear();
          this._vertices = (List<QVertex>) null;
        }
      }
      this._vertexArray = (QVertex[]) null;
      this._disposedValue = true;
    }

    public void Dispose() => this.Dispose(true);
  }
}
