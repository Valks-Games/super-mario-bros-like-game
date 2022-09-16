namespace Sankari.Netcode.Client;

using Event = ENet.Event;

public partial class GameClient : ENetClient
{
    /// <summary>
    /// This property is not thread safe
    /// </summary>
    public bool TryingToConnect { get; set; }

    /// <summary>
    /// This property is not thread safe
    /// </summary>
    public byte PeerId { get; set; }

    public GameClient(Net networkManager, GodotCommands godotCmds) : base(networkManager)
    {
        base.godotCmds = godotCmds;
    }

    protected override void Connecting()
    {
        Log("Client connecting...");
    }

    protected override void ClientCmds(ENet.Peer peer)
    {
        while (enetCmds.TryDequeue(out ENetClientCmd cmd))
        {
            switch (cmd.Opcode)
            {
                case ENetClientOpcode.Disconnect:
                    if (cancellationTokenSource.IsCancellationRequested)
                    {
                        Logger.LogWarning("Client is in the middle of stopping");
                        break;
                    }

                    cancellationTokenSource.Cancel();
                    peer.Disconnect(0);
                    break;

                case ENetClientOpcode.ExecuteCode:
                    var action = (Action<GameClient>)cmd.Data;
                    action(this);
                    break;
            }
        }
    }

    protected override void Receive(PacketReader reader)
    {
        godotCmds.Enqueue(GodotOpcode.ENetPacket, new PacketInfo(reader, this));
    }

    protected override void Connect(ref Event netEvent)
    {
        godotCmds.Enqueue(GodotOpcode.NetEvent, Sankari.Event.OnGameClientConnected);
        Log("Client connected");
    }

    protected override void Leave(ref Event netEvent)
    {
        Log("Client left");
    }

    protected override void Stopped()
    {
        godotCmds.Enqueue(GodotOpcode.NetEvent, Sankari.Event.OnGameClientStopped);
        Log("Client stopped");
    }

    private void Log(object v) => Logger.Log($"[Client] {v}", ConsoleColor.DarkGreen);
}
