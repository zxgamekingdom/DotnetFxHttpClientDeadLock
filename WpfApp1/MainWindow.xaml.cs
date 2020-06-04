using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace dotnetframeworkWPF
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Title = nameof(dotnetframeworkWPF);
        }

        private readonly HttpClient _httpClient = new HttpClient();

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button button)) throw new ArgumentException(nameof(sender));
            button.IsEnabled = false;
            var cancellationTokenSource = 显示计时();
            // ReSharper disable MethodSupportsCancellation
            await Task.Run(async () =>
                    // ReSharper restore MethodSupportsCancellation
                {
                    var tasks = Enumerable.Range(2, 4)
                        .Select(i =>
                            _httpClient.GetStreamAsync(
                                $"https://www.3dmgame.com/bagua/3088_{i}.html"));
                    await Task.WhenAll(tasks);
                    await Invoke(() =>
                    {
                        int i = default;
                        foreach (Stream stream in tasks.Select(task => task.Result))
                        {
                            ListBox.Items.Add(i++);
                        }
                    });
                })
                // ReSharper disable MethodSupportsCancellation
                .ContinueWith(task =>
                    // ReSharper restore MethodSupportsCancellation
                {
                    if (task.Exception != null) throw task.Exception;
                });
            button.IsEnabled = true;
            cancellationTokenSource.Cancel();
        }

        [SuppressMessage("ReSharper", "MethodSupportsCancellation")]
        private CancellationTokenSource 显示计时()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            Task.Run(async () =>
                {
                    TimeSpan timeSpan = TimeSpan.Zero;
                    var stopwatch = new Stopwatch();
                    while (cancellationTokenSource.IsCancellationRequested == false)
                    {
                        stopwatch.Reset();
                        stopwatch.Start();
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        stopwatch.Stop();
                        timeSpan = timeSpan.Add(stopwatch.Elapsed);
                        TimeSpan span = timeSpan;
                        _ = Invoke(() =>
                        {
                            Label.Content = span;
                        });
                    }
                })
                .ContinueWith(task =>
                {
                    if (task.Exception != null) throw task.Exception;
                });
            return cancellationTokenSource;
        }

        async Task Invoke(Action action)
        {
            await Dispatcher.InvokeAsync(action);
        }
    }
}