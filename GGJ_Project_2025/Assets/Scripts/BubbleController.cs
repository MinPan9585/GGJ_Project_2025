using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BubbleController : MonoBehaviour
{
    public GameObject bubbleChild; // 泡泡的子物体（用于显示/隐藏）
    private bool isVisible = true; // 泡泡是否处于显示状态
    private int triggerCount = 0; // 当前进入泡泡区域的生物数量
    public float respawnTimer = 3f; // 泡泡重新出现的计时器
    private bool isRespawning = false; // 是否正在计时重新出现
    private HashSet<Collider> insideEntities = new HashSet<Collider>(); // 记录进入泡泡区域的对象
    public AudioSource Bubble;

    public bool IsVisible()
    {
        return isVisible;
    }

    public void SetBubbleVisible(bool visible)
    {
        isVisible = visible;
        bubbleChild.SetActive(visible);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || (other.CompareTag("Dog") && other.TryGetComponent(out DogController dogController) && dogController.isVisible))
        {
            if (!insideEntities.Contains(other))
            {
                insideEntities.Add(other);
                triggerCount++;
                SetBubbleVisible(false);

                if (Random.Range(0f, 1f) > 0.8f)
                {
                    Bubble.Play();
                }
            }
        }
        else if (other.CompareTag("Dog") && other.TryGetComponent(out DogController hiddenDogController) && !hiddenDogController.isVisible && !isVisible)
        {
            StartCoroutine(hiddenDogController.ForceReveal(hiddenDogController.bubbleForceRevealTimer));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (insideEntities.Contains(other))
        {
            insideEntities.Remove(other);
            triggerCount--;
            if (triggerCount <= 0 && !isRespawning)
            {
                StartCoroutine(RespawnBubble());
            }
        }
    }

    public void CheckEntityStatus()
    {
        foreach (Collider entity in insideEntities)
        {
            if (entity.CompareTag("Dog"))
            {
                DogController dgCon = entity.GetComponent<DogController>();
                if (dgCon != null && dgCon.isVisible)
                {
                    SetBubbleVisible(false);
                    return;
                }
            }

            if (entity.CompareTag("Player"))
            {
                SetBubbleVisible(false);
                return;
            }
        }

        if (triggerCount <= 0 && !isRespawning)
        {
            StartCoroutine(RespawnBubble());
        }
    }

    private IEnumerator RespawnBubble()
    {
        isRespawning = true;
        float timer = respawnTimer;

        while (timer > 0)
        {
            if (triggerCount > 0)
            {
                isRespawning = false;
                yield break;
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        SetBubbleVisible(true);
        isRespawning = false;
    }
}
