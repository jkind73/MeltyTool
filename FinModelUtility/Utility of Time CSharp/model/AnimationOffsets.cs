using System.Collections.Generic;

using fin.util.enumerables;

using UoT.memory;
using UoT.util;

namespace UoT.model;

public static class AnimationOffsets {
  /// <summary>
  ///   Derived from the logic in the decomp: https://github.com/zeldaret/oot
  /// </summary>
  public static (IEnumerable<IZFile> animationFiles, int[]? offsets) GetFor(
      IZFile zFile,
      ZSegments zSegments)
    => zFile.FileName switch {
        "object_ane" => (
            zSegments.Objects.ByName("object_os_anime"),
            [0x718, 0x7d0, 0xa630, 0x9f94]
        ),
        "object_aob" => (
            zSegments.Objects.ByName("object_os_anime"),
            [0x92c]
        ),
        "object_bba" => (
            zSegments.Objects.ByName("object_os_anime"),
            [0x1e7c]
        ),
        "object_bji" => (
            zSegments.Objects.ByName("object_os_anime"),
            [0x1f18]
        ),
        _ => (
            zFile.Yield(),
            /*zSegments
                .Objects
                .Where(s => s.FileName.EndsWith("_anime") ||
                            s.FileName.EndsWith("_keep") ||
                            s.FileName.EndsWith("_animetion") ||
                            s.FileName.EndsWith("_anime1") ||
                            s.FileName.EndsWith("_anime2") ||
                            s.FileName.EndsWith("_anime3"))
                .Concat(zFile.Yield()),*/
            null)
    };
}