using System;
using Godot;

public class Army : Node2D
{
    private Godot.Collections.Array Units;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Units = GetChildren();
        int xSpacing = 66;
        int xOffset = xSpacing * (Units.Count - 1) / 2;
        for (int unitIdx = 0; unitIdx < Units.Count; unitIdx++)
        {
            Node2D unit = (Node2D)Units[unitIdx];
            unit.Position = new Vector2(xSpacing * unitIdx - xOffset, 0);
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
