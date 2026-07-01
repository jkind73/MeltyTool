using System.Reactive.Subjects;

using fin.io.bundles;
using fin.util.types;

namespace uni.services;

[IocCandiate]
public static class ContextService {
  private static readonly BehaviorSubject<IFileBundle?>
      fileBundleSubject_ = new(null);

  public static IObservable<IFileBundle?> ΔFileBundle
    => fileBundleSubject_;

  public static void SetCurrentFileBundle(IFileBundle? fileBundle)
    => fileBundleSubject_.OnNext(fileBundle);
}