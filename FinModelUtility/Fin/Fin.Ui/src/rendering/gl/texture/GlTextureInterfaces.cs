using fin.data.disposables;

namespace fin.ui.rendering.gl.texture;

public interface IGlTexture : IFinDisposable {
  int Id { get; }
  long Handle { get; }

  void Bind(int textureIndex = 0);
}