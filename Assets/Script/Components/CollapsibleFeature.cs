using System;
using System.Collections;
using MonkeyPlayground.Data;
using UnityEngine;

namespace MonkeyPlayground.Components
{ 
    [RequireComponent(typeof(Collider2D))] 
    public class CollapsibleFeature : MonoBehaviour
    {
        [SerializeField] private float squashFactor = 0.5f;
        [SerializeField] private float squashDuration = 0.1f;
        [SerializeField] private float restoreDuration = 0.2f;

        [SerializeField] private float restoreDelay = 0.5f;

        private Vector3 _originalScale;
        private Coroutine _animationCoroutine;
        private Coroutine _restoreDelayCoroutine;

        private void Start()
        {
            _originalScale = transform.localScale;
        }

        /// <summary>
        /// Public method: Trigger the squash
        /// </summary>
        public void TriggerSquash()
        {
            if (_restoreDelayCoroutine != null)
            {
                StopCoroutine(_restoreDelayCoroutine);
                _restoreDelayCoroutine = null;
            }

            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(ChangeScale(_originalScale * new Vector2(1, squashFactor)));
        }

        /// <summary>
        /// Public method: Trigger the resumption after a delay
        /// </summary>
        public void TriggerRestore()
        {
            if (_restoreDelayCoroutine != null) StopCoroutine(_restoreDelayCoroutine);
            _restoreDelayCoroutine = StartCoroutine(RestoreAfterDelay());
        }

        private IEnumerator RestoreAfterDelay()
        {
            yield return new WaitForSeconds(restoreDelay);
            if (_animationCoroutine != null) StopCoroutine(_animationCoroutine);
            _animationCoroutine = StartCoroutine(ChangeScale(_originalScale));
            _restoreDelayCoroutine = null;
        }

        private IEnumerator ChangeScale(Vector3 targetScale)
        {
            Vector3 startScale = transform.localScale;
            float duration = targetScale == _originalScale ? restoreDuration : squashDuration;
            
            float startY = transform.position.y;
            float endY = startY - (startScale.y - targetScale.y) * 0.5f;

            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                float progress = elapsedTime / duration;
                transform.localScale = Vector3.Lerp(startScale, targetScale, progress);
                transform.position = new Vector3(transform.position.x, Mathf.Lerp(startY, endY, progress), transform.position.z);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            transform.localScale = targetScale;
            transform.position = new Vector3(transform.position.x, endY, transform.position.z);
            _animationCoroutine = null;
        }
    }
}
