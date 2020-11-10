﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;
using SignalRClientWPF.Dto;

using System.Configuration;

namespace SignalRClientWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public System.Threading.Thread Thread { get; set; }

        public string Host = ConfigurationManager.AppSettings["ServerUrl"];

        public string UserName = ConfigurationManager.AppSettings["UserName"];

        public IHubProxy Proxy { get; set; }

        public HubConnection Connection { get; set; }

        public bool Active { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            ni.Icon = new System.Drawing.Icon("image.ico");
            ni.Visible = true;
            ni.DoubleClick += delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }


        private async void ActionHeartbeatButtonClick(object sender, RoutedEventArgs e)
        {
            await SendHeartbeat();
        }

        private async void ActionSendButtonClick(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void ActionSendObjectButtonClick(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async Task SendMessage()
        {
            await Proxy.Invoke("Addmessage", UserName, MessageTextBox.Text);
        }

        private async Task SendHeartbeat()
        {
            await Proxy.Invoke("Heartbeat");
        }


        private async void ActionWindowLoaded(object sender, RoutedEventArgs e)
        {
            Active = true;
            Thread = new System.Threading.Thread(() =>
            {
                Connection = new HubConnection(Host);
                Proxy = Connection.CreateHubProxy("MyHub");

                Proxy.On<string, string>("addmessage", (name, message) => OnSendData($"{name}:{message}"));
                Proxy.On("heartbeat", () => OnSendData("Recieved heartbeat"));

                Connection.Start();

                while (Active)
                {
                    System.Threading.Thread.Sleep(10);
                }
            })
            { IsBackground = true };
            Thread.Start();

        }

        private void OnSendData(string message)
        {
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() => MessagesListBox.Items.Insert(0, message)));
            Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                                                                         {
                                                                             this.Show();
                                                                             if (this.WindowState
                                                                                 == WindowState.Minimized)
                                                                                 this.WindowState =
                                                                                     WindowState.Normal;
                                                                         }));
        }

        private async void ActionMessageTextBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
            {
                await SendMessage();
                MessageTextBox.Text = "";
            }
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
