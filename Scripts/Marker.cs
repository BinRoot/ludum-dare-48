using System;
using Godot;

public class Marker : Node2D
{
    private Timer Timer;
    private Tween Tween;
    public override void _Ready()
    {
        Timer = GetNode<Timer>("Timer");
        Tween = GetNode<Tween>("Tween");
        Timer.Connect("timeout", this, nameof(OnTimerTimeout));
        Tween.InterpolateProperty(
            this, "modulate",
            new Color(1.0f, 1.0f, 1.0f, 1.0f),
            new Color(1.0f, 1.0f, 1.0f, 0.0f),
            0.5f, Tween.TransitionType.Linear,
            Tween.EaseType.In
        );
        Tween.Start();
    }

    public void OnTimerTimeout()
    {
        QueueFree();
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
