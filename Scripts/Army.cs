using System;
using System.Collections.Generic;
using Godot;

public class Army : Node2D
{
    private List<Unit> Units = new List<Unit>();

    private Label TotalCountLabel;

    [Export]
    private Boolean IsCPU = false;

    private int TotalPower = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        TotalCountLabel = GetNode<Label>("TotalCountLabel");
    }

    public void AddUnits(List<Unit> units)
    {
        foreach (Unit unit in Units.ToArray())
        {
            RemoveChild(unit);
        }
        Units.Clear();
        foreach (Unit unit in units.ToArray())
        {
            AddChild(unit);
            unit.SetDefault();
        }
        UpdateUnitPositions();
    }

    public void Randomize()
    {
        foreach (Unit unit in Units.ToArray())
        {
            RemoveChild(unit);
        }
        Units.Clear();
        Random random = new Random();
        var unitScene = GD.Load<PackedScene>("res://Scenes/Unit.tscn");
        for (int i = 0; i < random.Next(3, 6); i++)
        {
            var unitInstance = unitScene.Instance();
            ((Unit)unitInstance).Power = random.Next(1, 4);
            AddChild(unitInstance);
        }
        UpdateUnitPositions();
    }

    private void OnUnitSelected(Unit unit)
    {
        UpdateUnitPositions();
    }

    public void SetRandomSelection()
    {
        Random random = new Random();
        if (Units.Count > 0)
        {
            List<Unit> nonRestingUnits = new List<Unit>();
            foreach (Unit unit in Units.ToArray())
            {
                if (!unit.IsResting())
                {
                    nonRestingUnits.Add(unit);
                }
            }

            if (nonRestingUnits.Count == 0)
            {
                return;
            }

            int idx1 = random.Next(0, nonRestingUnits.Count);
            if (!nonRestingUnits[idx1].IsResting())
            {
                nonRestingUnits[idx1].SetSelected();
            }
            if (random.Next(0, 2) == 0)
            {
                int idx2 = random.Next(0, nonRestingUnits.Count);
                if (!nonRestingUnits[idx2].IsResting())
                {
                    nonRestingUnits[idx2].SetSelected();
                }
            }
            if (random.Next(0, 2) == 0)
            {
                int idx3 = random.Next(0, nonRestingUnits.Count);
                if (!nonRestingUnits[idx3].IsResting())
                {
                    nonRestingUnits[idx3].SetSelected();
                }
            }
        }
    }

    public List<Unit> GetSelectedUnits()
    {
        List<Unit> selectedUnits = new List<Unit>();
        foreach (Unit unit in Units.ToArray())
        {
            if (unit.IsSelected() || unit.IsNavigating())
            {
                selectedUnits.Add(unit);
            }
        }
        return selectedUnits;
    }

    public List<Unit> GetUnits()
    {
        return Units;
    }

    public void RemoveUnits()
    {
        foreach (Unit unit in Units.ToArray())
        {
            RemoveChild(unit);
        }
        Units.Clear();
    }

    public void AddRestingUnits(List<Unit> units)
    {
        foreach (Unit unit in units)
        {
            AddChild(unit);
            unit.SetResting();
        }
    }

    public void RemoveSelectedUnits()
    {
        foreach (Unit unit in Units.ToArray())
        {
            if (unit.IsSelected() || unit.IsNavigating())
            {
                RemoveChild(unit);
            }
        }
    }

    public int GetTotalPower()
    {
        return TotalPower;
    }

    public int GetArmyPower()
    {
        int power = 0;
        foreach (Unit unit in Units.ToArray())
        {
            power += unit.Power;
        }
        return power;
    }

    private void UpdateChildrenUnits()
    {
        Units.Clear();
        foreach (Node node in GetChildren())
        {
            if (node is Unit)
            {
                Units.Add((Unit)node);
            }
        }
    }

    public void UpdateUnitPositions()
    {
        UpdateChildrenUnits();
        int totalPowerCount = 0;
        int xSpacing = 66;
        int xOffset = xSpacing * (Units.Count - 1) / 2;
        for (int unitIdx = 0; unitIdx < Units.Count; unitIdx++)
        {
            Unit unit = Units[unitIdx];
            unit.Position = new Vector2(xSpacing * unitIdx - xOffset, 0);
            unit.SetIsCPU(IsCPU);
            if (!unit.IsConnected("UnitSelected", this, nameof(OnUnitSelected)))
            {
                unit.Connect("UnitSelected", this, nameof(OnUnitSelected));
            }
            if (unit.IsSelected())
            {
                totalPowerCount += unit.Power;
            }
        }
        TotalPower = totalPowerCount;
        TotalCountLabel.Text = totalPowerCount.ToString();
        TotalCountLabel.RectPosition = new Vector2(-2, -20 + 50 * (IsCPU ? 1 : -1));
        //TotalCountLabel.RectSize = new Vector2(200, 200);
    }

    public void WakeUnits()
    {
        foreach (Unit unit in Units.ToArray())
        {
            if (unit.IsResting())
            {
                unit.SetDefault();
            }
        }
    }

    public void DeselectAll()
    {
        foreach (Unit unit in Units.ToArray())
        {
            if (unit.IsSelected() || unit.IsNavigating())
            {
                unit.SetResting();
            }
        }
    }

    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
