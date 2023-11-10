using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Swinging : MonoBehaviour
{
    [Header("References")]
    [SerializeField] playerController playerScript;
    [SerializeField] Transform player;
    [SerializeField] Transform cam;
    [SerializeField] Transform gunTip;
    [SerializeField] LayerMask whatIsGrappleable;
    [SerializeField] LineRenderer lr;
    [SerializeField] Image grappleCooldownImage;
    public Transform grappleGun;

    [Header("Grapple")]
    public float maxGrappleDistance;
    private Vector3 grappleGunOrigin;
    private Quaternion grappleGunShootPos;
    public Vector3 grapplePoint;
    private SpringJoint joint;

    [Header("Cooldown")]
    [SerializeField] float grapplingCd;
    private float grapplingCdTimer;

    [Header("Input")]
    public KeyCode grappleKey = KeyCode.E;

    public bool isGrappling;
    bool canGrapple;
    bool onCooldown;
    bool toggleGraple = false;

    public void Start()
    {
        playerScript = GetComponent<playerController>();
        grappleGunOrigin = grappleGun.position;
        grappleCooldownImage = GameObject.Find("Grapple Cooldown").GetComponent<Image>();
        grappleCooldownImage.fillAmount = 0;
    }

    private void Update()
    {
        RaycastHit hit;
        if (Input.GetKeyDown(grappleKey))
        {
            if (onCooldown)
            {
                return; // Do nothing if still in cooldown
            }

            if (toggleGraple)
            {
                StopSwing();
                StartCoroutine(Cooldown());
            }
            else if (!toggleGraple)
            {
                if (!Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
                    return;
                StartSwing();
            }

            toggleGraple = !toggleGraple;
        }


        //if (Input.GetKeyDown(grappleKey))
        //{
        //    if (toggleGraple)
        //    {
        //        StopSwing();
        //        StartCoroutine(Cooldown());
        //    }
        //    else if(canGrapple && !toggleGraple) 
        //    {
        //        StartSwing();
        //    }

        //    toggleGraple = !toggleGraple;
        //}

        //
        //if (Input.GetKeyUp(grappleKey))
        //{
        //    StopSwing();
        //    StartCoroutine(Cooldown());
        //}

        if (grapplingCdTimer > 0)
        {
            grapplingCdTimer -= Time.deltaTime;
        }

        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            gameManager.instance.grappleBar1.color = gameManager.instance.GrappleYes;
            gameManager.instance.grappleBar2.color = gameManager.instance.GrappleYes;
            gameManager.instance.grappleBar3.color = gameManager.instance.GrappleYes;
        }
        else
        {
            gameManager.instance.grappleBar1.color = gameManager.instance.GrappleNo;
            gameManager.instance.grappleBar2.color = gameManager.instance.GrappleNo;
            gameManager.instance.grappleBar3.color = gameManager.instance.GrappleNo;
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    private void StartSwing()
    {
        if (grapplingCdTimer > 0)
            return;


        RaycastHit hit;
        if (Physics.Raycast(cam.position, cam.forward, out hit, maxGrappleDistance, whatIsGrappleable))
        {
            isGrappling = true;
            grapplePoint = hit.point;
            joint = player.gameObject.AddComponent<SpringJoint>();

            lr.positionCount = 2;
            currentGrapplePosition = gunTip.position;
        }

    }


    public void StopSwing()
    {
        isGrappling = false;
        lr.enabled = false;
        lr.positionCount = 2;
        Destroy(joint);
    }

    private Vector3 currentGrapplePosition;

    public void DrawRope()
    {
        if (!joint)
            return;

        lr.enabled = true;
        currentGrapplePosition =
            Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, currentGrapplePosition);

    }

    IEnumerator Cooldown()
    {
        if (onCooldown)
            yield break;

        onCooldown = true;
        canGrapple = false;
        grappleCooldownImage.fillAmount = 1;

        float remainingTime = grapplingCd;
        while (remainingTime >= 0)
        {
            grappleCooldownImage.fillAmount = Mathf.Lerp(remainingTime / grapplingCd, grappleCooldownImage.fillAmount, .1f);
            yield return new WaitForSeconds(.1f);
            remainingTime -= .1f;
        }
        grappleCooldownImage.fillAmount = 0;
        canGrapple = true;
        onCooldown = false;
    }
}