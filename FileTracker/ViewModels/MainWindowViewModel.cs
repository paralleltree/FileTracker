﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        public ReadOnlyDispatcherCollection<FolderItemViewModel> TrackingFolders { get; private set; }

        public void Initialize()
        {
            Model = Model.Instance;
            TrackingFolders = ViewModelHelper.CreateReadOnlyDispatcherCollection(Model.TrackingFolders, p => new FolderItemViewModel(p), DispatcherHelper.UIDispatcher);
            RaisePropertyChanged("TrackingFolders");

            var listener = new EventListener<EventHandler<TransmissionMessageEventArgs>>(
                h => Model.MessageRaised += h,
                h => Model.MessageRaised -= h,
                (sender, e) =>
                {
                    Messenger.Raise(new InformationMessage(e.Message, "FileTracker", "InformationMessage"));
                    e.Handled = true;
                });
            CompositeDisposable.Add(listener);
        }


        #region AddFolderCommand
        private ListenerCommand<string> _AddFolderCommand;

        public ListenerCommand<string> AddFolderCommand
        {
            get
            {
                if (_AddFolderCommand == null)
                {
                    _AddFolderCommand = new ListenerCommand<string>(AddFolder);
                }
                return _AddFolderCommand;
            }
        }

        public void AddFolder(string parameter)
        {
            string path = Regex.Replace(parameter, @"\\{2,}\Z", "");

            if (!Directory.Exists(path))
            {
                Messenger.Raise(new InformationMessage()
                {
                    Text = "指定のフォルダは見つかりませんでした。",
                    Caption = "登録エラー",
                    Image = System.Windows.MessageBoxImage.Exclamation,
                    MessageKey = "InformationMessage"
                });
                return;
            }

            if (TrackingFolders.Any(p => string.Equals(p.Path, path, StringComparison.OrdinalIgnoreCase)))
            {
                Messenger.Raise(new InformationMessage()
                {
                    Text = "既に登録されたフォルダです。",
                    Caption = "登録エラー",
                    Image = System.Windows.MessageBoxImage.Exclamation,
                    MessageKey = "InformationMessage"
                });
                return;
            }

            new FolderItem(Model, path).AddToCollection();
        }
        #endregion

        #region RemoveFolderCommand
        private ListenerCommand<FolderItemViewModel> _RemoveFolderCommand;

        public ListenerCommand<FolderItemViewModel> RemoveFolderCommand
        {
            get
            {
                if (_RemoveFolderCommand == null)
                {
                    _RemoveFolderCommand = new ListenerCommand<FolderItemViewModel>(RemoveFolder);
                }
                return _RemoveFolderCommand;
            }
        }

        public void RemoveFolder(FolderItemViewModel parameter)
        {
            parameter.RemoveCommand.Execute();
        }
        #endregion


        protected override void Dispose(bool disposing)
        {

            Model.Dispose();
            base.Dispose(disposing);
        }
    }
}
