using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.IO;
using System.Diagnostics;
using System.Reflection;

using Livet;
using FileTracker.Reporter;

namespace FileTracker
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif
        }

        static readonly string DumpName = "exception_{0}.json";
        static readonly string ReporterName = "Reporter.exe";

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                var asm = Assembly.GetExecutingAssembly();
                var info = new ExceptionInfo()
                {
                    EnvironmentInfo = new EnvironmentInfo()
                    {
                        OSVersion = Environment.OSVersion.Version,
                        ImageRuntimeVersion = asm.ImageRuntimeVersion,
                        RuntimeVersion = Environment.Version,
                        AssemblyVersion = asm.GetName().Version,
                        Is64BitProcess = Environment.Is64BitProcess,
                        ExecutionPath = asm.Location
                    },
                    RunningInfo = new RunningInfo()
                    {
                        Timestamp = DateTime.Now,
                        ProcessId = Process.GetCurrentProcess().Id,
                        Exception = (Exception)e.ExceptionObject
                    }
                };

                string dest = string.Format(DumpName, Path.GetRandomFileName());
                ExceptionInfo.WriteInfo(info, dest);

                DispatcherHelper.UIDispatcher.Invoke(() =>
                {
                    MessageBox.Show(
                        Application.Current.MainWindow,
                        "開発者の想定外のエラーが発生しました。\nアプリケーションを終了します。",
                        "FileTracker",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                });

                Process.Start(ReporterName, dest);
            }
            catch (Exception)
            {
            }

            Environment.Exit(1);
        }
    }
}
