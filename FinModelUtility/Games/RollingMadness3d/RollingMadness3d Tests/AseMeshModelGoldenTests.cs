using System.Reflection;

using fin.io;
using fin.testing.model;
using fin.testing;

using rollingMadness.api;

namespace rollingMadness;

public sealed class AseMeshModelGoldenTests
    : BModelGoldenTests<AseMeshModelFileBundle, AseMeshModelImporter> {
  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public async Task TestExportsGoldenAsExpected(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  [Test]
  [TestCaseSource(nameof(GetGoldenDirectories_))]
  public void TestPreservesCompoundLightmapNames(
      IFileHierarchyDirectory goldenDirectory) {
    var inputDirectory = goldenDirectory.AssertGetExistingSubdir("input");
    var model = new AseMeshModelImporter().Import(
        this.GetFileBundleFromDirectory(inputDirectory));

    var actualLightmapNames = model.MaterialManager.Textures
                                   .Select(texture => texture.Name)
                                   .Where(name => name.StartsWith(
                                              "lev1.ase.",
                                              StringComparison.Ordinal))
                                   .OrderBy(name => name)
                                   .ToArray();
    var expectedLightmapNames = Enumerable.Range(0, 6)
                                          .Select(index => $"lev1.ase.{index}")
                                          .ToArray();

    NUnit.Framework.Assert.That(actualLightmapNames,
                                Is.EqualTo(expectedLightmapNames));
  }

  public override AseMeshModelFileBundle GetFileBundleFromDirectory(
      IFileHierarchyDirectory directory)
    => new(directory.FilesWithExtension(".ase.mesh").Single(), directory);

  private static IFileHierarchyDirectory[] GetGoldenDirectories_()
    => GoldenAssert
       .GetGoldenDirectories(
           GoldenAssert
               .GetRootGoldensDirectory(Assembly.GetExecutingAssembly()))
       .ToArray();
}
