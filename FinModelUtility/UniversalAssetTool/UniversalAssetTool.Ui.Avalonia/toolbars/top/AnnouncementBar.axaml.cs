using System;

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

using fin.io.web;
using fin.services;
using fin.ui;
using fin.util.asserts;

using ReactiveUI;

using uni.services;

namespace uni.ui.avalonia.toolbars.top;

public sealed class AnnouncementBarViewModelForDesigner
    : AnnouncementBarViewModel {
  public AnnouncementBarViewModelForDesigner() {
    this.Announcement
        = new Announcement(AnnouncementType.ERROR,
                           "Uh oh, failed to do something.",
                           [
                               (new Exception("Here is an error message."), null)
                           ]);
  }
}

public class AnnouncementBarViewModel : BViewModel {
  public AnnouncementBarViewModel() {
    this.Announcement = null;
    AnnouncementService.OnAnnouncement
        += announcement => this.Announcement = announcement;

    ExportService.OnExportStart
        += () => {
          this.IsExporting = true;
          this.ProgressValue = 0;
        };
    ExportService.OnExportComplete
        += () => this.IsExporting = false;
    ExportService.Progress.ProgressChanged
        += (_, current) => this.ProgressValue = 100 * current.Item1;
  }

  public Announcement? Announcement {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      this.Foreground = value?.Type switch {
          AnnouncementType.ERROR => SolidColorBrush.Parse("Red"),
          AnnouncementType.INFO  => SolidColorBrush.Parse("White"),
          _                      => SolidColorBrush.Parse("Gray"),
      };
      this.Text = value?.Message ?? "No announcements.";

      var exceptionsAndContexts = value?.ExceptionsAndContexts ?? [];
      this.ExceptionAndContext
          = exceptionsAndContexts.Length > 0 ? exceptionsAndContexts[0] : null;
    }
  }

  public IBrush Foreground {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public string Text {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public (Exception, IExceptionContext?)? ExceptionAndContext {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public bool IsExporting  {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }

  public double ProgressValue {
    get;
    set => this.RaiseAndSetIfChanged(ref field, value);
  }
}

public partial class AnnouncementBar : UserControl {
  public AnnouncementBar() {
    this.DataContext ??= new AnnouncementBarViewModel();
    InitializeComponent();
  }

  private void ShowException_(object? sender, RoutedEventArgs e) {
    var dataContext = this.DataContext.AssertAsA<AnnouncementBarViewModel>();
    if (dataContext.ExceptionAndContext == null) {
      return;
    }

    var (exception, context) = dataContext.ExceptionAndContext.Value;
    ExceptionService.HandleException(exception, context);
  }
}