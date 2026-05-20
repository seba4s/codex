using UnityEngine;

namespace CODEX.Systems
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private Vector2 offset = new Vector2(2f, 1f);
        [SerializeField] private Vector2 minBounds = new Vector2(-100f, -10f);
        [SerializeField] private Vector2 maxBounds = new Vector2(200f, 10f);

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = new Vector3(
                target.position.x + offset.x,
                target.position.y + offset.y,
                transform.position.z
            );

            desired.x = Mathf.Clamp(desired.x, minBounds.x, maxBounds.x);
            desired.y = Mathf.Clamp(desired.y, minBounds.y, maxBounds.y);

            transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        }

        public void SetTarget(Transform t) => target = t;

        public void SnapToTarget()
        {
            if (target == null) return;
            Vector3 pos = new Vector3(
                Mathf.Clamp(target.position.x + offset.x, minBounds.x, maxBounds.x),
                Mathf.Clamp(target.position.y + offset.y, minBounds.y, maxBounds.y),
                transform.position.z
            );
            transform.position = pos;
        }
    }
}
