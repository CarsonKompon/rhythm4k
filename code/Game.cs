using System;
using Sandbox;
using Sandbox.UI;

namespace Rhythm4K;

public partial class Rhythm4KGame : GameManager
{
    public Rhythm4KGame()
    {
        if(Game.IsClient)
        {
            // Init HUD
            Game.RootPanel?.Delete(true);
            Game.RootPanel = new GameHud();
        }
    }

    public override void ClientJoined(IClient client)
    {
        base.ClientJoined(client);

        // Create a pawn
        var pawn = new RhythmPlayer();
        client.Pawn = pawn;
    }
}
