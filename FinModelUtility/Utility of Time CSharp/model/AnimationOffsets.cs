using System.Collections.Generic;

using fin.util.enumerables;

using UoT.memory;
using UoT.util;

namespace UoT.model;

public sealed class AnimationOffsets {
  public (IEnumerable<IZFile> animationFiles, int[]? offsets) GetFor(
      IZFile zFile,
      ZSegments zSegments)
    => zFile.FileName switch {
        "object_ane" => (
            zSegments
                .Objects
                .ByName("object_os_anime"),
            [0x718, 0x7d0, 0xa630, 0x9f94]),
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