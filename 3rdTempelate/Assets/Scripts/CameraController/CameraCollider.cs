using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace CameraController
{
    public class CameraCollider : MonoBehaviour
    {
        //最小值和最大值 偏移量
        //Layer 障碍物层级
    
        [FormerlySerializedAs("_maxDistanceOffset")] [SerializeField , Header("最大最小偏移量")] private Vector2 maxDistanceOffset;
        [FormerlySerializedAs("_whatIsWall")] [SerializeField , Header("障碍物层级") , Space(10)] private LayerMask whatIsWall;
        [FormerlySerializedAs("_rayLength")] [SerializeField , Header("射线长度")] private float rayLength;
        [FormerlySerializedAs("_smoothTime")] [SerializeField , Header("平滑时间")] private float smoothTime;
    
    
        //开始的时候 保存一下起始点和起始的偏移量
        private Vector3 _originPosition;
        private float _originOffsetDistance;
        private Transform _cameraTransform;


        private void Awake()
        {
            _cameraTransform = Camera.main.transform;
        }

        private void Start()
        {
            _originPosition = transform.position.normalized;
            _originOffsetDistance = maxDistanceOffset.y;
        }

        private void Update()
        {
            UpdateCameraCollider();
        }

        private void UpdateCameraCollider()
        {
            var detectionDirection = transform.TransformPoint(_originPosition * rayLength);
            if (Physics.Linecast(transform.position, detectionDirection, out var hit, whatIsWall,
                    QueryTriggerInteraction.Ignore))
            {
                //如果打到东西了
                _originOffsetDistance = Mathf.Clamp(hit.distance * 0.8f , maxDistanceOffset.x, maxDistanceOffset.y);
            }
            else
            {
                //没打到 默认最大值
                _originOffsetDistance = maxDistanceOffset.y;
        
            }
            //更新相机位置
            _cameraTransform.localPosition = Vector3.Lerp(_cameraTransform.localPosition,
                _originPosition * (_originOffsetDistance - 0.1f), DevelopmentTools.UnTetheredLerp(smoothTime));

        }
    
    
    
    
    
   
    
    }
}
