
/*
using UnityUtils.StateMachine;

public class PlayerStates
{
    public class SwimmingState : IState
    {
        private readonly PlayerMoveControllerRB _playerMoveController;
        public SwimmingState(PlayerMoveControllerRB controller)
        {
            _playerMoveController = controller;
        }

        public void OnEnter()
        {
            _playerMoveController.OnStartSwim();
        }

        public void OnExit()
        {
            _playerMoveController.OnEndSwim();
        }
    }
    
    public class NormalState : IState
    {
        private readonly PlayerMoveControllerRB _playerMoveController;
        public NormalState(PlayerMoveControllerRB controller)
        {
            _playerMoveController = controller;
        }

        public void OnEnter()
        {
            _playerMoveController.OnNormal();
        }
        
    }
    
    public class FallingState : IState
    {
        private readonly PlayerMoveControllerRB _playerMoveController;
        public FallingState(PlayerMoveControllerRB controller)
        {
            _playerMoveController = controller;
        }

        public void OnEnter()
        {
            _playerMoveController.OnFalling();
        }
        
    }
    
    public class RisingState : IState
    {
        private readonly PlayerMoveControllerRB _playerMoveController;
        public RisingState(PlayerMoveControllerRB controller)
        {
            _playerMoveController = controller;
        }

        public void OnEnter()
        {
            _playerMoveController.OnRising();
        }
        
    }
    
    public class JumpingState : IState
    {
        private readonly PlayerMoveControllerRB _playerMoveController;
        public JumpingState(PlayerMoveControllerRB controller)
        {
            _playerMoveController = controller;
        }

        public void OnEnter()
        {
            _playerMoveController.OnGroundContactLost();
            _playerMoveController.OnJumpStart();
        }
    }
    
    public class SlidingState : IState {
        readonly PlayerMoveControllerRB _playerMoveController;

        public SlidingState(PlayerMoveControllerRB controller) {
            _playerMoveController = controller;
        }

        public void OnEnter() {
            _playerMoveController.OnGroundContactLost();
        }
    }
}
*/
