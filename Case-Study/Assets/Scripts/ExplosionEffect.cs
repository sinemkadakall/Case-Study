using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExploitAnimation : MonoBehaviour
{
    [SerializeField] private float explosionForce = 12f;
    [SerializeField] private float explosionDuration = 1.5f;
    [SerializeField] private float fallSpeed = 3000f;
    [SerializeField] private float delayBeforeParticles = 0.2f; // Parçacýk efektleri için gecikme

    public static void TriggerExplosionAtGrid(GameObject node)
    {
        var target = node.transform.Find("ExploitPaths")?.gameObject;
        if (target != null)
        {
            var animator = target.GetComponent<ExploitAnimation>();
            if (animator == null)
                animator = target.AddComponent<ExploitAnimation>();

            // Patlama sekansýný baþlat
            animator.StartCoroutine(animator.HandleExplosionSequence(target));
        }
    }

    private IEnumerator HandleExplosionSequence(GameObject target)
    {
        // Ana parçayý hazýrla
        PrepareGridForExplosion(target);

        // Parçacýk efektlerini baþlatmadan önce kýsa bir bekleme
        yield return new WaitForSeconds(delayBeforeParticles);

        // Parçacýk efektlerini baþlat
        ScatterChildrenOf(target);

        // Tüm efektler bitince objeyi temizle
        yield return new WaitForSeconds(explosionDuration);
        Destroy(target);
    }

    private void PrepareGridForExplosion(GameObject gridObject)
    {
        var mainObject = gridObject.transform.Find("main")?.gameObject;
        if (mainObject != null)
        {
            // Ana parçayý yavaþça saydamlaþtýr
            StartCoroutine(FadeOutMain(mainObject));
        }

        Vector3 worldPosition = gridObject.transform.position;
        gridObject.transform.SetParent(null);
        gridObject.transform.position = worldPosition;
        gridObject.SetActive(true);
    }

    private IEnumerator FadeOutMain(GameObject mainObject)
    {
        SpriteRenderer renderer = mainObject.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            float elapsedTime = 0f;
            Color startColor = renderer.color;

            while (elapsedTime < delayBeforeParticles)
            {
                elapsedTime += Time.deltaTime;
                float alpha = 1 - (elapsedTime / delayBeforeParticles);
                renderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }
        }
        mainObject.SetActive(false);
    }

    private void ScatterChildrenOf(GameObject explosionPath)
    {
        List<Transform> children = new List<Transform>();
        foreach (Transform child in explosionPath.transform)
        {
            children.Add(child);
        }

        // Parçacýklarý rastgele sýrayla baþlat
        foreach (Transform child in children)
        {
            StartCoroutine(ScatterChild(child.gameObject));
        }
    }

    private IEnumerator ScatterChild(GameObject child)
    {
        Vector3 startPosition = child.transform.position;
        Vector3 direction = GetRandomDirection();
        Quaternion startRotation = child.transform.rotation;
        Quaternion targetRotation = Random.rotation;
        float elapsedTime = 0f;

        SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = child.AddComponent<SpriteRenderer>();

        Color startColor = spriteRenderer.color;

        while (elapsedTime < explosionDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / explosionDuration;

            // Geliþtirilmiþ hareket ve düþüþ fiziði
            float explosionProgress = Mathf.Sin(progress * Mathf.PI * 0.5f);
            Vector3 explosionOffset = direction * explosionForce * (1 - explosionProgress);
            Vector3 fallOffset = Vector3.down * fallSpeed * progress * progress;

            Vector3 newPosition = startPosition + explosionOffset + fallOffset;
            Quaternion newRotation = Quaternion.Lerp(startRotation, targetRotation, progress);

            // Yumuþak geçiþli saydamlýk
            float alpha = Mathf.Lerp(1f, 0f, Mathf.Pow(progress, 0.5f));
            spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            child.transform.position = newPosition;
            child.transform.rotation = newRotation;

            yield return null;
        }

        Destroy(child);
    }

    private Vector3 GetRandomDirection()
    {
        return new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.2f, 1f),
            0
        ).normalized;
    }
}