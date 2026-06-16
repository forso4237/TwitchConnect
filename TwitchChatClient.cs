using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BTD_Mod_Helper;

namespace TwitchConnect;

public static class TwitchChatClient
{
    private const string TwitchIrcUrl = "wss://irc-ws.chat.twitch.tv:443";

    private static ClientWebSocket? _ws;
    private static CancellationTokenSource? _cts;
    private static volatile bool _running;

    public static void Start()
    {
        if (_running) return;
        _running = true;
        Task.Run(MaintainConnectionLoop);
    }

    public static void Stop()
    {
        _running = false;
        _cts?.Cancel();
    }

    private static async Task MaintainConnectionLoop()
    {
        while (_running)
        {
            var channel = ((string)TwitchConnectMod.TwitchChannel)?.Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(channel))
            {
                await Task.Delay(2000);
                continue;
            }

            try
            {
                await ConnectAndListen(channel);
            }
            catch (Exception e)
            {
                ModHelper.Warning<TwitchConnectMod>($"Twitch chat error: {e.Message}");
            }

            if (_running) await Task.Delay(3000);
        }
    }

    private static async Task ConnectAndListen(string channel)
    {
        var cts = new CancellationTokenSource();
        var ws = new ClientWebSocket();
        _cts = cts;
        _ws = ws;

        await ws.ConnectAsync(new Uri(TwitchIrcUrl), cts.Token);

        await SendRaw("NICK justinfan" + new Random().Next(10000, 99999));
        await SendRaw("JOIN #" + channel);
        ModHelper.Msg<TwitchConnectMod>($"Connected to Twitch chat: #{channel}");

        var buffer = new byte[8192];
        var sb = new StringBuilder();

        while (_running && ws.State == WebSocketState.Open)
        {
            var current = ((string)TwitchConnectMod.TwitchChannel)?.Trim().ToLowerInvariant();
            if (current != channel) break;

            var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
            if (result.MessageType == WebSocketMessageType.Close) break;

            sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
            if (!result.EndOfMessage) continue;

            foreach (var raw in sb.ToString().Split('\n'))
                HandleLine(raw.TrimEnd('\r'));
            sb.Clear();
        }

        try { ws.Dispose(); } catch {  }
    }

    private static void HandleLine(string line)
    {
        if (line.Length == 0) return;

        if (line.StartsWith("PING"))
        {
            _ = SendRaw("PONG :tmi.twitch.tv");
            return;
        }

        if (!line.Contains("PRIVMSG") || line[0] != ':') return;

        var bang = line.IndexOf('!');
        if (bang < 2) return;
        var user = line.Substring(1, bang - 1);

        var textStart = line.IndexOf(" :", bang, StringComparison.Ordinal);
        if (textStart < 0) return;
        var text = line.Substring(textStart + 2);

        CommandRouter.Handle(user, text);
    }

    private static async Task SendRaw(string line)
    {
        var ws = _ws;
        var cts = _cts;
        if (ws == null || cts == null) return;

        var bytes = Encoding.UTF8.GetBytes(line + "\r\n");
        await ws.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token);
    }
}