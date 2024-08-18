using Ghost.AdvancedPlayerController;

namespace Ghost.StateMachine
{
    public class GroundedState : IState
    {
        private readonly PlayerAdvancedController _controller;

        public GroundedState(PlayerAdvancedController controller)
        {
            _controller = controller;
        }

        public void OnEnter()
        {
            _controller.OnGroundContactRegained();
        }
    }

    public class FallingState : IState
    {
        private readonly PlayerAdvancedController _controller;

        public FallingState(PlayerAdvancedController controller)
        {
            _controller = controller;
        }

        public void OnEnter()
        {
            _controller.OnFallStart();
        }
    }

    public class SlidingState : IState
    {
        private readonly PlayerAdvancedController _controller;

        public SlidingState(PlayerAdvancedController controller)
        {
            _controller = controller;
        }

        public void OnEnter()
        {
            _controller.OnGroundContactLost();
        }
    }

    public class RisingState : IState
    {
        private readonly PlayerAdvancedController _controller;

        public RisingState(PlayerAdvancedController controller)
        {
            _controller = controller;
        }

        public void OnEnter()
        {
            _controller.OnGroundContactLost();
        }
    }

    public class JumpingState : IState
    {
        private readonly PlayerAdvancedController _controller;

        public JumpingState(PlayerAdvancedController controller)
        {
            _controller = controller;
        }

        public void OnEnter()
        {
            _controller.OnGroundContactLost();
            _controller.OnJumpStart();
        }
    }
}