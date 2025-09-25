using ReactiveUI;

namespace fin.ui.avalonia;

public interface IViewModelBase
    : IReactiveNotifyPropertyChanged<IReactiveObject>,
      IHandleObservableErrors,
      IReactiveObject;

public abstract class BViewModel : ReactiveObject, IViewModelBase;