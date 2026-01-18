using System.Collections.Generic;
using System.Linq;

using fin.data.indexable;
using fin.data.queues;

using readOnly;


namespace fin.model.skin;

public static class MeshExtensions {
  public static IEnumerable<IReadOnlyMesh> SelfAndChildren(
      this IReadOnlyMesh root) {
    var queue = new FinQueue<IReadOnlyMesh>(root);
    while (queue.TryDequeue(out var current)) {
      yield return current;

      queue.Enqueue(current.SubMeshes);
    }
  }
}