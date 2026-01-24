using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;

namespace benchmarks;

public sealed class DependencyInjection {
  private const int N_ = 100000;
  private const float VALUE_ = 1;


  public interface IMethod {
    double Run(double x);
  }


    
    
  [Benchmark]
  public void CallDirectly() {
    for (var i = 0; i < N_; i++) {
      var x = Math.Acos(VALUE_);
    }
  }



  [Benchmark]
  public void CallViaMethod() {
    var viaMethod = new ViaMethodWrapper();
    for (var i = 0; i < N_; i++) {
      var x = viaMethod.Run(VALUE_);
    }
  }

  public sealed class ViaMethodWrapper : IMethod {
    public double Run(double d) => Math.Acos(d);
  }




  [Benchmark]
  public void CallViaInline() {
    var viaInlineWrapper = new ViaInlineWrapper();
    for (var i = 0; i < N_; i++) {
      var x = viaInlineWrapper.Run(VALUE_);
    }
  }

  public sealed class ViaInlineWrapper : IMethod {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Run(double d) => Math.Acos(d);
  }



  [Benchmark]
  public void CallViaOptimizedInline() {
    var viaOptimizedInlineWrapper = new ViaOptimizedInlineWrapper();
    for (var i = 0; i < N_; i++) {
      var x = viaOptimizedInlineWrapper.Run(VALUE_);
    }
  }

  public sealed class ViaOptimizedInlineWrapper : IMethod {
    [MethodImpl(MethodImplOptions.AggressiveInlining |
                MethodImplOptions.AggressiveOptimization)]
    public double Run(double d) => Math.Acos(d);
  }





  [Benchmark]
  public void CallViaMethodGroup() {
    var viaMethodGroup = new ViaMethodGroup(Math.Acos);
    for (var i = 0; i < N_; i++) {
      var x = viaMethodGroup.methodGroup(VALUE_);
    }
  }

  public sealed class ViaMethodGroup(Func<double, double> methodGroup) {
    public Func<double, double> methodGroup = methodGroup;
  }




  [Benchmark]
  public void CallViaLambda() {
    var viaLambda = new ViaLambda(d => Math.Acos(d));
    for (var i = 0; i < N_; i++) {
      var x = viaLambda.lambda(VALUE_);
    }
  }

  public sealed class ViaLambda(Func<double, double> lambda) {
    public Func<double, double> lambda = lambda;
  }




  [Benchmark]
  public void CallViaInterfaceImpl() {
    var viaInterface = new ViaInterfaceImpl(new ViaOptimizedInlineWrapper());
    for (var i = 0; i < N_; i++) {
      var x = viaInterface.Run(VALUE_);
    }
  }

  public sealed class ViaInterfaceImpl(IMethod impl) : IMethod {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Run(double x)  => impl.Run(x);
  }



  [Benchmark]
  public void CallViaGenericImpl() {
    var viaGeneric = new ViaGenericImpl<ViaOptimizedInlineWrapper>(new ViaOptimizedInlineWrapper());
    for (var i = 0; i < N_; i++) {
      var x = viaGeneric.Run(VALUE_);
    }
  }

  public sealed class ViaGenericImpl<TMethod>(TMethod impl)
      where TMethod : IMethod {
    private readonly TMethod impl_ = impl;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Run(double x) => this.impl_.Run(x);
  }




  [Benchmark]
  public void CallViaStruct() {
    var viaGeneric = new ViaStruct();
    for (var i = 0; i < N_; i++) {
      var x = viaGeneric.Run(VALUE_);
    }
  }

  public readonly struct ViaStruct : IMethod {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Run(double x) => Math.Acos(x);
  }


  [Benchmark]
  public void CallViaGenericStruct() {
    var viaGeneric = new ViaGenericImpl<ViaStruct>(new ViaStruct());
    for (var i = 0; i < N_; i++) {
      var x = viaGeneric.Run(VALUE_);
    }
  }
}