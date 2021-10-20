using DG.Tweening;
using System.Collections;
using UnityEngine;

public class SlingshotDevice : Device
{
    public AppsManager appsManager;
    public HapticNetworkBehaviour hapticApi;
    public Transform pocket;
    public AudioClip clickSound;
    public AudioClip shotSound;
    public AudioSource audioSource;
    public float dollyMinPos;
    public float dollyMaxPos;
    public GameObject projectilePrefab;
    public float strength = 50;

    int lastFrameFlooredPos = 0;
    Tween tween;
    GameObject loadedProjectile;
    Material loadedProjectileMat;

    private void Start()
    {
        Reload();
    }

    public void OnShot(float tension)
    {
        // Reset position of the pocket
        tween = pocket.DOLocalMoveZ(dollyMinPos, 0.25f).SetEase(Ease.InQuad).OnComplete(() => Reload());
        loadedProjectile.transform.parent = null;
        Rigidbody rigidbdoy = loadedProjectile.GetComponent<Rigidbody>();
        rigidbdoy.isKinematic = false;
        rigidbdoy.AddForce(-pocket.forward * tension * strength);
        loadedProjectile.GetComponentInChildren<TrailRenderer>().enabled = true;
        loadedProjectile = null;
        loadedProjectileMat = null;
        StartCoroutine(ShotSoundFx(tension));
    }

    IEnumerator ShotSoundFx(float tension)
    {
        audioSource.pitch = 1;
        for (int i = 0; i < 5; i++)
        {
            audioSource.PlayOneShot(clickSound);
            yield return new WaitForSeconds(0.025f);
        }
        audioSource.pitch = Mathf.Lerp(0.6f, 1.0f, tension);
        audioSource.PlayOneShot(shotSound);
    }

    float dissolveProgress = 1;

    void Reload()
    {
        loadedProjectile = Instantiate(projectilePrefab, pocket);
        loadedProjectile.transform.localPosition = new Vector3(0.0f, -0.02f, -0.02f);
        loadedProjectile.transform.DOLocalMoveY(0.0f, 0.5f);

        loadedProjectileMat = loadedProjectile.GetComponent<Renderer>().material;
        dissolveProgress = 0;
        DOTween.To(() => dissolveProgress, x => dissolveProgress = x, 10, 0.5f).OnUpdate(() => OnProgessUpdated()).SetEase(Ease.InSine);
    }

    private void OnProgessUpdated()
    {
        if(loadedProjectileMat)
            loadedProjectileMat.SetFloat("_FresnelPower", dissolveProgress);
    }

    public void OnMove(float tension)
    {
        if(tween != null && tween.IsPlaying()) pocket.DOKill();

        int flooredPos = (int)(tension * 10);
        if (hapticApi && lastFrameFlooredPos != flooredPos)
        {
            hapticApi.SendHapticRequest();
            audioSource.PlayOneShot(clickSound);
        }
        lastFrameFlooredPos = (int)(tension * 10);

        // Adjust position of the pocket
        Vector3 pos = pocket.transform.localPosition;
        pos.z = Mathf.Lerp(dollyMinPos, dollyMaxPos, tension);
        pocket.transform.localPosition = pos;
    }

    public override void ToggleOn()
    {
        gameObject.SetActive(true);
    }

    public override void ToggleOff()
    {
        gameObject.SetActive(false);
    }

    public override float GetShowDuration()
    {
        return 0;
    }

    public override float GetHideDuration()
    {
        return 0;
    }
}
