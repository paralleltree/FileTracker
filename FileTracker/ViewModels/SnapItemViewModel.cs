using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using FileTracker.Models;

namespace FileTracker.ViewModels
{
    public class SnapItemViewModel : ViewModel
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

        private SnapItem Source { get; set; }
        public DateTime SnappedTime { get { return Source.SnappedTime; } }
        public long Length { get { return Source.SnappedFile.Length; } }

        public SnapItemViewModel(SnapItem source)
        {
            this.Source = source;
        }


        #region RemoveCommand
        private ViewModelCommand _RemoveCommand;

        public ViewModelCommand RemoveCommand
        {
            get
            {
                if (_RemoveCommand == null)
                {
                    _RemoveCommand = new ViewModelCommand(Remove);
                }
                return _RemoveCommand;
            }
        }

        public void Remove()
        {
            Source.Remove();
        }
        #endregion

        #region RestoreCommand
        private ViewModelCommand _RestoreCommand;

        public ViewModelCommand RestoreCommand
        {
            get
            {
                if (_RestoreCommand == null)
                {
                    _RestoreCommand = new ViewModelCommand(Restore);
                }
                return _RestoreCommand;
            }
        }

        public void Restore()
        {
            if (File.Exists(Source.Destination))
            {
                var msg = new ConfirmationMessage()
                {
                    Text = "既に同じ復元ファイルが存在します。\n上書きして復元しますか？",
                    Caption = "ファイルの復元",
                    Image = System.Windows.MessageBoxImage.Question,
                    Button = System.Windows.MessageBoxButton.YesNo,
                    MessageKey = "ConfirmationMessage"
                };
                DispatcherHelper.UIDispatcher.Invoke(() => Messenger.GetResponse(msg));

                if (!msg.Response.HasValue || !msg.Response.Value) return;
            }

            Source.Restore();
        }
        #endregion
    }
}
