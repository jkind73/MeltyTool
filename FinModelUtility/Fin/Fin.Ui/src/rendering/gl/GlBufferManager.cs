using fin.data;
using fin.model;
using fin.shaders.glsl;
using fin.ui.rendering.gl.model;
using fin.util.enumerables;
using fin.util.linq;

using OpenTK.Graphics.OpenGL4;

using FinPrimitiveType = fin.model.PrimitiveType;
using PrimitiveType = OpenTK.Graphics.OpenGL4.PrimitiveType;

namespace fin.ui.rendering.gl;

public interface IGlBufferManager : IDisposable {
  int VaoId { get; }

  IGlBufferRenderer CreateRenderer(
      FinPrimitiveType primitiveType,
      IReadOnlyList<IReadOnlyVertex> triangleVertices,
      bool isFlipped = false);

  IGlBufferRenderer CreateRenderer(in MergedPrimitive mergedPrimitive);
}

public interface IDynamicGlBufferManager : IGlBufferManager {
  void UpdateBuffer();
}

public interface IGlBufferRenderer : IRenderable;

public sealed partial class GlBufferManager : IDynamicGlBufferManager {
  private readonly IReadOnlyModel model_;
  private readonly IModelRequirements modelRequirements_;
  private readonly BufferUsageHint bufferType_;
  private readonly VertexArrayObject vao_;

  public int VaoId => this.vao_.VaoId;

  public static IGlBufferManager CreateStatic(
      IReadOnlyModel model,
      IModelRequirements modelRequirements)
    => new GlBufferManager(model,
                           modelRequirements,
                           BufferUsageHint.StaticDraw);

  public static IDynamicGlBufferManager CreateDynamic(
      IReadOnlyModel model,
      IModelRequirements modelRequirements)
    => new GlBufferManager(model,
                           modelRequirements,
                           BufferUsageHint.DynamicDraw);

  private GlBufferManager(IReadOnlyModel model,
                          IModelRequirements modelRequirements,
                          BufferUsageHint bufferType) {
    this.model_ = model;
    this.modelRequirements_ = modelRequirements;
    this.bufferType_ = bufferType;
    this.vao_ = vaoCache_.GetAndIncrement(
        (this.model_, this.modelRequirements_, bufferType));
  }

  ~GlBufferManager() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_() {
    vaoCache_.DecrementAndMaybeDispose((this.model_,
                                        this.modelRequirements_,
                                        this.bufferType_));
  }

  private static ReferenceCountCacheDictionary<
          (IReadOnlyModel, IModelRequirements, BufferUsageHint),
          VertexArrayObject>
      vaoCache_ = new(modelAndBufferType => new VertexArrayObject(
                          modelAndBufferType.Item1,
                          modelAndBufferType.Item2,
                          modelAndBufferType.Item3),
                      (_, vao) => vao.Dispose());

  public IGlBufferRenderer CreateRenderer(
      FinPrimitiveType primitiveType,
      IReadOnlyList<IReadOnlyVertex> triangleVertices,
      bool isFlipped = false)
    => new GlBufferRenderer(this.vao_.VaoId,
                            primitiveType,
                            isFlipped,
                            triangleVertices);

  public IGlBufferRenderer CreateRenderer(in MergedPrimitive mergedPrimitive)
    => new GlBufferRenderer(this.vao_.VaoId, mergedPrimitive);

  public void UpdateBuffer() => this.vao_.UpdateBuffer();

  public sealed class GlBufferRenderer : IGlBufferRenderer {
    private readonly int vaoId_;
    private PrimitiveType beginMode_;
    private readonly bool isFlipped_;

    // Present if in indices mode
    private int eboId_;
    private readonly int[]? indices_;

    // Present if in vertex mode
    private readonly int vertexCount_;

    private const DrawElementsType INDEX_TYPE = DrawElementsType.UnsignedInt;

    public GlBufferRenderer(
        int vaoId,
        FinPrimitiveType primitiveType,
        bool isFlipped,
        IEnumerable<IReadOnlyVertex> vertices) : this(
        vaoId,
        new MergedPrimitive {
            PrimitiveType = primitiveType,
            Vertices = vertices.Yield(),
            IsFlipped = isFlipped
        }) { }

    public GlBufferRenderer(
        int vaoId,
        in MergedPrimitive mergedPrimitive) {
      this.vaoId_ = vaoId;
      this.beginMode_ = mergedPrimitive.PrimitiveType switch {
          FinPrimitiveType.POINTS         => PrimitiveType.Points,
          FinPrimitiveType.LINES          => PrimitiveType.Lines,
          FinPrimitiveType.LINE_STRIP     => PrimitiveType.LineStrip,
          FinPrimitiveType.TRIANGLES      => PrimitiveType.Triangles,
          FinPrimitiveType.TRIANGLE_FAN   => PrimitiveType.TriangleFan,
          FinPrimitiveType.TRIANGLE_STRIP => PrimitiveType.TriangleStrip,
          _                            => throw new ArgumentOutOfRangeException()
      };
      this.isFlipped_ = mergedPrimitive.IsFlipped;

      GlUtil.BindVao(this.vaoId_);
      GL.GenBuffers(1, out this.eboId_);

      IReadOnlyList<int> restartIndex = [
          (int) (INDEX_TYPE switch {
              DrawElementsType.UnsignedByte  => byte.MaxValue,
              DrawElementsType.UnsignedShort => ushort.MaxValue,
              DrawElementsType.UnsignedInt   => uint.MaxValue,
              _                              => throw new ArgumentOutOfRangeException()
          })
      ];

      var vertices = mergedPrimitive.Vertices.SelectMany(e => e).ToArray();

      if (!vertices.All((v, i) => v.Index == i)) {
        this.indices_ =
            mergedPrimitive
                .Vertices
                .Select(vertices
                            => vertices.Select(vertex => vertex.Index))
                .Intersperse(restartIndex)
                .SelectMany(indices => indices)
                .ToArray();

        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.eboId_);
        GL.BufferData(BufferTarget.ElementArrayBuffer,
                      new IntPtr(sizeof(int) * this.indices_.Length),
                      this.indices_,
                      BufferUsageHint.StaticDraw);
      } else {
        this.vertexCount_ = vertices.Length;
      }
    }

    ~GlBufferRenderer() => this.ReleaseUnmanagedResources_();

    public void Dispose() {
      this.ReleaseUnmanagedResources_();
      GC.SuppressFinalize(this);
    }

    private void ReleaseUnmanagedResources_()
      => GL.DeleteBuffers(1, ref this.eboId_);

    public void Render() {
      GlUtil.SetFlipFaces(this.isFlipped_);
      GlUtil.BindVao(this.vaoId_);

      if (this.indices_ != null) {
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.eboId_);

        GlUtil.ValidateCurrentProgram();
        GL.DrawElements(
            this.beginMode_,
            this.indices_.Length,
            INDEX_TYPE,
            IntPtr.Zero);
      } else {
        GL.DrawArrays(
            this.beginMode_,
            0,
            this.vertexCount_);
      }
    }
  }
}