using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace fin.util.cmd;

/// <summary>
/// Allows processes to be automatically killed if this parent process unexpectedly quits.
/// This feature requires Windows 8 or greater. On Windows 7, nothing is done.</summary>
/// <remarks>References:
///  https://stackoverflow.com/a/4657392/386091
///  https://stackoverflow.com/a/9164742/386091 </remarks>
public static class ChildProcessTracker {
  /// <summary>
  /// Add the process to be tracked. If our current process is killed, the child processes
  /// that we are tracking will be automatically killed, too. If the child process terminates
  /// first, that's fine, too.</summary>
  /// <param name="process"></param>
  public static void AddProcess(Process process) {
    if (S_JOB_HANDLE_ != IntPtr.Zero) {
      bool success = AssignProcessToJobObject(S_JOB_HANDLE_, process.Handle);
      if (!success && !process.HasExited)
        throw new Win32Exception();
    }
  }

  static ChildProcessTracker() {
    // This feature requires Windows 8 or later. To support Windows 7 requires
    //  registry settings to be added if you are using Visual Studio plus an
    //  app.manifest change.
    //  https://stackoverflow.com/a/4232259/386091
    //  https://stackoverflow.com/a/9507862/386091
    if (Environment.OSVersion.Version < new Version(6, 2))
      return;

    // The job name is optional (and can be null) but it helps with diagnostics.
    //  If it's not null, it has to be unique. Use SysInternals' Handle command-line
    //  utility: handle -a ChildProcessTracker
    string jobName = "ChildProcessTracker" + Process.GetCurrentProcess().Id;
    S_JOB_HANDLE_ = CreateJobObject(IntPtr.Zero, jobName);

    var info = new JobobjectBasicLimitInformation();

    // This is the key flag. When our process is killed, Windows will automatically
    //  close the job handle, and when that happens, we want the child processes to
    //  be killed, too.
    info.LimitFlags = Jobobjectlimit.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

    var extendedInfo = new JobobjectExtendedLimitInformation();
    extendedInfo.BasicLimitInformation = info;

    int length = Marshal.SizeOf(typeof(JobobjectExtendedLimitInformation));
    IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);
    try {
      Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

      if (!SetInformationJobObject(S_JOB_HANDLE_, JobObjectInfoType.EXTENDED_LIMIT_INFORMATION,
          extendedInfoPtr, (uint)length)) {
        throw new Win32Exception();
      }
    } finally {
      Marshal.FreeHGlobal(extendedInfoPtr);
    }
  }

  [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
  static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string name);

  [DllImport("kernel32.dll")]
  static extern bool SetInformationJobObject(IntPtr job, JobObjectInfoType infoType,
      IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

  [DllImport("kernel32.dll", SetLastError = true)]
  static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

  // Windows will automatically close any open job handles when our process terminates.
  //  This can be verified by using SysInternals' Handle utility. When the job handle
  //  is closed, the child processes will be killed.
  private static readonly IntPtr S_JOB_HANDLE_;
}

public enum JobObjectInfoType {
  ASSOCIATE_COMPLETION_PORT_INFORMATION = 7,
  BASIC_LIMIT_INFORMATION = 2,
  BASIC_UI_RESTRICTIONS = 4,
  END_OF_JOB_TIME_INFORMATION = 6,
  EXTENDED_LIMIT_INFORMATION = 9,
  SECURITY_LIMIT_INFORMATION = 5,
  GROUP_INFORMATION = 11
}

[StructLayout(LayoutKind.Sequential)]
public struct JobobjectBasicLimitInformation {
  public Int64 PerProcessUserTimeLimit;
  public Int64 PerJobUserTimeLimit;
  public Jobobjectlimit LimitFlags;
  public UIntPtr MinimumWorkingSetSize;
  public UIntPtr MaximumWorkingSetSize;
  public UInt32 ActiveProcessLimit;
  public Int64 Affinity;
  public UInt32 PriorityClass;
  public UInt32 SchedulingClass;
}

[Flags]
public enum Jobobjectlimit : uint {
  JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000
}

[StructLayout(LayoutKind.Sequential)]
public struct IoCounters {
  public UInt64 ReadOperationCount;
  public UInt64 WriteOperationCount;
  public UInt64 OtherOperationCount;
  public UInt64 ReadTransferCount;
  public UInt64 WriteTransferCount;
  public UInt64 OtherTransferCount;
}

[StructLayout(LayoutKind.Sequential)]
public struct JobobjectExtendedLimitInformation {
  public JobobjectBasicLimitInformation BasicLimitInformation;
  public IoCounters IoInfo;
  public UIntPtr ProcessMemoryLimit;
  public UIntPtr JobMemoryLimit;
  public UIntPtr PeakProcessMemoryUsed;
  public UIntPtr PeakJobMemoryUsed;
}