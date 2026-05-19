using System.Numerics;

using Avalonia.Controls;
using Avalonia.Media;

using fin.language.equations.fixedFunction;
using fin.ui;

using ReactiveUI;

using IColorRegister = fin.language.equations.fixedFunction.IColorRegister;

namespace uni.ui.avalonia.resources.registers;

public sealed class ColorRegisterPickerViewModelForDesigner
    : ColorRegisterPickerViewModel {
  public ColorRegisterPickerViewModelForDesigner() {
    this.ColorRegister
        = new FixedFunctionRegisters().GetOrCreateColorRegister(
            "foobar",
            new ColorConstant(.3f, .4f, .5f));
  }
}

public class ColorRegisterPickerViewModel : BViewModel {
  public required IColorRegister ColorRegister {
    get;
    set {
      this.RaiseAndSetIfChanged(ref field, value);

      var rgb = value.Value * 255;
      this.Color = new Color(255, (byte) rgb.X, (byte) rgb.Y, (byte) rgb.Z);
    }
  }

  public Color Color {
    get;
    set {
      if (field == value) {
        return;
      }

      this.RaiseAndSetIfChanged(ref field, value);
      this.ColorRegister.Value = new Vector3(value.R, value.G, value.B) / 255;
    }
  }
}

public partial class ColorRegisterPicker : UserControl {
  public ColorRegisterPicker() {
    this.InitializeComponent();
  }
}