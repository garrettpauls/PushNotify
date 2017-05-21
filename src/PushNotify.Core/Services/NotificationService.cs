using System;
using System.Reactive.Disposables;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

using Windows.Networking.Sockets;

using LanguageExt;

using MetroLog;

using PushNotify.Core.Models;

namespace PushNotify.Core.Services
{
    public interface INotificationService
    {
        Task Initialize();
    }

    public sealed class NotificationService : INotificationService, IDisposable
    {
        private readonly IConfigService mConfig;
        private readonly CompositeDisposable mDisposables = new CompositeDisposable();
        private readonly ILogger mLog;

        private Option<MessageWebSocket> mSocket = Option<MessageWebSocket>.None;

        public NotificationService(IConfigService config, ILogger log)
        {
            mConfig = config;
            mLog = log;
            mConfig.Authentication.Subscribe(_HandleAuthChanged).AddTo(mDisposables);
        }

        private Task<MessageWebSocket> _Disable(MessageWebSocket socket)
        {
            mLog.Trace("Closing existing web socket.");

            socket.MessageReceived -= _HandleMessageReceived;
            socket.Close(0, "");
            socket.Dispose();

            return null;
        }

        private async Task<MessageWebSocket> _Enable(PushoverAuth auth)
        {
            mLog.Trace($"Enabling web socket for {auth.DeviceId}");

            var socket = new MessageWebSocket();
            socket.Control.MessageType = SocketMessageType.Utf8;
            socket.MessageReceived += _HandleMessageReceived;

            try
            {
                await socket.ConnectAsync(new Uri("wss://client.pushover.net/push"));
                mLog.Trace("Connected to web socket.");

                await _Login(socket, auth);
                mLog.Trace("Logged in to web socket.");
            }
            catch(Exception ex)
            {
                mLog.Error("Failed to connect and log in to web socket", ex);
                throw;
            }

            await Task.Delay(0);

            return socket;
        }

        private Task _FailedConnection()
        {
            mLog.Fatal("Connection failed, invalidating login");
            mConfig.SetAuthentication(Option<PushoverAuth>.None);

            return Task.CompletedTask;
        }

        private async void _HandleAuthChanged(Option<PushoverAuth> optAuth)
        {
            var result = await optAuth.MatchAsync(
                async auth =>
                {
                    return await mSocket.MatchAsync(Task.FromResult, () => _Enable(auth));
                },
                async () =>
                {
                    return await mSocket.MatchAsync(_Disable, () => Task.FromResult((MessageWebSocket) null));
                });

            mSocket = result;
        }

        private async void _HandleMessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
        {
            const char KEEP_ALIVE = '#';
            const char NEW_MESSAGE = '!';
            const char RELOAD = 'R';
            const char ERROR = 'E';

            try
            {
                using(var reader = args.GetDataReader())
                {
                    var result = reader.ReadString(reader.UnconsumedBufferLength);
                    mLog.Trace($"Received: type={args.MessageType}; data={result}");

                    var msg = result.Length > 0 ? result[0] : '#';

                    switch(msg)
                    {
                        case NEW_MESSAGE:
                            mLog.Trace("New message received");
                            break;
                        case KEEP_ALIVE:
                            mLog.Trace("Keep alive request");
                            break;
                        case RELOAD:
                            await _ReloadConnection();
                            break;
                        case ERROR:
                            await _FailedConnection();
                            break;
                        default:
                            mLog.Warn($"Unknown message type: {msg}");
                            break;
                    }
                }
            }
            catch(Exception ex)
            {
                mLog.Fatal("Failed to read from web socket", ex);
                throw;
            }

            mLog.Trace($"Received: type={args.MessageType}");
        }

        private async Task _Login(IWebSocket socket, PushoverAuth auth)
        {
            var login = $"login:{auth.DeviceId}:{auth.Secret}\n";
            var buffer = Encoding.UTF8.GetBytes(login).AsBuffer();

            await socket.OutputStream.WriteAsync(buffer);
            await socket.OutputStream.FlushAsync();
        }

        private async Task _ReloadConnection()
        {
            mLog.Info("Reloading connection");

            await mSocket.IfSomeAsync(new Func<MessageWebSocket, Task>(_Disable));
            if(mConfig.TryGetAuthentication(out PushoverAuth auth))
            {
                mSocket = await _Enable(auth);
            }
        }

        public async void Dispose()
        {
            mDisposables?.Dispose();

            await mSocket.IfSomeAsync(new Func<MessageWebSocket, Task>(_Disable));
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }
    }
}
