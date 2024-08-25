using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Animator animator;
    public float moveSpeed = 5f;
    
    private Vector2 moveInput;
    private bool isMoving;
    public Rigidbody2D rb;
   
    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        
        UpdateAnimatorParameters();
        Vector2 dir = Vector2.zero;
            if (Input.GetKey(KeyCode.A))
            {
                dir.x = -1;
                animator.SetInteger("Direction", 3);
            }
            else if (Input.GetKey(KeyCode.D))
            {
                dir.x = 1;
                animator.SetInteger("Direction", 2);
            }

            if (Input.GetKey(KeyCode.W))
            {
                dir.y = 1;
                animator.SetInteger("Direction", 1);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                dir.y = -1;
                animator.SetInteger("Direction", 0);
            }
        MovePlayer();
    }

    void UpdateAnimatorParameters()
    {
        isMoving = moveInput.magnitude > 0f;

        animator.SetBool("IsMoving", isMoving);
        animator.SetFloat("Horizontal", moveInput.x);
        animator.SetFloat("Vertical", moveInput.y);
    }

    void MovePlayer()
    {

        if (moveInput.magnitude > 1)
        {
            moveInput.Normalize();
        }
        rb.MovePosition(rb.position +moveInput*moveSpeed*Time.deltaTime );
    }
}

