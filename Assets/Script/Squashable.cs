using UnityEngine;
using System.Collections;

public class Squashable : MonoBehaviour
{
    [Header("挤压设置")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float squashFactor = 0.5f;
    [SerializeField] private float squashDuration = 0.1f;
    [SerializeField] private float restoreDuration = 0.2f;

    [Tooltip("猴子离开后，等待多少秒才开始恢复")]
    [SerializeField] private float restoreDelay = 2f;

    private Vector3 originalScale;
    private Vector3 squashedScale;

    private Coroutine animationCoroutine;
    private Coroutine restoreDelayCoroutine; //用于处理延迟恢复的协程
    private bool isCurrentlySquashed = false;

    private Box boxComponent;

    private void Start()
    {
        originalScale = transform.localScale;
        squashedScale = new Vector3(originalScale.x, originalScale.y * squashFactor, originalScale.z);

        boxComponent = GetComponent<Box>();
        if (boxComponent == null)
        {
            Debug.LogError("Squashable脚本需要与Box脚本在同一个物体上！", this.gameObject);
            this.enabled = false;
        }
    }

    private void Update()
    {
        if (boxComponent == null) return;
        if (boxComponent.IsBeingCarried) return;

        //当猴子在箱子上时
        if (boxComponent.IsBeingSteppedOn)
        {
            //如果恢复延迟正在进行中，则取消它
            if (restoreDelayCoroutine != null)
            {
                StopCoroutine(restoreDelayCoroutine);
                restoreDelayCoroutine = null;
                Debug.Log("恢复延迟被重置！");
            }

            //如果当前没有被压扁，则触发压扁动画
            if (!isCurrentlySquashed)
            {
                TriggerSquash();
            }
        }
        //当猴子不在箱子上时
        else
        {
            //如果当前是被压扁状态，并且恢复延迟还未启动
            if (isCurrentlySquashed && restoreDelayCoroutine == null)
            {
                //启动延迟恢复的协程
                restoreDelayCoroutine = StartCoroutine(DelayedRestore());
            }
        }
    }
    
    private IEnumerator DelayedRestore()
    {
        Debug.Log("恢复延迟开始...");
        yield return new WaitForSeconds(restoreDelay);
        Debug.Log("延迟结束，开始恢复！");
        TriggerRestore();
        restoreDelayCoroutine = null; //协程结束后清空引用
    }


    private void TriggerSquash()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateSquash(squashedScale));
        isCurrentlySquashed = true;
    }

    private void TriggerRestore()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateSquash(originalScale));
        isCurrentlySquashed = false;
    }

    private IEnumerator AnimateSquash(Vector3 targetScale)
    {
        Vector3 startScale = transform.localScale;
        Vector3 startPosition = transform.position;

        float positionOffsetY = (startScale.y - targetScale.y) * 0.5f;
        Vector3 targetPosition = startPosition - new Vector3(0, positionOffsetY, 0);

        float elapsedTime = 0;
        float duration = targetScale == squashedScale ? squashDuration : restoreDuration;

        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / duration);
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
        transform.position = targetPosition;
        animationCoroutine = null;
    }
}