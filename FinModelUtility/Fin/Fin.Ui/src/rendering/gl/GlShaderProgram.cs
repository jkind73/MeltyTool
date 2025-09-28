using fin.data;
using fin.data.lazy;
using fin.util.asserts;

using OpenTK.Graphics.ES30;


namespace fin.ui.rendering.gl;

public sealed partial class GlShaderProgram : IShaderProgram {
  private const bool ASSERT_COMPILATION = true;

  private bool isDisposed_;
  private readonly CachedShaderProgram cachedShaderProgram_;

  private static ReferenceCountCacheDictionary<string, int>
      vertexShaderCache_ = new(
          src => CreateAndCompileShader_(src, ShaderType.VertexShader),
          (_, id) => {
            if (id != UNDEFINED_ID) {
              GlUtil.AssertNoErrorsWhenDebugging();
              GL.DeleteShader(id);
              GlUtil.AssertNoErrorsWhenDebugging();
            }
          });

  private static ReferenceCountCacheDictionary<string, int>
      fragmentShaderCache_ = new(
          src => CreateAndCompileShader_(src, ShaderType.FragmentShader),
          (_, id) => {
            if (id != UNDEFINED_ID) {
              GlUtil.AssertNoErrorsWhenDebugging();
              GL.DeleteShader(id);
              GlUtil.AssertNoErrorsWhenDebugging();
            }
          });

  private static
      ReferenceCountCacheDictionary<(string vertexSrc, string fragmentSrc),
          CachedShaderProgram> programCache_ =
          new(vertexAndFragmentSrc => {
                var (vertexSrc, fragmentSrc) = vertexAndFragmentSrc;
                var vertexShaderId =
                    vertexShaderCache_.GetAndIncrement(
                        vertexSrc);
                var fragmentShaderId =
                    fragmentShaderCache_.GetAndIncrement(
                        fragmentSrc);

                GlUtil.AssertNoErrorsWhenDebugging();
                var programId = GL.CreateProgram();

                GL.AttachShader(programId, vertexShaderId);
                GL.AttachShader(programId, fragmentShaderId);
                GL.LinkProgram(programId);
                GlUtil.AssertNoErrorsWhenDebugging();

                return new CachedShaderProgram {
                    ProgramId = programId,
                    VertexShaderSource = vertexSrc,
                    FragmentShaderSource = fragmentSrc,
                };
              },
              (vertexAndFragmentSrc, cachedShaderProgram) => {
                GlUtil.AssertNoErrorsWhenDebugging();
                GL.DeleteProgram(cachedShaderProgram.ProgramId);
                GlUtil.AssertNoErrorsWhenDebugging();

                var (vertexSrc, fragmentSrc) = vertexAndFragmentSrc;
                vertexShaderCache_.DecrementAndMaybeDispose(vertexSrc);
                fragmentShaderCache_.DecrementAndMaybeDispose(fragmentSrc);
              });

  private const int UNDEFINED_ID = -1;

  public static GlShaderProgram FromShaders(string vertexShaderSrc,
                                            string fragmentShaderSrc)
    => new(vertexShaderSrc, fragmentShaderSrc);

  private GlShaderProgram(string vertexShaderSrc,
                          string fragmentShaderSrc) {
    this.cachedShaderProgram_ =
        programCache_.GetAndIncrement(
            (vertexShaderSrc, fragmentShaderSrc));
  }

  ~GlShaderProgram() => this.ReleaseUnmanagedResources_();

  public void Dispose() {
    this.isDisposed_ = true;
    this.ReleaseUnmanagedResources_();
    GC.SuppressFinalize(this);
  }

  private void ReleaseUnmanagedResources_()
    => programCache_.DecrementAndMaybeDispose(
        (this.VertexShaderSource, this.FragmentShaderSource));

  private static int CreateAndCompileShader_(string src,
                                             ShaderType shaderType) {
    GlUtil.AssertNoErrorsWhenDebugging();
    var shaderId = GL.CreateShader(shaderType);
    GL.ShaderSource(shaderId, 1, [src], (int[]) null);
    GL.CompileShader(shaderId);

    // TODO: Throw/return this error
    var bufferSize = 10000;
    GL.GetShaderInfoLog(
        shaderId,
        bufferSize,
        out var shaderErrorLength,
        out var shaderError);

    if (ASSERT_COMPILATION) {
      if (shaderError?.Length > 0) {
        Asserts.Fail(shaderError);
      }
    }

    GlUtil.AssertNoErrorsWhenDebugging();

    return shaderId;
  }

  public int ProgramId => this.cachedShaderProgram_.ProgramId;

  public string VertexShaderSource
    => this.cachedShaderProgram_.VertexShaderSource;

  public string FragmentShaderSource
    => this.cachedShaderProgram_.FragmentShaderSource;


  public void Use() {
    if (this.isDisposed_) {
      return;
    }

    GlUtil.UseProgram(this.ProgramId);
    foreach (var uniform in this.cachedUniforms_.Values) {
      uniform.PassValueToProgramIfDirty();
    }
  }

  private class CachedShaderProgram {
    private readonly LazyDictionary<string, int> lazyUniforms_;

    public CachedShaderProgram() {
      this.lazyUniforms_ = new(uniformName
                                   => GL.GetUniformLocation(
                                       this.ProgramId,
                                       uniformName));
    }

    public required int ProgramId { get; init; }
    public required string VertexShaderSource { get; init; }
    public required string FragmentShaderSource { get; init; }

    public int GetUniformLocation(string uniformName)
      => this.lazyUniforms_[uniformName];
  }
}