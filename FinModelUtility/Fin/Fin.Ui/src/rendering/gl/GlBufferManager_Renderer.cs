using fin.math;
using fin.model;
using fin.ui.rendering.gl.model;
using fin.util.enumerables;
using fin.util.linq;

using Nito.Disposables;

using OpenTK.Graphics.OpenGL4;

using FinPrimitiveType = fin.model.PrimitiveType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace fin.ui.rendering.gl;

public partial interface IGlBufferManager {
  IGlBufferRenderer[] CreateRenderers(
      IReadOnlyList<MergedPrimitive> mergedPrimitives);
}

public interface IGlBufferRenderer : IRenderable;

public sealed partial class GlBufferManager {
  private const DrawElementsType INDEX_TYPE = DrawElementsType.UnsignedInt;

  public IGlBufferRenderer[] CreateRenderers(
      IReadOnlyList<MergedPrimitive> mergedPrimitives) {
    IReadOnlyList<int> restartIndex = [
        (int) (INDEX_TYPE switch {
            DrawElementsType.UnsignedByte => byte.MaxValue,
            DrawElementsType.UnsignedShort => ushort.MaxValue,
            DrawElementsType.UnsignedInt => uint.MaxValue,
            _ => throw new ArgumentOutOfRangeException()
        })
    ];

    IGlBufferRenderer[] renderers
        = new IGlBufferRenderer[mergedPrimitives.Count];
    var eboDisposable = new Disposable(null);

    GlUtil.BindVao(this.VaoId);
    GL.GenBuffers(1, out int eboId);

    var eboIndices = new List<int>();
    var eboInstanceCount = 0;
    for (var i = 0; i < mergedPrimitives.Count; ++i) {
      var mergedPrimitive = mergedPrimitives[i];
      var mpIndices
          = mergedPrimitive
            .Vertices
            .Select(vertices => vertices.Select(vertex => vertex.Index))
            .Intersperse(restartIndex)
            .SelectMany(indices => indices)
            .ToArray();

      if (mpIndices.IsSequentiallyIncreasing(
              out var vertexOffset,
              out var vertexCount)) {
        renderers[i] = new GlBufferRenderer(
            this.VaoId,
            mergedPrimitive.PrimitiveType,
            mergedPrimitive.IsFlipped,
            null,
            null,
            vertexOffset,
            vertexCount
        );
      } else {
        vertexOffset = eboIndices.Count;
        vertexCount = mpIndices.Length;

        eboIndices.AddRange(mpIndices);

        renderers[i] = new GlBufferRenderer(
            this.VaoId,
            mergedPrimitive.PrimitiveType,
            mergedPrimitive.IsFlipped,
            eboId,
            eboDisposable,
            vertexOffset,
            vertexCount
        );
      }
    }

    if (eboIndices.Count == 0) {
      GL.DeleteBuffers(1, ref eboId);
    } else {
      GL.BindBuffer(BufferTarget.ElementArrayBuffer, eboId);
      GL.BufferData(BufferTarget.ElementArrayBuffer,
                    new IntPtr(sizeof(int) * eboIndices.Count),
                    eboIndices.ToArray(),
                    BufferUsageHint.StaticDraw);

      var count = eboInstanceCount;
      eboDisposable.Add(() => {
        if (--count == 0) {
          GL.DeleteBuffers(1, ref eboId);
        }
      });
    }

    return renderers;
  }

  public sealed class GlBufferRenderer(
      int vaoId,
      FinPrimitiveType primitiveType,
      bool isFlipped,
      int? eboId,
      IDisposable? eboDisposable,
      int vertexOffset,
      int vertexCount) : IGlBufferRenderer {
    private readonly PrimitiveType beginMode_
        = primitiveType switch {
            FinPrimitiveType.POINTS => PrimitiveType.Points,
            FinPrimitiveType.LINES => PrimitiveType.Lines,
            FinPrimitiveType.LINE_STRIP => PrimitiveType.LineStrip,
            FinPrimitiveType.TRIANGLES => PrimitiveType.Triangles,
            FinPrimitiveType.TRIANGLE_FAN => PrimitiveType.TriangleFan,
            FinPrimitiveType.TRIANGLE_STRIP => PrimitiveType.TriangleStrip,
            _ => throw new ArgumentOutOfRangeException()
        };

    ~GlBufferRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_() => eboDisposable?.Dispose();

    public void Render() {
      GlUtil.SetFlipFaces(isFlipped);
      GlUtil.BindVao(vaoId);

      if (eboId != null) {
        GlUtil.BindEbo(eboId.Value);

        GlUtil.ValidateCurrentProgram();
        GL.DrawElements(
            this.beginMode_,
            vertexCount,
            INDEX_TYPE,
            new IntPtr(sizeof(int) * vertexOffset));
      } else {
        GL.DrawArrays(
            this.beginMode_,
            vertexOffset,
            vertexCount);
      }
    }
  }
}