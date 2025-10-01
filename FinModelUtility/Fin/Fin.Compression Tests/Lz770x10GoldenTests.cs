using System.Reflection;

using fin.compression;
using fin.io;
using fin.testing;

namespace Fin.Compression_Tests;

public sealed class Lz77GoldenTests {
  [Test]
  [TestCaseSource(nameof(Get0x10GoldenDirectories_))]
  public async Task Test0x10(
      IFileHierarchyDirectory goldenDirectory)
    => await this.AssertGolden(goldenDirectory);

  private static IFileHierarchyDirectory[] Get0x10GoldenDirectories_()
    => GoldenAssert.GetGoldenDirectories(
                       GoldenAssert.GetRootGoldensDirectory(
                                       Assembly.GetExecutingAssembly())
                                   .AssertGetExistingSubdir("Lz77/0x10"))
                   .ToArray();

  public async Task AssertGolden(IFileHierarchyDirectory goldenSubdir)
    => await GoldenAssert.AssertGoldenFiles(
        goldenSubdir,
        (inputDirectory, targetDirectory) => {
          var lz10File = inputDirectory.FilesWithExtension(".lz77").Single();

          using var br = lz10File.OpenReadAsBinary();
          var decompressedBytes = new Lz77Decompressor().Decompress(br);

          var targetFile = new FinFile(
              Path.Combine(targetDirectory.FullPath,
                           $"{lz10File.NameWithoutExtension}.bin"));
          targetFile.WriteAllBytes(decompressedBytes);
        });
}