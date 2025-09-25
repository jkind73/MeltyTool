using fin.color;
using fin.model;

namespace f3dzex2.model;

public sealed class RdpState {
  public ITexture Texture0 { get; set; }
  public ITexture Texture1 { get; set; }
  public IColor EnvironmentColor { get; set; }
  public IColor PrimitiveColor { get; set; }
}

public sealed class N64FixedFunctionGenerator {
  public void GenerateFixedFunctionMaterial(IModel model,
                                            ITexture texture0,
                                            ITexture texture1) {

    }
}