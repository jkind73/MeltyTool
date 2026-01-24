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
namespace QuickFont;

public class QVertexArrayObject : IDisposable
{
  private const int INITIAL_SIZE_ = 1000;
  private int bufferSize_;
  private int bufferMaxVertexCount_;
  public int vertexCount;
  private int vaoid_;
  private int vboid_;
  public readonly QFontSharedState qFontSharedState;
  private List<QVertex> vertices_;
  private QVertex[] vertexArray_;
  private static readonly int Q_VERTEX_STRIDE_ = Marshal.SizeOf<QVertex>(new QVertex());
  private bool disposedValue_;

  public QVertexArrayObject(QFontSharedState state)
  {
    this.qFontSharedState = state;
    this.vertices_ = new List<QVertex>(1000);
    this.bufferMaxVertexCount_ = 1000;
    this.bufferSize_ = this.bufferMaxVertexCount_ * QVertexArrayObject.Q_VERTEX_STRIDE_;
    GlUtil.AssertNoErrorsWhenDebugging();
    this.vaoid_ = GL.GenVertexArray();
    GL.UseProgram(this.qFontSharedState.ShaderVariables.ShaderProgram);
    GL.BindVertexArray(this.vaoid_);
    GL.GenBuffers(1, out this.vboid_);
    GL.BindBuffer(BufferTarget.ArrayBuffer, this.vboid_);
    this.EnableAttributes_();
    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) this.bufferSize_, IntPtr.Zero, BufferUsageHint.StreamDraw);
    GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
    GL.BindVertexArray(0);
    GlUtil.AssertNoErrorsWhenDebugging();
  }

  internal void AddVertexes(IList<QVertex> vertices)
  {
    this.vertexCount += vertices.Count;
    this.vertices_.AddRange((IEnumerable<QVertex>) vertices);
  }

  public void AddVertex(Vector3 position, Vector2 textureCoord, Vector4 colour)
  {
    ++this.vertexCount;
    this.vertices_.Add(new QVertex()
    {
        Position = position,
        TextureCoord = textureCoord,
        VertexColor = colour
    });
  }

  public void Load()
  {
    if (this.vertexCount == 0)
      return;
    this.vertexArray_ = this.vertices_.ToArray();
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.BindBuffer(BufferTarget.ArrayBuffer, this.vboid_);
    GL.BindVertexArray(this.vaoid_);
    if (this.vertexCount > this.bufferMaxVertexCount_)
    {
      while (this.vertexCount > this.bufferMaxVertexCount_)
      {
        this.bufferMaxVertexCount_ += 1000;
        this.bufferSize_ = this.bufferMaxVertexCount_ * QVertexArrayObject.Q_VERTEX_STRIDE_;
      }
      GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) this.bufferSize_, IntPtr.Zero, BufferUsageHint.StreamDraw);
    }
    GL.BufferSubData<QVertex>(BufferTarget.ArrayBuffer, IntPtr.Zero, (IntPtr) (this.vertexCount * QVertexArrayObject.Q_VERTEX_STRIDE_), this.vertexArray_);
    GlUtil.AssertNoErrorsWhenDebugging();
  }

  public void Reset()
  {
    this.vertices_.Clear();
    this.vertexCount = 0;
  }

  public void Bind() {
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.BindVertexArray(this.vaoid_);
    GlUtil.AssertNoErrorsWhenDebugging();
  }

  public void DisableAttributes()
  {
    GlUtil.AssertNoErrorsWhenDebugging();
    GL.DisableVertexAttribArray(this.qFontSharedState.ShaderVariables.PositionCoordAttribLocation);
    GL.DisableVertexAttribArray(this.qFontSharedState.ShaderVariables.TextureCoordAttribLocation);
    GL.DisableVertexAttribArray(this.qFontSharedState.ShaderVariables.ColorCoordAttribLocation);
    GlUtil.AssertNoErrorsWhenDebugging();
  }

  private void EnableAttributes_()
  {
    GlUtil.AssertNoErrorsWhenDebugging();
    int qvertexStride = QVertexArrayObject.Q_VERTEX_STRIDE_;
    GL.EnableVertexAttribArray(this.qFontSharedState.ShaderVariables.PositionCoordAttribLocation);
    GL.EnableVertexAttribArray(this.qFontSharedState.ShaderVariables.TextureCoordAttribLocation);
    GL.EnableVertexAttribArray(this.qFontSharedState.ShaderVariables.ColorCoordAttribLocation);
    GL.VertexAttribPointer(this.qFontSharedState.ShaderVariables.PositionCoordAttribLocation, 3, VertexAttribPointerType.Float, false, qvertexStride, IntPtr.Zero);
    GL.VertexAttribPointer(this.qFontSharedState.ShaderVariables.TextureCoordAttribLocation, 2, VertexAttribPointerType.Float, false, qvertexStride, new IntPtr(12));
    GL.VertexAttribPointer(this.qFontSharedState.ShaderVariables.ColorCoordAttribLocation, 4, VertexAttribPointerType.Float, false, qvertexStride, new IntPtr(20));
    GlUtil.AssertNoErrorsWhenDebugging();
  }

  protected virtual void Dispose(bool disposing)
  {
    if (this.disposedValue_)
      return;
    if (disposing)
    {
      GlUtil.AssertNoErrorsWhenDebugging();
      GL.DeleteBuffers(1, ref this.vboid_);
      GL.DeleteVertexArrays(1, ref this.vaoid_);
      GlUtil.AssertNoErrorsWhenDebugging();
      if (this.vertices_ != null)
      {
        this.vertices_.Clear();
        this.vertices_ = (List<QVertex>) null;
      }
    }
    this.vertexArray_ = (QVertex[]) null;
    this.disposedValue_ = true;
  }

  public void Dispose() => this.Dispose(true);
}