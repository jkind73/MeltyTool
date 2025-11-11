using System.Collections.Generic;
using System.Linq;

using UoT.memory;

namespace UoT.util;

public static class EnumerableExtensions {
  public static IEnumerable<ZObject> ByName(this IEnumerable<ZObject> objs,
                                            string name)
    => objs.Where(o => o.FileName == name);
}