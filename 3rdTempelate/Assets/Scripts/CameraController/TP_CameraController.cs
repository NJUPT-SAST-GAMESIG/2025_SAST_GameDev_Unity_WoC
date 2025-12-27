using Input;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace CameraController
{
    public class TP_CameraController : MonoBehaviour
    {
        //相机的移动速度
        [FormerlySerializedAs("_controlSpeed")] [SerializeField, Header("相机参数配置")] private float controlSpeed;  //摄像机移动速度
        [FormerlySerializedAs("_cameraVerticalMaxAngle")] [SerializeField, Header("最大俯仰角")] private float cameraVerticalMaxAngle;  //限制相机上下最大旋转角度
        [FormerlySerializedAs("_cameraVerticalMinAngle")] [SerializeField, Header("最小俯仰角")] private float cameraVerticalMinAngle;  //限制相机上下最小转角度
        [FormerlySerializedAs("_smoothRotationTime")] [SerializeField, Header("相机旋转平滑时间")] private float smoothRotationTime;   //摄像机平滑速度
        [FormerlySerializedAs("_positionOffset")] [SerializeField, Header("TP_Camera偏移值")] private float positionOffset;   //摄像机与目标物体的距离偏移
        [FormerlySerializedAs("_positionSmoothTime")] [SerializeField, Header("相机位置平滑插值系数")] private float positionSmoothTime;    //摄像机位置平滑时间


        [FormerlySerializedAs("_lookTarget")] [SerializeField , Header("跟随目标")]
        public Transform lookTarget;
        private Vector3 _smoothDampVelocity = Vector3.zero;
        private Vector2 _input;    //相机的输入 旋转角度
        private Vector3 _cameraRotation;   //当前摄像机的旋转角度

        private void Update()
        {
            if(lookTarget == null)
                return;
            CameraInput();
        }

        private void LateUpdate()
        {
            if(lookTarget == null)
                return;
            UpdateCameraRotation();
            CameraPosition();
        }

        private void CameraInput()
        {

            _input.y += GameInputManager.MainInstance.CameraLook.x * controlSpeed;
            _input.x -= GameInputManager.MainInstance.CameraLook.y * controlSpeed;

            _input.x = Mathf.Clamp(_input.x, cameraVerticalMinAngle, cameraVerticalMaxAngle);
        }

        //更新相机的旋转
        private void UpdateCameraRotation()
        {
            _cameraRotation = Vector3.SmoothDamp(_cameraRotation , new Vector3(_input.x, _input.y, 0), ref _smoothDampVelocity, smoothRotationTime);
            transform.eulerAngles = _cameraRotation;
        }

        private void CameraPosition()
        {
            var newPos = (lookTarget.position + (-transform.forward * positionOffset));
            transform.position = Vector3.Lerp(transform.position , newPos , DevelopmentTools.UnTetheredLerp(positionSmoothTime));
        }
    
    
    
    }
}
