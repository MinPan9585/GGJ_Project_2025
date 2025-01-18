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

    // 判断泡泡是否处于显示状态
    public bool IsVisible()
    {
        return isVisible;
    }

    // 设置泡泡的显示状态
    public void SetBubbleVisible(bool visible)
    {
        isVisible = visible; // 同步更新显示状态
        bubbleChild.SetActive(visible);
    }

    public void OnTriggerEnter(Collider other)
    {
        // 玩家或现身状态的狗进入泡泡区域
        if (other.CompareTag("Player") || (other.CompareTag("Dog") && other.TryGetComponent(out DogController dogController) && dogController.isVisible))
        {
            if (!insideEntities.Contains(other))
            {
                insideEntities.Add(other);
                triggerCount++;
                SetBubbleVisible(false); // 隐藏泡泡
            }
        }
        // 隐身狗进入已隐藏的泡泡
        else if (other.CompareTag("Dog") && other.TryGetComponent(out DogController hiddenDogController) && !hiddenDogController.isVisible && !isVisible)
        {
            StartCoroutine(hiddenDogController.ForceReveal()); // 强制现身狗
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
        //Debug.Log("--------------------------------" + insideEntities.Count);
        // 检查触发区域内的所有实体
        foreach (Collider entity in insideEntities)
        {
            // 如果是现身状态的狗，隐藏泡泡
            if (entity.CompareTag("Dog"))
            {
                DogController dgCon = entity.GetComponent<DogController>();
                if (dgCon != null && dgCon.isVisible)
                {
                    //Debug.Log("set bubble to disappear");
                    SetBubbleVisible(false);
                    return;
                }
            }

            // 如果是玩家，隐藏泡泡
            if (entity.CompareTag("Player"))
            {
                SetBubbleVisible(false);
                return;
            }
        }

        // 如果没有玩家或现身的狗，泡泡可以重新显示（如果计时器允许）
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
                isRespawning = false; // 如果有生物进入，停止计时
                yield break;
            }
            timer -= Time.deltaTime;
            yield return null;
        }

        SetBubbleVisible(true); // 重新显示泡泡
        isRespawning = false;
    }
}











