using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Base
{
    [RequireComponent(typeof(CharacterController))]
    public abstract class CharacterMovementControlBase : MonoBehaviour
    {
        protected CharacterController Control ;
        protected Animator Animator;
        
        protected Vector3 MoveDirection; //角色的移动方向
        
        [Header("地面检测")]
        protected bool CharacterIsOnGround; //角色是否在地面
        [FormerlySerializedAs("_groundDetectionPositionOffset")] [SerializeField,Header("地面检测")]
        protected float groundDetectionPositionOffset;   //检测高度偏移量
        [FormerlySerializedAs("_detectionRang")] [SerializeField]
        protected float detectionRang;    //检测范围
        [FormerlySerializedAs("whatisGround")] [SerializeField]
        protected LayerMask whatIsGround;   //哪些层级是地面
        
        [Header("重力")]
        protected readonly float CharacterGravity = -9.8f;
        protected float CharacterVerticalVelocity; //用来更新角色的Y轴速度 , 可以应用于重力和跳跃高度 砍一刀 往天上飞一下等 
        protected float FallOutDeltaTime; 
        protected readonly float FallOutTime = 0.15f; //防止角色下楼梯的时候鬼畜 比如播放一下跌落动画
        protected readonly float CharacterVerticalMaxVelocity = 54f;  //角色在低于这个值的时候 才需要应用重力
        protected Vector3 CharacterVerticalDirection; //角色的Y轴方向 因为我们是通过CC的Move函数 来实现重力 所以把velocity 应用到这个向量的Y值里去更新
        private bool _isEnableGravity; //是否启用重力
        
        
        
        protected virtual void Awake()
        {
            Control = GetComponent<CharacterController>();
            Animator = GetComponent<Animator>();
            _isEnableGravity = true;
        }
        
        protected virtual void Start()
        {
            FallOutDeltaTime = FallOutTime;
        }

        protected virtual void Update()
        {
            SetCharacterGravity();
            UpdateCharacterGravity();
        }

        protected virtual void OnAnimatorMove()
        {
            Animator.ApplyBuiltinRootMotion();
            UpdateCharacterMoveDirection(Animator.deltaPosition);
        }

        //地面检测
        private bool GroundDetection()
        {
            var detectionPosition = new Vector3(transform.position.x , transform.position.y - groundDetectionPositionOffset,
                transform.position.z); //检测的中心点
            return Physics.CheckSphere(detectionPosition, detectionRang, whatIsGround , QueryTriggerInteraction.Ignore);
        }

        private void SetCharacterGravity()
        {
            CharacterIsOnGround = GroundDetection();

            if (CharacterIsOnGround)
            {
                /*
                1.如果角色在地面上 需要重置FallOutTime
                2.重置角色的垂直速度
                */
                FallOutDeltaTime = FallOutTime;
                if (CharacterVerticalVelocity < 0f)
                {
                    CharacterVerticalVelocity = -2f;
                    //如果这里不固定死 那么他会一直累计
                    //那么在第二次跳跃或者高处跌落 角色的下落速度会很快 不会从慢到快
                    //固定-2 那么第二次高处掉落 垂直速度是从-2开始的
                    //非固定 在地面还一直累计 -500 这时候 第二次高处跌落 垂直速度就是从-500开始计算
                }
            }
            else
            {
                //不在地面
                if (FallOutDeltaTime > 0)
                {
                    FallOutDeltaTime -= Time.deltaTime;
                    //等待0.15秒 这个0.15秒是用来帮助角色从较低的高度差下落
                    //比如走楼梯 在0.15秒内角色就会下落到地面 那么这样根本不用播放跌落动画
                }
                else
                {
                    //说明角色还没有落地 那么不可能在下楼梯
                    //那么就有必要播放跌落动画
                    //0.15可以根据需求更改
                }

                if (CharacterVerticalVelocity < CharacterVerticalMaxVelocity && _isEnableGravity)
                {
                    CharacterVerticalVelocity += CharacterGravity * Time.deltaTime;
                }
            }
        }


        private void UpdateCharacterGravity()
        {
            if (_isEnableGravity)
            {
                CharacterVerticalDirection.Set(0, CharacterVerticalVelocity , 0f);  //x z 是角色的移动去负责的 重力不关心
               
                Control.Move(CharacterVerticalDirection * Time.deltaTime);

            }
        }
        
        
        //坡道检测
        private Vector3 StopResetDirection(Vector3 moveDirection)
        {
            //检测角色现在是否在坡上移动 防止角色在下坡速度过快时 导致变成弹力球
            if (Physics.Raycast(transform.position + (transform.up * .5f), Vector3.down, out var hit,
                    Control.height * .85f, whatIsGround, QueryTriggerInteraction.Ignore))
            {
                if (Vector3.Dot(hit.normal, Vector3.up) != 0)  //不垂直
                {
                    return moveDirection = Vector3.ProjectOnPlane(moveDirection, hit.normal);
                }
            }
            return moveDirection;
        }
        
        
        protected void UpdateCharacterMoveDirection(Vector3 direction)
        {
            MoveDirection = StopResetDirection(direction);
            Control.Move(MoveDirection * Time.deltaTime);
        }

        private void OnDrawGizmos()
        {
            var detectionPosition = new Vector3(transform.position.x , transform.position.y - groundDetectionPositionOffset,
                transform.position.z); //检测的中心点
            Gizmos.DrawSphere(detectionPosition , detectionRang);
        }
        
        
        //事件 注册
        private void EnableCharacterGravity(bool enable)
        {
            _isEnableGravity = enable;
            CharacterVerticalVelocity = (enable) ? -2f : 0f;
        }
        
        
    }
    
    
}
