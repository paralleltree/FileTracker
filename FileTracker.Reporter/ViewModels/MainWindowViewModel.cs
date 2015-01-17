using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using FileTracker.Reporter.Models;

namespace FileTracker.Reporter.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        private Model Model { get; set; }

        public bool IsInfoGiven
        {
            get
            {
                if (Model == null) return false;
                return Model.IsInfoGiven;
            }
        }

        public string InfoPath
        {
            get
            {
                if (Model == null) return "";
                return Model.InfoPath;
            }
        }

        public ExceptionInfo Info
        {
            get
            {
                if (Model == null) return null;
                return Model.ExceptionInfo;
            }
        }


        public void Initialize()
        {
            Model = Model.Instance;
            RaisePropertyChanged("IsInfoGiven");
            RaisePropertyChanged("Info");
        }


        #region SaveInfoCommand
        private ViewModelCommand _SaveInfoCommand;

        public ViewModelCommand SaveInfoCommand
        {
            get
            {
                if (_SaveInfoCommand == null)
                {
                    _SaveInfoCommand = new ViewModelCommand(SaveInfo, CanSaveInfo);
                }
                return _SaveInfoCommand;
            }
        }

        public bool CanSaveInfo()
        {
            return IsInfoGiven;
        }

        public void SaveInfo()
        {
            Model.SaveInfo();
        }
        #endregion

        #region SelectFileCommand
        private ListenerCommand<OpeningFileSelectionMessage> _SelectFileCommand;

        public ListenerCommand<OpeningFileSelectionMessage> SelectFileCommand
        {
            get
            {
                if (_SelectFileCommand == null)
                {
                    _SelectFileCommand = new ListenerCommand<OpeningFileSelectionMessage>(SelectFile);
                }
                return _SelectFileCommand;
            }
        }

        public void SelectFile(OpeningFileSelectionMessage parameter)
        {
            if (parameter.Response == null) return;
            Model.ExceptionInfo.RunningInfo.FilePath = parameter.Response;
        }
        #endregion

        #region LaunchExplorerCommand
        private ListenerCommand<string> _LaunchExplorerCommand;

        public ListenerCommand<string> LaunchExplorerCommand
        {
            get
            {
                if (_LaunchExplorerCommand == null)
                {
                    _LaunchExplorerCommand = new ListenerCommand<string>(LaunchExplorer);
                }
                return _LaunchExplorerCommand;
            }
        }

        public void LaunchExplorer(string parameter)
        {
            Process.Start("explorer", @"/select," + parameter);
        }
        #endregion

        #region OpenUrlCommand
        private ListenerCommand<string> _OpenUrlCommand;

        public ListenerCommand<string> OpenUrlCommand
        {
            get
            {
                if (_OpenUrlCommand == null)
                {
                    _OpenUrlCommand = new ListenerCommand<string>(OpenUrl);
                }
                return _OpenUrlCommand;
            }
        }

        public void OpenUrl(string parameter)
        {
            Process.Start(parameter);
        }
        #endregion
    }
}
