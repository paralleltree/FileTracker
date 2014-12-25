// reference to: https://github.com/ugaya40/Livet/blob/master/.NET4.0/Livet(.NET4.0)/DispatcherCollection.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows.Threading;

namespace Livet
{
    public class DispatcherDictionary<TKey, TValue> : DispatcherCollection<KeyValuePair<TKey, TValue>>,
        IDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> dictionary;
        //private object _syncRoot = new object();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="dispatcher">UIDispatcher(通常はDispatcherHelper.UIDispatcher)</param>
        public DispatcherDictionary(Dispatcher dispatcher) : this(new ObservableCollection<KeyValuePair<TKey, TValue>>(), dispatcher) { }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="collection">元となるコレクション(IList(Of(T)とINotifyPropertyChangedも実装している必要があります)</param>
        /// <param name="dispatcher">UIDispatcher(通常はDispatcherHelper.UIDispatcher)</param>
        public DispatcherDictionary(INotifyCollectionChanged collection, Dispatcher dispatcher)
            : base(dispatcher)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }

            var element = (IEnumerable<KeyValuePair<TKey, TValue>>)collection;
            dictionary = new Dictionary<TKey, TValue>(element.Count());
            foreach (var e in element)
                Add(e);

            //Dispatcher = dispatcher;
            //CollectionChangedDispatcherPriority = DispatcherPriority.Normal;

            //((INotifyPropertyChanged)collection).PropertyChanged += (sender, e) =>
            //{
            //    if (!Dispatcher.CheckAccess())
            //    {
            //        Dispatcher.Invoke(CollectionChangedDispatcherPriority, (Action)(() => OnPropertyChanged(e.PropertyName)));
            //    }
            //    else
            //    {
            //        OnPropertyChanged(e.PropertyName);
            //    }
            //};

            //collection.CollectionChanged += (sender, e) =>
            //{
            //    if (!Dispatcher.CheckAccess())
            //    {
            //        Dispatcher.Invoke(CollectionChangedDispatcherPriority, (Action)(() => OnCollectionChanged(e)));
            //    }
            //    else
            //    {
            //        OnCollectionChanged(e);
            //    }
            //};
        }


        ///// <summary>
        ///// このコレクションに関連付けられたDispatcherを取得、または設定します。
        ///// </summary>
        //public Dispatcher Dispatcher { get; set; }

        ///// <summary>
        ///// このコレクションに関連付けられたDispatcherを取得、または設定します。
        ///// </summary>
        //public DispatcherPriority CollectionChangedDispatcherPriority { get; set; }


        /// <summary>
        /// 指定のキーと要素を追加します。
        /// </summary>
        /// <param name="key">追加するキー</param>
        /// <param name="value">キーに関連付ける要素</param>
        public void Add(TKey key, TValue value)
        {
            dictionary.Add(key, value);
            base.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// 指定のキーがこのディクショナリに含まれているかどうかを判断します。
        /// </summary>
        /// <param name="key">判断するキー</param>
        /// <returns>キーが含まれているかどうか</returns>
        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// このディクショナリのキーを格納しているコレクションを取得します。
        /// </summary>
        public ICollection<TKey> Keys
        {
            get { return dictionary.Keys; }
        }

        /// <summary>
        /// 指定のキーを持つ要素をこのディクショナリから削除します。
        /// </summary>
        /// <param name="key">削除する要素のキー</param>
        /// <returns>正常に削除されたかどうか</returns>
        public bool Remove(TKey key)
        {
            var kvp = new KeyValuePair<TKey, TValue>(key, dictionary[key]);
            dictionary.Remove(key);
            return base.Remove(kvp);

        }

        /// <summary>
        /// 指定したキーに関連付けられている値を取得します。
        /// </summary>
        /// <param name="key">取得する値のキー</param>
        /// <param name="value">キーに関連付けられた値が格納されるパラメータ</param>
        /// <returns>指定のキーを持つ要素が格納されているかどうか</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// このディクショナリ内の値を格納しているコレクションを取得します。
        /// </summary>
        public ICollection<TValue> Values
        {
            get { return dictionary.Values; }
        }

        /// <summary>
        /// 指定のキーの関連付けられている要素を取得または設定します。
        /// </summary>
        /// <param name="key">取得、設定する要素のキー</param>
        /// <returns>取得した要素</returns>
        public TValue this[TKey key]
        {
            get
            {
                return dictionary[key];
            }
            set
            {
                var old = new KeyValuePair<TKey, TValue>(key, dictionary[key]);
                int index = base.IndexOf(old);
                base.Remove(old);
                dictionary[key] = value;
                base.Insert(index, new KeyValuePair<TKey, TValue>(key, value));
            }
        }

        /// <summary>
        /// 指定したキーと要素をディクショナリに追加します。
        /// </summary>
        /// <param name="item">追加する<see cref="System.Collections.Generic.KeyValuePair{TKey, TValue}"/></param>

        public new void Add(KeyValuePair<TKey, TValue> item)
        {
            dictionary.Add(item.Key, item.Value);
            base.Add(item);
        }

        /// <summary>
        /// このディクショナリからすべての要素を削除します。
        /// </summary>
        public new void Clear()
        {
            dictionary.Clear();
            base.Clear();
        }

        ///// <summary>
        ///// 指定の要素がこのディクショナリに含まれているかどうかを判断します。
        ///// </summary>
        ///// <param name="item">検索する要素</param>
        ///// <returns>含まれているかどうか</returns>
        //public new bool Contains(KeyValuePair<TKey, TValue> item)
        //{
        //    return base.Contains(item);
        //}

        ///// <summary>
        ///// 全体を互換性のある1次元の配列にコピーします。コピー操作は、コピー先の配列の指定したインデックスから始まります。
        ///// </summary>
        ///// <param name="array">コピー先の配列</param>
        ///// <param name="arrayIndex">コピー先の配列のどこからコピー操作をするかのインデックス</param>
        //public new void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        //{
        //    base.CopyTo(array, arrayIndex);
        //}

        ///// <summary>
        ///// 実際に格納されている要素の数を取得します。
        ///// </summary>
        //public new int Count
        //{
        //    get { return base.Count; }
        //}

        ///// <summary>
        ///// このコレクションが読み取り専用かどうかを取得します。
        ///// </summary>
        //public new bool IsReadOnly
        //{
        //    get { return base.IsReadOnly; }
        //}

        /// <summary>
        /// 指定の要素を削除します。
        /// </summary>
        /// <param name="item">削除する要素</param>
        /// <returns>正常に削除したかどうか</returns>
        public new bool Remove(KeyValuePair<TKey, TValue> item)
        {
            dictionary.Remove(item.Key);
            return base.Remove(item);
        }

        ///// <summary>
        ///// 反復処理するための列挙子を返します。
        ///// </summary>
        ///// <returns>列挙子</returns>
        //public new IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        //{
        //    return base.GetEnumerator();
        //}

        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return base.GetEnumerator();
        //}

        //public new void CopyTo(Array array, int index)
        //{
        //    CopyTo(array.Cast<KeyValuePair<TKey, TValue>>().ToArray(), index);
        //}

        //public bool IsSynchronized
        //{
        //    get { return _sourceAsIDictionary is ObservableSynchronizedCollection<KeyValuePair<TKey, TValue>>; }
        //}

        //public object SyncRoot
        //{
        //    get { return _syncRoot; }
        //}


        //protected void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        //{
        //    var threadSafeHandler = Interlocked.CompareExchange(ref CollectionChanged, null, null);

        //    if (threadSafeHandler != null)
        //    {
        //        threadSafeHandler(this, args);
        //    }
        //}

        //protected void OnPropertyChanged(string propertyName)
        //{
        //    var threadSafeHandler = Interlocked.CompareExchange(ref PropertyChanged, null, null);

        //    if (threadSafeHandler != null)
        //    {
        //        threadSafeHandler(this, EventArgsFactory.GetPropertyChangedEventArgs(propertyName));
        //    }
        //}


        ///// <summary>
        ///// プロパティが変更された際に発生するイベントです。
        ///// </summary>
        //[field: NonSerialized]
        //public event NotifyCollectionChangedEventHandler CollectionChanged;

        ///// <summary>
        ///// コレクションが変更された際に発生するイベントです。
        ///// </summary>
        //[field: NonSerialized]
        //public event PropertyChangedEventHandler PropertyChanged;

    }
}
