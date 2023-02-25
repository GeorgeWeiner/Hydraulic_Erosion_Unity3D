using Player;
using Simulation;

public class PlayerBuoyancyManager : BuoyantObject
{
    private PlayerController _controller;
    private float normalPlayerGravity;
    
    protected override void Awake()
    {
        base.Awake();
        _controller = GetComponentInParent<PlayerController>();
        normalPlayerGravity = _controller.GravityAcceleration;
    }

    public override void EnableBuoyancy()
    {
        base.EnableBuoyancy();
        _controller.GravityAcceleration = 0f;
        _controller.GravityStrength = 10f;
        _controller.IsSwimming = true;
    }

    public override void DisableBuoyancy()
    {
        base.DisableBuoyancy();
        _controller.GravityAcceleration = normalPlayerGravity;
        _controller.IsSwimming = false;
    }
}
