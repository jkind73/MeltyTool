using System.Collections.Generic;

using fin.util.asserts;

using NUnit.Framework;

namespace fin.model.util;

public sealed class RenderPriorityOrderedSetTests {
  [Test]
  public void TestSimplestOrder() {
    var impl = new RenderPriorityOrderedSet<string> {
        { "2 abc", 1, 1, true },
        { "2 123", 1, 1, false },
        { "2 bar", 1, 0, true },
        { "2 foo", 1, 0, false },
        { "1 abc", 0, 1, true },
        { "1 123", 0, 1, false },
        { "1 bar", 0, 0, true },
        { "1 foo", 0, 0, false },
    };

    Asserts.SequenceEqual<IEnumerable<(bool, int, uint, string)>>(
        impl,
        [
            // Opaque
            (false, 0, 0, "1 foo"),
            (false, 1, 0, "2 foo"),
            (false, 0, 1, "1 123"),
            (false, 1, 1, "2 123"),
            // Transparent
            (true, 0, 0, "1 bar"),
            (true, 1, 0, "2 bar"),
            (true, 0, 1, "1 abc"),
            (true, 1, 1, "2 abc"),
        ]);
  }

  [Test]
  public void TestSimplestOrderWithDuplicates() {
    var impl = new RenderPriorityOrderedSet<string> {
        { "bar-1", 0, 3, true },
        { "bar-2", 0, 3, true },
        { "foo", 0, 2, false },
        { "abc", 0, 1, true },
        { "123", 0, 1, false },
        { "bar-2", 0, 0, true },
        { "bar-1", 0, 0, true },
        { "foo", 0, 0, false },
    };

    Asserts.SequenceEqual<IEnumerable<(bool, int, uint, string)>>(
        impl,
        [
            (false, 0, 0, "foo"),
            (false, 0, 1, "123"),
            (true, 0, 0, "bar-1"),
            (true, 0, 0, "bar-2"),
            (true, 0, 1, "abc"),
        ]);
  }
}