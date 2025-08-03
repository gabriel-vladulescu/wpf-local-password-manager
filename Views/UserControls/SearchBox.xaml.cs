using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AccountManager.Views.UserControls
{
    public partial class SearchBox : UserControl
    {
        private DispatcherTimer _searchTimer;

        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(nameof(SearchText), typeof(string), typeof(SearchBox),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnSearchTextChanged));

        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(nameof(PlaceholderText), typeof(string), typeof(SearchBox),
                new PropertyMetadata("Search..."));

        public static readonly DependencyProperty SearchDelayProperty =
            DependencyProperty.Register(nameof(SearchDelay), typeof(int), typeof(SearchBox),
                new PropertyMetadata(300));

        public static readonly DependencyProperty ClearCommandProperty =
            DependencyProperty.Register(nameof(ClearCommand), typeof(ICommand), typeof(SearchBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register(nameof(SearchCommand), typeof(ICommand), typeof(SearchBox),
                new PropertyMetadata(null));

        // FIXED: Renamed to avoid conflict with the event accessor
        public static readonly RoutedEvent SearchTextChangedRoutedEvent =
            EventManager.RegisterRoutedEvent("SearchTextChanged", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(SearchBox));

        public string SearchText
        {
            get => (string)GetValue(SearchTextProperty);
            set => SetValue(SearchTextProperty, value);
        }

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public int SearchDelay
        {
            get => (int)GetValue(SearchDelayProperty);
            set => SetValue(SearchDelayProperty, value);
        }

        public ICommand ClearCommand
        {
            get => (ICommand)GetValue(ClearCommandProperty);
            set => SetValue(ClearCommandProperty, value);
        }

        public ICommand SearchCommand
        {
            get => (ICommand)GetValue(SearchCommandProperty);
            set => SetValue(SearchCommandProperty, value);
        }

        // FIXED: Now uses the renamed RoutedEvent
        public event RoutedEventHandler SearchTextChanged
        {
            add { AddHandler(SearchTextChangedRoutedEvent, value); }
            remove { RemoveHandler(SearchTextChangedRoutedEvent, value); }
        }

        public SearchBox()
        {
            InitializeComponent();
            InitializeSearchTimer();
        }

        private void InitializeSearchTimer()
        {
            _searchTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(SearchDelay)
            };
            _searchTimer.Tick += OnSearchTimerTick;
        }

        private static void OnSearchTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is SearchBox searchBox)
            {
                searchBox.OnSearchTextChanged();
            }
        }

        private void OnSearchTextChanged()
        {
            // Restart the timer for delayed search
            _searchTimer.Stop();
            _searchTimer.Start();

            // FIXED: Use the renamed RoutedEvent
            RaiseEvent(new RoutedEventArgs(SearchTextChangedRoutedEvent));
        }

        private void OnSearchTimerTick(object sender, EventArgs e)
        {
            _searchTimer.Stop();
            
            // Execute search command
            if (SearchCommand?.CanExecute(SearchText) == true)
            {
                SearchCommand.Execute(SearchText);
            }
        }

        private void OnClearClick(object sender, RoutedEventArgs e)
        {
            SearchText = string.Empty;
            
            if (ClearCommand?.CanExecute(null) == true)
            {
                ClearCommand.Execute(null);
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                SearchText = string.Empty;
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                _searchTimer.Stop();
                if (SearchCommand?.CanExecute(SearchText) == true)
                {
                    SearchCommand.Execute(SearchText);
                }
                e.Handled = true;
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == SearchDelayProperty)
            {
                _searchTimer.Interval = TimeSpan.FromMilliseconds(SearchDelay);
            }
        }
    }
}