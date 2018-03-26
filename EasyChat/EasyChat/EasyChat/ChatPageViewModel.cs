using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Plugin.DeviceInfo;
using Xamarin.Forms;

namespace EasyChat
{
    public sealed class ChatPageViewModel : INotifyPropertyChanged
    {
        private readonly ClientWebSocket client;
        private readonly CancellationTokenSource cts;
        private readonly string username;
        private string messageText;


        private Command<string> sendMessageCommand;

        public ChatPageViewModel(string username)
        {
            client = new ClientWebSocket();
            cts = new CancellationTokenSource();
            Messages = new ObservableCollection<Message>();

            this.username = username;

            ConnectToServerAsync();
        }

        public bool IsConnected => client.State == WebSocketState.Open;

        public Command SendMessage => sendMessageCommand ??
                                      (sendMessageCommand = new Command<string>(SendMessageAsync, CanSendMessage));

        public ObservableCollection<Message> Messages { get; }

        public string MessageText
        {
            get => messageText;
            set
            {
                messageText = value;
                OnPropertyChanged();

                sendMessageCommand.ChangeCanExecute();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private async void ConnectToServerAsync()
        {
            try
            {
#if __IOS__
                await client.ConnectAsync(new Uri("ws://localhost:5000"), cts.Token);
#else
                await client.ConnectAsync(new Uri("ws://10.0.2.2:5000"), cts.Token);
#endif

                UpdateClientState();

                await Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        WebSocketReceiveResult result;
                        var message = new ArraySegment<byte>(new byte[4096]);
                        do
                        {
                            result = await client.ReceiveAsync(message, cts.Token);
                            var messageBytes = message.Skip(message.Offset).Take(result.Count).ToArray();
                            var serialisedMessae = Encoding.UTF8.GetString(messageBytes);

                            try
                            {
                                var msg = JsonConvert.DeserializeObject<Message>(serialisedMessae);
                                Messages.Add(msg);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Invalide message format. {ex.Message}");
                            }
                        } while (!result.EndOfMessage);
                    }
                }, cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                void UpdateClientState()
                {
                    OnPropertyChanged(nameof(IsConnected));
                    sendMessageCommand.ChangeCanExecute();
                    Console.WriteLine($"Websocket state {client.State}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR : {ex.Message}");
            }
        }

        private async void SendMessageAsync(string message)
        {
            try
            {
                var msg = new Message
                {
                    Name = username,
                    MessagDateTime = DateTime.Now,
                    Text = message,
                    UserId = CrossDeviceInfo.Current.Id
                };

                var serialisedMessage = JsonConvert.SerializeObject(msg);

                var byteMessage = Encoding.UTF8.GetBytes(serialisedMessage);
                var segmnet = new ArraySegment<byte>(byteMessage);

                await client.SendAsync(segmnet, WebSocketMessageType.Text, true, cts.Token);
                MessageText = string.Empty;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private bool CanSendMessage(string message)
        {
            return IsConnected && !string.IsNullOrEmpty(message);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}