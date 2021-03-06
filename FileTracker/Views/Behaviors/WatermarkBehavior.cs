﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Interactivity;

namespace FileTracker.Views.Behaviors
{
    /// <summary>
    /// <see cref="System.Windows.Controls.TextBox"/>へのウォーターマークの表示を行うビヘイビアです。
    /// </summary>
    class WatermarkBehavior : Behavior<TextBox>
    {
        private bool IsInitialized { get; set; }
        private Label Watermark { get; set; }
        private VisualBrush WatermarkBrush { get; set; }

        /// <summary>
        /// ウォーターマークとして表示する文字列を取得、設定します。
        /// </summary>
        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(WatermarkBehavior), new PropertyMetadata("", OnDependencyPropertyChanged));

        /// <summary>
        /// ウォーターマークの前景色を表すブラシを取得、設定します。
        /// </summary>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        public static readonly DependencyProperty ForegroundProperty =
            DependencyProperty.Register("Foreground", typeof(Brush), typeof(WatermarkBehavior), new PropertyMetadata(Brushes.DarkGray, OnDependencyPropertyChanged));

        /// <summary>
        /// フォーカス時にウォーターマークを非表示にするかどうかを取得、設定します。
        /// </summary>
        public bool HiddenWhenFocused
        {
            get { return (bool)GetValue(HiddenWhenGetFocusProperty); }
            set { SetValue(HiddenWhenGetFocusProperty, value); }
        }

        public static readonly DependencyProperty HiddenWhenGetFocusProperty =
            DependencyProperty.Register("HiddenWhenFocused", typeof(bool), typeof(WatermarkBehavior), new PropertyMetadata(false, OnDependencyPropertyChanged));


        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += OnLoaded;
            this.AssociatedObject.TextChanged += OnTextChanged;
            this.AssociatedObject.GotFocus += OnFocusChanged;
            this.AssociatedObject.LostFocus += OnFocusChanged;

            if (this.AssociatedObject.IsLoaded) Initialize();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            this.AssociatedObject.Loaded -= OnLoaded;
            this.AssociatedObject.TextChanged -= OnTextChanged;
            this.AssociatedObject.GotFocus -= OnFocusChanged;
            this.AssociatedObject.LostFocus -= OnFocusChanged;

            BindingOperations.ClearAllBindings(Watermark);
        }


        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Initialize();
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Refresh();
        }

        private void OnFocusChanged(object sender, RoutedEventArgs e)
        {
            Refresh();
        }


        private void Refresh()
        {
            TextBox textbox = this.AssociatedObject;
            if (textbox == null) return;

            if (string.IsNullOrEmpty(textbox.Text) && (!textbox.IsFocused || !HiddenWhenFocused))
            {
                // TextBoxの背景に直接Brushを指定する
                textbox.Background = WatermarkBrush;
            }
            else
            {
                // ウォーターマーク役のLabelの背景を指定する。
                textbox.Background = Watermark.Background;
            }
        }

        private void Initialize()
        {
            if (IsInitialized) throw new InvalidOperationException("既にビヘイビアは初期化されています。");

            this.Watermark = new Label()
            {
                Background = this.AssociatedObject.Background,
                FontStyle = FontStyles.Italic,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };

            var desc = new Binding()
            {
                Path = new PropertyPath("Description"),
                Source = this
            };
            BindingOperations.SetBinding(Watermark, Label.ContentProperty, desc);

            var foreground = new Binding()
            {
                Path = new PropertyPath("Foreground"),
                Source = this
            };
            BindingOperations.SetBinding(Watermark, Label.ForegroundProperty, foreground);

            var width = new Binding()
            {
                Path = new PropertyPath("ActualWidth"),
                Source = this.AssociatedObject
            };
            BindingOperations.SetBinding(Watermark, Label.WidthProperty, width);

            this.WatermarkBrush = new VisualBrush(Watermark)
            {
                Stretch = Stretch.None,
                TileMode = TileMode.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Center
            };

            IsInitialized = true;
            Refresh();
        }


        private static void OnDependencyPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var behavior = sender as WatermarkBehavior;
            if (behavior == null) return;

            behavior.Refresh();
        }
    }
}
