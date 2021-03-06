﻿using System;
using System.Linq;
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
    public interface INotificationListenerService
    {
        Task Initialize();
    }

    public sealed class NotificationListenerService : INotificationListenerService, IDisposable
    {
        private readonly IConfigService mConfigService;
        private readonly CompositeDisposable mDisposables = new CompositeDisposable();
        private readonly ILogger mLog;
        private readonly IMessageService mMessageService;
        private readonly INotificationService mNotifier;
        private Config mConfig = new Config();
        private Option<MessageWebSocket> mSocket = Option<MessageWebSocket>.None;

        public NotificationListenerService(IConfigService configService, INotificationService notifier, IMessageService messageService, ILogger log)
        {
            mConfigService = configService;
            mNotifier = notifier;
            mMessageService = messageService;
            mLog = log;

            mConfigService.Authentication.Subscribe(_HandleAuthChanged).AddTo(mDisposables);
            mConfigService.Config.Subscribe(_HandleConfigChanged).AddTo(mDisposables);
        }

        private void _Disable(MessageWebSocket socket)
        {
            if(socket == null)
            {
                return;
            }

            mLog.Trace("Closing existing web socket.");

            socket.MessageReceived -= _HandleMessageReceived;
            socket.Close(0, "");
            socket.Dispose();
        }

        private Task<MessageWebSocket> _DisableAsync(MessageWebSocket socket)
        {
            _Disable(socket);

            return Task.FromResult((MessageWebSocket) null);
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
            mConfigService.SetAuthentication(Option<PushoverAuth>.None);

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
                    return await mSocket.MatchAsync(_DisableAsync, () => Task.FromResult((MessageWebSocket) null));
                });

            mSocket = result;
        }

        private void _HandleConfigChanged(Config config)
        {
            mConfig = config;
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
                            await _RetrieveNewMessages();
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

            mSocket.IfSome(new Action<MessageWebSocket>(_Disable));
            if(mConfigService.TryGetAuthentication(out PushoverAuth auth))
            {
                mSocket = await _Enable(auth);
            }
        }

        private async Task _RetrieveNewMessages()
        {
            var messages = await mMessageService.FetchNewMessages();
            var now = DateTimeOffset.Now;
            var currentMessages = messages
                .Where(msg => now < msg.Date || now.Subtract(msg.Date) <= mConfig.NotificationAgeThreshold);
            foreach(var message in messages)
            {
                mNotifier.Show(message);
            }
        }

        public void Dispose()
        {
            mDisposables?.Dispose();

            mSocket.IfSome(new Action<MessageWebSocket>(_Disable));
        }

        public Task Initialize()
        {
            return Task.CompletedTask;
        }
    }
}
