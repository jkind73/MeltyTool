using System.Numerics;

using bar.schema;

using fin.data.lazy;
using fin.io;
using fin.model;
using fin.scene;
using fin.util.sets;

using schema.binary;

namespace bar.api;

public static class BarUtils {
  public static readonly Matrix4x4 ROOT_MATRIX = Matrix4x4.CreateFromQuaternion(
      Quaternion.CreateFromYawPitchRoll(0, -MathF.PI / 2, 0));

  public static ILazyDictionary<short, IReadOnlyModel>
      CreateLazyUvmdModelDictionary(
          HashSet<IReadOnlyGenericFile> files,
          IReadOnlyTreeDirectory rootDirectory)
    => new LazyDictionary<short, IReadOnlyModel>(uvmdIndex => {
      var finModel = UvmdModelFileImporter.Import(
          new UvmdModelFileBundle(
              rootDirectory.AssertGetExistingFile($"uvmd/{uvmdIndex}.uvmd"),
              rootDirectory),
          false);
      files.Add(finModel.Files);

      return finModel;
    });
}