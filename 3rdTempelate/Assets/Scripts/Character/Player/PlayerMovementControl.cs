using System;
using System.Collections;
using System.Collections.Generic;
using ARPG.Movement;
using Tools;
using UnityEngine;

namespace ARPG.Movement
{
    
    public class PlayerMovementControl : CharacterMovementControlBase
    {
        
        private float _rotationAngle;
        private float _angleVolocity = 0f;
        [SerializeField] private float _rotationSmoothTime;

        private Transform _mainCamera;

        public bool _isLock = false;
        //脚步声
        private float _nextStepTime;
        [SerializeField] private float _slowFootTime;
        [SerializeField] private float _fastFootTime;
        [SerializeField] private float _parryFootTime;
        
        [SerializeField , Header("是否开启转身跑") , Space(10)] private bool _canTurnAndRun;
        
        
        //目标朝向
        private Vector3 _characterTargetDirection;
        
        protected override void Awake()
        {
            base.Awake();
            _mainCamera = Camera.main.transform;
        }

        protected override void Update()
        {
            base.Update();
            OnLockUpdateXY();
        }

        private void LateUpdate()
        {

            CharacterRotationControl();


            UpdateAnimator();
        }


        private void CharacterRotationControl()
        {
            
            if(!_characterIsOnGround || _isLock) return;
            
            if (_animator.GetBool(AnimationID.HasInput))
            {
                _rotationAngle = 
                    Mathf.Atan2(GameInputManager.MainInstance.Movement.x , GameInputManager.MainInstance.Movement.y) * 
                    Mathf.Rad2Deg + _mainCamera.eulerAngles.y;
                
            }
            
            
            if (_animator.GetBool(AnimationID.HasInput) && _animator.AnimationAtTag("Motion"))
            {

                if(_canTurnAndRun)
                    _animator.SetFloat(AnimationID.DeltaAngle , DevelopmentTools.GetDeltaAngle(transform, _characterTargetDirection.normalized));
                if (_canTurnAndRun)
                {
                    if(_animator.GetFloat(AnimationID.DeltaAngle) < -135f && _animator.GetBool(AnimationID.Run)) return;
                    if(_animator.GetFloat(AnimationID.DeltaAngle) > 135f && _animator.GetBool(AnimationID.Run)) return;
                }
                
                transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, _rotationAngle,
                    ref _angleVolocity, _rotationSmoothTime);
            
                if (_canTurnAndRun)
                {
                    //得到我们要转到的目标方向
                    _characterTargetDirection = Quaternion.Euler(0, _rotationAngle, 0) * Vector3.forward;
                   
                }
                
               
            }
            // if(_canTurnAndRun)
            //     _animator.SetFloat(AnimationID.DeltaAngle , DevelopmentToos.GetDeltaAngle(transform, _characterTargetDirection.normalized));
            
        }
        
       
        

        private void UpdateAnimator()
        {
            if(!_characterIsOnGround) return;

            _animator.SetBool(AnimationID.HasInput, GameInputManager.MainInstance.Movement != Vector2.zero);
            
            if (_animator.GetBool(AnimationID.HasInput))
            {
                
                _animator.SetBool(AnimationID.Run , GameInputManager.MainInstance.Run);

                
                _animator.SetFloat(AnimationID.Movement , (_animator.GetBool(AnimationID.Run) ? 2f : GameInputManager.MainInstance.Movement.sqrMagnitude ), 0.25f, Time.deltaTime);
            }
            else
            {
                _animator.SetFloat(AnimationID.Movement , 0f, 0.25f, Time.deltaTime);
                if (_animator.GetFloat(AnimationID.Movement) < 0.2f)
                {
                    _animator.SetBool(AnimationID.Run , false);

                }
            }
        }

        private void OnLockUpdateXY()
        {
            if (_isLock)
            {
                _animator.SetFloat(AnimationID.Horizontal, GameInputManager.MainInstance.Movement.x , 0.25f, Time.deltaTime);
                _animator.SetFloat(AnimationID.Vertical, GameInputManager.MainInstance.Movement.y , 0.25f, Time.deltaTime);
            }
        }
        
    }

}