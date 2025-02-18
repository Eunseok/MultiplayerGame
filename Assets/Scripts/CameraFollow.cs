using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 따라다닐 대상 (로컬 플레이어)
    public Vector3 offset = new Vector3(0f, 0f, -10f); // 기본 카메라 오프셋
    public float lerpTime = 10f; 

    
    private Vector3 velocity = Vector3.zero;

    //캐릭터라 rigidbody2d로 움직여서 fixedUpdate에서 팔로우
    private void FixedUpdate()
    {
        if (target == null) return;

        // 목표 위치 계산
        Vector3 targetPosition = target.position + offset;

        
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * lerpTime);

    }
    public void SetTarget(Transform newTarget)
    {
        // 카메라가 따라다닐 대상을 설정
        target = newTarget;
    }
}