using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private LayerMask Ground;
    [SerializeField] private float jumpSpeed = 7f;
    [SerializeField] private float rushDistance = 5f;
    [SerializeField] private float RisingGravityScale = 5f;
    [SerializeField] private float fallingGravityScale = 10f;
    [SerializeField] private Vector2 maxFallingSpeed = new Vector2(0, -10f);
    [SerializeField] private Vector3 resetPos;

    private Rigidbody2D rb;
    private BoxCollider2D foot;
    private BoxCollider2D body;
    private float moveInput;
    private float stepTimer = 0;
    [SerializeField] private float KPressedTimer = 0f; //记录K键按下的时间
    [SerializeField] private float jumpPressThreshold = 0.3f;

    [SerializeField] private float accelerationDurationTime = 0.2f;
    [SerializeField] private float decelerationDurationTime = 0.2f;
    private float acceleration;
    private float deceleration;
    [SerializeField] private float currentSpeed = 0f;

    private bool isMovingRight = true;
    private bool isWalking => moveInput != 0;
    private bool isGrounded => foot.IsTouchingLayers(Ground);
    private bool isJumping => rb.velocity.y > 1e-4;
    private bool isFalling => rb.velocity.y < -1e-4;

<<<<<<< Updated upstream
=======
    private bool isRushing = false;
    [SerializeField] private float rushDuration = 1f;
    [SerializeField] private float rushingSpeed = 24f;
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Animator Animator;

    private Coroutine jumpCoroutine;

>>>>>>> Stashed changes
    // 是否带着松子
    private bool isTakingAcorn;
    public bool GetIsTakingAcorn() => isTakingAcorn;
    public void GetAcorn() { isTakingAcorn = true; }

    private void Start()
    {
        body = gameObject.GetComponents<BoxCollider2D>()[1];
        foot = gameObject.GetComponents<BoxCollider2D>()[0];
        rb = gameObject.GetComponent<Rigidbody2D>();
        acceleration = moveSpeed / accelerationDurationTime;
        deceleration = moveSpeed / decelerationDurationTime;
        isTakingAcorn = false;
    }

    public void Reset()
    {
        isTakingAcorn = false;
        rb.velocity = Vector2.zero;
        gameObject.transform.position = resetPos;
    }

    private void Update()
    {
        if (GameManager.Instance.GetIsGaming())
            CheckInput();

        if (transform.position.y < -30)
        {
            GameManager.Instance.OnPlayerDied();
        }
    }

    private void CheckInput()
    {
        if(isRushing)
        {
            return;
        }
        moveInput = Input.GetAxisRaw("Horizontal");

        // 根据玩家输入逐渐改变速度
        if (moveInput != 0)
        {                  
            if( Mathf.Sign(currentSpeed) == Mathf.Sign(moveInput) || currentSpeed == 0)
            {
                currentSpeed += moveInput * acceleration * Time.deltaTime;
            }
            else
            {
                float sign = Mathf.Sign(currentSpeed);
                currentSpeed -= sign * deceleration * Time.deltaTime;
                if (Mathf.Sign(currentSpeed) != sign) // 确保速度的方向正确
                {
                    currentSpeed = 0f;
                }
            }
            currentSpeed = Mathf.Clamp(currentSpeed, -moveSpeed, moveSpeed); // 限制速度在最大速度范围内
        }
        else // 如果没有输入，则逐渐减小速度
        {
            float sign = Mathf.Sign(currentSpeed);
            currentSpeed -= sign * deceleration * Time.deltaTime;
            if (Mathf.Sign(currentSpeed) != sign)
            {
                currentSpeed = 0f;
            }
        }

        rb.velocity = new Vector2(currentSpeed, rb.velocity.y);

        stepTimer -= Time.deltaTime;
        if ((Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) && stepTimer <= 0)
        {
            stepTimer = 0.3f;
            AudioManager.Ins.PlaySounds("walk", GameManager.Instance.GetPlayer().transform.position);
        }
        if (isGrounded)
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                AudioManager.Ins.PlaySounds("jump", GameManager.Instance.GetPlayer().transform.position);
                rb.gravityScale = RisingGravityScale;
                rb.velocity += new Vector2(0, jumpSpeed);
                Debug.Log(KPressedTimer);
                KPressedTimer = 0f;
            }
        }

        if(!isRushing && Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(Rush());
        }

        
        ControllJump();
        CheckDirection();
    }

<<<<<<< Updated upstream
=======
    private bool CheckJumpInputTolerance()
    {
        //考虑还未落地时的
        if (Physics2D.Raycast(JumpToleranceChecker.position,Vector2.down,jumpToleranceDistance,LayerMask.GetMask("Ground")))
        {
            if (jumpCoroutine != null)
            {
                StopCoroutine(jumpCoroutine);
            }
            // 记录跳跃，落地后立即跳跃
            jumpCoroutine = StartCoroutine(RecordJump());
            return true;
        }
        else
        {
            return false;
        }
    }

    private IEnumerator Rush()
    {
        isRushing = true;
        rb.velocity = new Vector3(Mathf.Sign(transform.rotation.y) * rushingSpeed,3f,0f);
        trailRenderer.emitting = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 1f;
        yield return new WaitForSeconds(rushDuration);
        rb.gravityScale = originalGravity;
        trailRenderer.emitting = false;
        isRushing = false;
    }
    private IEnumerator RecordJump()
    {
        Debug.Log("Tolerance");
        while (!isGrounded)
        {
            yield return null;
        }
        yield return new WaitForSeconds(0.05f);
        Jump();
    }

    private void Decelerate()
    {
        float sign = Mathf.Sign(currentSpeed);
        currentSpeed -= sign * deceleration * Time.deltaTime;
    }

    private void Accelerate()
    {
        float sign = Mathf.Sign(moveInput);
        currentSpeed += sign * acceleration * Time.deltaTime;
    }

>>>>>>> Stashed changes
    private void CheckDirection()
    {
        if (isMovingRight && moveInput < 0)
        {
            Flip();
        }
        else if (!isMovingRight && moveInput > 0)
        {
            Flip();
        }
    }


    private void ControllJump()
    {
        if(isJumping)
        {
            KPressedTimer += Time.deltaTime;
            if (KPressedTimer < jumpPressThreshold && Input.GetKeyUp(KeyCode.K))
            {
                rb.gravityScale = fallingGravityScale;
            }
        }
        if (isFalling)
        {
            //下落加快
            rb.gravityScale = fallingGravityScale;
            if(rb.velocity.y < maxFallingSpeed.y)
            {
                rb.velocity = maxFallingSpeed;
            }
        }
    }
    private void Flip()
    {
        transform.Rotate(0, 180f, 0);
        isMovingRight = !isMovingRight;
    }

    public bool IsWalking() => isWalking;

    public bool IsGrounded() => isGrounded;

    public bool IsJumping() => isJumping;

    public bool IsFalling() => isFalling;

    public bool IsRushing() => isRushing;
}
