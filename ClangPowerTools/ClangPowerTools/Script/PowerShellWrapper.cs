using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Management.Automation;

namespace ClangPowerTools
{
  public static class PowerShellWrapper
  {
    #region Members

    private static PowerShell mPowerShell;
    private static ObservableCollection<string> mOutput = new ObservableCollection<string>();

    #endregion


    #region Properties

    //public static DataReceivedEventHandler DataErrorHandler { get; set; }
    //public static DataReceivedEventHandler DataHandler { get; set; }
    //public static EventHandler ExitedHandler { get; set; }


    public static NotifyCollectionChangedEventHandler DataHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> ExitedHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> WaringHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> VerboseHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> InformationHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> ProgressHandlerPowerShell { get; set; }

    #endregion

    #region Public Methods


    public static void Initialize()
    {
      mPowerShell = PowerShell.Create();
      mOutput.CollectionChanged += DataHandlerPowerShell;
      mPowerShell.Streams.Error.DataAdded += ExitedHandlerPowerShell;
      mPowerShell.Streams.Warning.DataAdded += WaringHandlerPowerShell;
      mPowerShell.Streams.Verbose.DataAdded += VerboseHandlerPowerShell;
      mPowerShell.Streams.Information.DataAdded += InformationHandlerPowerShell;
      mPowerShell.Streams.Progress.DataAdded += ProgressHandlerPowerShell;
    }


    public static void Invoke(string aScript, RunningProcesses aRunningProcesses)
    {
      mPowerShell.AddScript("Set-ExecutionPolicy Unrestricted -Scope Process");
      mPowerShell.AddArgument("-NoProfile -Noninteractive");
      mPowerShell.AddScript(aScript);

      mPowerShell.Invoke(null, mOutput);

      //mPowerShell.Streams.ClearStreams();
      
      //var process = new Process();
      //try
      //{
      //  process.StartInfo = new ProcessStartInfo()
      //  {
      //    FileName = $"{Environment.SystemDirectory}\\{ScriptConstants.kPowerShellPath}",
      //    RedirectStandardError = true,
      //    RedirectStandardOutput = true,
      //    CreateNoWindow = true,
      //    UseShellExecute = false,
      //    Arguments = aScript
      //  };

      //  process.EnableRaisingEvents = true;
      //  process.ErrorDataReceived += DataErrorHandler;
      //  process.OutputDataReceived += DataHandler;
      //  process.Exited += ExitedHandler;
      //  process.Disposed += ExitedHandler;

      //  aRunningProcesses.Add(process);

      //  process.Start();
      //  process.BeginErrorReadLine();
      //  process.BeginOutputReadLine();
      //  process.WaitForExit();
      //}
      //catch (Exception)
      //{
      //}
      //finally
      //{
      //  process.ErrorDataReceived -= DataErrorHandler;
      //  process.OutputDataReceived -= DataHandler;
      //  process.Exited -= ExitedHandler;
      //  process.EnableRaisingEvents = false;

      //}
      //return process;
    }

    public static void Clear()
    {
      mOutput.CollectionChanged -= DataHandlerPowerShell;
      mPowerShell.Streams.Error.DataAdded -= ExitedHandlerPowerShell;
      mPowerShell.Streams.Warning.DataAdded -= WaringHandlerPowerShell;
      mPowerShell.Streams.Verbose.DataAdded -= VerboseHandlerPowerShell;
      mPowerShell.Streams.Information.DataAdded -= InformationHandlerPowerShell;
      mPowerShell.Streams.Progress.DataAdded -= ProgressHandlerPowerShell;
    }

    #endregion

  }
}

