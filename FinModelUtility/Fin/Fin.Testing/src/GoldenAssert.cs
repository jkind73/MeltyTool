using System.Reflection;
using System.Text;

using fin.image;
using fin.io;
using fin.util.asserts;
using fin.util.exceptions;
using fin.util.strings;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using SixLabors.ImageSharp.PixelFormats;


namespace fin.testing;

public static partial class GoldenAssert {
  private const string TMP_NAME = "tmp";

  public static ISystemDirectory GetRootGoldensDirectory(
      Assembly executingAssembly) {
    var assemblyName =
        executingAssembly.ManifestModule.Name.SubstringUpTo(".dll");

    var executingAssemblyDll = new FinFile(executingAssembly.Location);
    var executingAssemblyDir = executingAssemblyDll.AssertGetParent();

    var currentDir = executingAssemblyDir;
    while (!currentDir.Name.Equals(assemblyName,
                                   StringComparison.OrdinalIgnoreCase)) {
      currentDir = currentDir.AssertGetParent();
    }

    Assert.IsNotNull(currentDir);

    var gloTestsDir = currentDir;
    var goldensDirectory = gloTestsDir.AssertGetExistingSubdir("goldens");

    return goldensDirectory;
  }

  public static IEnumerable<IFileHierarchyDirectory> GetGoldenDirectories(
      ISystemDirectory rootGoldenDirectory) {
    var hierarchy = FileHierarchy.From(rootGoldenDirectory);
    return hierarchy.Root.GetExistingSubdirs()
                    .Where(subdir => !subdir.Name.SequenceEqual(TMP_NAME));
  }

  public static async Task AssertGoldenFiles(
      IFileHierarchyDirectory goldenSubdir,
      Action<IFileHierarchyDirectory, ISystemDirectory> handler) {
    var inputDirectory = goldenSubdir.AssertGetExistingSubdir("input");
    var outputDirectory = goldenSubdir.AssertGetExistingSubdir("output");

    if (outputDirectory.IsEmpty) {
      handler(inputDirectory, outputDirectory.Impl);
      return;
    }

    var tmpDirectory = FinFileSystem.CreateVirtualTempDirectory();
    handler(inputDirectory, tmpDirectory);

    await GoldenAssert.AssertFilesInDirectoriesAreIdentical_(
        tmpDirectory,
        outputDirectory.Impl);

    tmpDirectory.Delete(true);
  }

  private static async Task AssertFilesInDirectoriesAreIdentical_(
      IReadOnlyTreeDirectory lhs,
      IReadOnlyTreeDirectory rhs) {
    var lhsFiles = lhs.GetExistingFiles()
                      .ToDictionary(file => file.Name.ToString());
    var rhsFiles = rhs.GetExistingFiles()
                      .ToDictionary(file => file.Name.ToString());

    var lhsFullPaths = lhsFiles.Keys.ToHashSet();
    var rhsFullPaths = rhsFiles.Keys.ToHashSet();

    if (!lhsFullPaths.SetEquals(rhsFullPaths)) {
      var lhsOnly = lhsFullPaths.Where(v => !rhsFullPaths.Contains(v))
                                .Order()
                                .ToArray();
      var rhsOnly = rhsFullPaths.Where(v => !lhsFullPaths.Contains(v))
                                .Order()
                                .ToArray();

      var sb = new StringBuilder();
      sb.AppendLine(
          "Expected fileset to be identical to golden's, but there were the following differences:");

      if (lhsOnly.Length > 0) {
        sb.AppendLine("Lhs only:");
        foreach (var lhsPath in lhsOnly) {
          sb.AppendLine($" - {lhsPath}");
        }
      }

      if (rhsOnly.Length > 0) {
        sb.AppendLine("Rhs only:");
        foreach (var rhsPath in rhsOnly) {
          sb.AppendLine($" - {rhsPath}");
        }
      }

      Assert.Fail(sb.ToString());
    }

    foreach (var (name, lhsFile) in lhsFiles) {
      var rhsFile = rhsFiles[name];

      await AnnotatedException.SpaceAsync(
          $"Found a change in file {name}:\n",
          async () => {
            await AssertFilesAreIdentical_(lhsFile, rhsFile);
          });
    }
  }

  private static async Task AssertFilesAreIdentical_(
      IReadOnlyTreeFile lhs,
      IReadOnlyTreeFile rhs) {
    var lhsAndRhsBytes
        = await Task.WhenAll(lhs.ReadAllBytesAsync(),
                             rhs.ReadAllBytesAsync());

    var lhsBytes = lhsAndRhsBytes[0];
    var rhsBytes = lhsAndRhsBytes[1];

    try {
      Assert.IsTrue(lhsBytes.SequenceEqual(rhsBytes));
    } catch (Exception e) {
      if (lhs.FileType.ToLower() is ".bmp"
                                    or ".jpg"
                                    or ".jpeg"
                                    or ".gif"
                                    or ".png") {
        AssertImageFilesAreIdentical_(lhs, rhs);
      } else if (lhs.FileType.ToLower() is ".glb" or ".gltf") {
        AssertModelFilesAreIdentical_(lhs, rhs);
        Asserts.SpansEqual(lhsBytes, rhsBytes);
      } else {
        Asserts.SpansEqual(lhsBytes, rhsBytes);
      }
    }
  }

  private static void AssertImageFilesAreIdentical_(
      IReadOnlyTreeFile lhs,
      IReadOnlyTreeFile rhs,
      float allowableError = 2) {
    using var lhsImage = FinImage.FromFile(lhs);
    using var rhsImage = FinImage.FromFile(rhs);

    Assert.AreEqual(lhsImage.Width, rhsImage.Width);
    Assert.AreEqual(lhsImage.Height, rhsImage.Height);

    lhsImage.Access(lhsGet => {
      rhsImage.Access(rhsGet => {
        for (var y = 0; y < lhsImage.Height; ++y) {
          for (var x = 0; x < lhsImage.Width; ++x) {
            lhsGet(x,
                   y,
                   out var lhsR,
                   out var lhsG,
                   out var lhsB,
                   out var lhsA);

            rhsGet(x,
                   y,
                   out var rhsR,
                   out var rhsG,
                   out var rhsB,
                   out var rhsA);

            var lPixel = new Rgba32(lhsR, lhsG, lhsB, lhsA);
            var rPixel = new Rgba32(rhsR, rhsG, rhsB, rhsA);

            if (Math.Abs(lhsR - rhsR) > allowableError ||
                Math.Abs(lhsG - rhsG) > allowableError ||
                Math.Abs(lhsB - rhsB) > allowableError ||
                Math.Abs(lhsA - rhsA) > allowableError) {
              Asserts.Fail(
                  $"Files with name \"{lhs.Name}\" are different at pixel ({x},{y}): {lPixel} / {rPixel}");
            }
          }
        }
      });
    });
  }
}