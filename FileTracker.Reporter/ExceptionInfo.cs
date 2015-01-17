using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace FileTracker.Reporter
{
    public class ExceptionInfo
    {
        /// <summary>
        /// プロセス実行時の環境情報を格納します。
        /// </summary>
        public EnvironmentInfo EnvironmentInfo { get; set; }

        /// <summary>
        /// 例外発生時の実行情報を格納します。
        /// </summary>
        public RunningInfo RunningInfo { get; set; }


        /// <summary>
        /// ExceptionInfoのインスタンスをファイルに保存します。
        /// </summary>
        /// <param name="info">保存するExceptionInfoインスタンス</param>
        /// <param name="path">保存先</param>
        public static void WriteInfo(ExceptionInfo info, string path)
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(info, Formatting.Indented));
        }

        /// <summary>
        /// 指定のファイルからExceptionInfoのインスタンスを生成します。
        /// </summary>
        /// <param name="path">読み込み先</param>
        /// <returns>生成されたExceptionInfoインスタンス</returns>
        public static ExceptionInfo ReadInfo(string path)
        {
            return JsonConvert.DeserializeObject<ExceptionInfo>(File.ReadAllText(path));
        }
    }

    public class EnvironmentInfo
    {
        /// <summary>
        /// プロセス実行時のOSのバージョンを格納します。
        /// </summary>
        public Version OSVersion { get; set; }

        /// <summary>
        /// アセンブリの作成に使用された共通言語ランタイムのバージョンを格納します。
        /// </summary>
        public string ImageRuntimeVersion { get; set; }

        /// <summary>
        /// プロセス実行時の共通言語ランタイムのバージョンを格納します。
        /// </summary>
        public Version RuntimeVersion { get; set; }

        /// <summary>
        /// アセンブリ本体のバージョンを格納します。
        /// </summary>
        public Version AssemblyVersion { get; set; }

        /// <summary>
        /// 実行プロセスが64ビットプロセスであるかどうかを格納します。
        /// </summary>
        public bool Is64BitProcess { get; set; }

        /// <summary>
        /// プロセスの実行パスを格納します。
        /// </summary>
        public string ExecutionPath { get; set; }
    }

    public class RunningInfo
    {
        /// <summary>
        /// 例外の記録日時を格納します。
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 実行時のプロセスIDを格納します。
        /// </summary>
        public int ProcessId { get; set; }

        /// <summary>
        /// 例外オブジェクトを格納します。
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// ユーザーが使用していたソフトウェア名を格納します。
        /// </summary>
        public string SoftwareName { get; set; }

        /// <summary>
        /// ユーザーが編集していたファイルのパスを格納します。
        /// </summary>
        public IEnumerable<string> FilePath { get; set; }

        /// <summary>
        /// ユーザーからの付加情報を格納します。
        /// </summary>
        public string UserComment { get; set; }
    }
}
