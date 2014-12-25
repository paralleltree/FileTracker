﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    public class FileItemViewModel : ViewModel
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

        public FileItem Source { get; private set; }
        public string Name { get { return Source.Source.Name; } }
        public DateTime LastWriteTime { get { return Source.Source.LastWriteTime; } }
        public ReadOnlyDispatcherCollection<KeyValuePair<DateTime, System.IO.FileInfo>> SnappedFiles { get; private set; }

        public FileItemViewModel(FileItem source)
        {
            this.Source = source;
            SnappedFiles = ViewModelHelper.CreateReadOnlyDispatcherCollection(Source.SnappedFiles, p => p, DispatcherHelper.UIDispatcher);
            Source.PropertyChanged += (sender, e) => RaisePropertyChanged(e.PropertyName);
        }


        #region SnapCommand
        private ViewModelCommand _SnapCommand;

        public ViewModelCommand SnapCommand
        {
            get
            {
                if (_SnapCommand == null)
                {
                    _SnapCommand = new ViewModelCommand(Snap);
                }
                return _SnapCommand;
            }
        }

        public void Snap()
        {
            Source.Snap();
        }
        #endregion

        #region RemoveCommand
        private ListenerCommand<DateTime> _RemoveCommand;

        public ListenerCommand<DateTime> RemoveCommand
        {
            get
            {
                if (_RemoveCommand == null)
                {
                    _RemoveCommand = new ListenerCommand<DateTime>(Remove, CanRemove);
                }
                return _RemoveCommand;
            }
        }

        public bool CanRemove()
        {
            return SnappedFiles.Count > 0;
        }

        public void Remove(DateTime parameter)
        {
            Source.Remove(parameter);
        }
        #endregion

        #region ClearCommand
        private ViewModelCommand _ClearCommand;

        public ViewModelCommand ClearCommand
        {
            get
            {
                if (_ClearCommand == null)
                {
                    _ClearCommand = new ViewModelCommand(Clear, CanClear);
                }
                return _ClearCommand;
            }
        }

        public bool CanClear()
        {
            return SnappedFiles.Count > 0;
        }

        public void Clear()
        {
            Source.Clear();
        }
        #endregion

        #region RestoreCommand
        private ListenerCommand<DateTime> _RestoreCommand;

        public ListenerCommand<DateTime> RestoreCommand
        {
            get
            {
                if (_RestoreCommand == null)
                {
                    _RestoreCommand = new ListenerCommand<DateTime>(Restore, CanRestore);
                }
                return _RestoreCommand;
            }
        }

        public bool CanRestore()
        {
            return SnappedFiles.Count > 0;
        }

        public void Restore(DateTime parameter)
        {
            Source.Restore(parameter);
        }
        #endregion


    }
}