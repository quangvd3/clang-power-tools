using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Management.Automation;

namespace ClangPowerTools
{
  public static class PowerShellWrapper
  {
    #region Members

    private static PowerShell mPowerShell;
    private static ObservableCollection<string> mOutput;
    private static AsyncPackage mAsyncPackage;

    #endregion


    #region Properties

    public static NotifyCollectionChangedEventHandler DataHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> ErrorHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> WaringHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> VerboseHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> InformationHandlerPowerShell { get; set; }
    public static EventHandler<DataAddedEventArgs> ProgressHandlerPowerShell { get; set; }

    #endregion

    #region Public Methods


    public static void Initialize(AsyncPackage aAsyncPackage)
    {
      mAsyncPackage = aAsyncPackage;
      mPowerShell = PowerShell.Create();
      mOutput = new ObservableCollection<string>();

      mOutput.CollectionChanged += DataHandlerPowerShell;
      mPowerShell.Streams.Error.DataAdded += ErrorHandlerPowerShell;
      mPowerShell.Streams.Warning.DataAdded += WaringHandlerPowerShell;
      mPowerShell.Streams.Verbose.DataAdded += VerboseHandlerPowerShell;
      mPowerShell.Streams.Information.DataAdded += InformationHandlerPowerShell;
      mPowerShell.Streams.Progress.DataAdded += ProgressHandlerPowerShell;
    }


    public async static void Invoke(string aScriptPath, string aArguments)
    {
      await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(mAsyncPackage.DisposalToken);

      mPowerShell.AddScript("Set-ExecutionPolicy Unrestricted -Scope Process");
      mPowerShell.AddCommand(aScriptPath);
      mPowerShell.AddArgument("-NoProfile -Noninteractive");
      mPowerShell.AddArgument(aArguments);

      mPowerShell.Invoke(null, mOutput);
    }

    public static void Clear()
    {
      mOutput.CollectionChanged -= DataHandlerPowerShell;
      mPowerShell.Streams.Error.DataAdded -= ErrorHandlerPowerShell;
      mPowerShell.Streams.Warning.DataAdded -= WaringHandlerPowerShell;
      mPowerShell.Streams.Verbose.DataAdded -= VerboseHandlerPowerShell;
      mPowerShell.Streams.Information.DataAdded -= InformationHandlerPowerShell;
      mPowerShell.Streams.Progress.DataAdded -= ProgressHandlerPowerShell;
    }

    #endregion

  }
}
