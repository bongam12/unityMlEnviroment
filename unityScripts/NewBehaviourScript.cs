using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
 
    
    public Animator anim;
    public float jumper = 10f;
    public Rigidbody rb;
    private float veticalVel;
    private Vector3 moveD = Vector3.zero;
    private float gravity = 14f;
    public ePower electricPower;
    public float punchCount = 0;
    public bool idlSet = false;
    public Transform swordPos;
    public GameObject Sword;
    //movement var



    public float InputX;
    public float InputZ;

    public Vector3 desiredMoveDirection;

    public bool blockRotPlayer;
    public Camera CamObj;

    public float desiredRotSpeed;
    
    public float speed;
    public float allowdplyrRot;
    public CharacterController cc;
    public float onGround;
    public float verticalVal;
    private Vector3 moveVector;
    public bool Grd;
    public bool isJumping;
    public float jumpForce;
    [SerializeField] private AnimationCurve jumpFallOff;
    //end movement var
    // Start is called before the first frame update
    void Start()
    {

        anim = this.GetComponent<Animator>();
        CamObj = Camera.main;
        cc = this.GetComponent<CharacterController>();


    }

    // Update is called once per frame
    void Update()
    {
        //movement
        

        InputMagnitude();
        //gravity
        onGround = this.transform.position.y;
        Grd = false;
        
        if (onGround < 21.8f)
        {
            Grd = true;
           
        }
        if (Grd) {
            verticalVal = 0;

            if (Input.GetKey(KeyCode.Space) && !isJumping)
            {
                JumpInput();
            }
            else
            {
                anim.SetBool("jump", false);
            }

        }
        else
        {

            anim.SetBool("idle", false);
            verticalVal = -.3f ;
        }
moveVector = new Vector3(0, verticalVal, 0);
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    float InputDirection = Input.GetAxis("Horizontal");
        //    moveVector.y = jumpForce;
        //    moveVector.z = InputDirection;
        //    StartCoroutine(JumpEvent());
        //}
        cc.Move(moveVector);
        //end gravity
        //end movement

        //Special moves
        defense();
        //movement();
        fight();
        sword();
        
    }
    
    void sword()
    {
        if (Input.GetKey(KeyCode.LeftCommand))
        {
            anim.SetBool("idle", false);
            anim.SetBool("e-movement", false);
            anim.SetBool("withdrawSword", true);
            Sword.gameObject.transform.position = swordPos.position;
            
            var rotationVector = transform.rotation.eulerAngles;
            rotationVector.z = 206.55f;
            rotationVector.y = -202.79f;
            rotationVector.y = 10f;
            Sword.gameObject.transform.rotation = Quaternion.Euler(rotationVector);
            //(10f, -202.79, 206.55)

        }
        else
        {
            anim.SetBool("sword", true);
            //  anim.SetBool("idle", true);

        }
    }
    void JumpInput()
    {
        if (Input.GetKey(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            StartCoroutine(JumpEvent());
           
            
            anim.SetBool("jump", true);
            //    Debug.Log("jumped !");
        }
        else if(isJumping == false)
        {
            anim.SetBool("jump", false);
           // anim.SetBool("idle", true);
        }

    }
    private IEnumerator JumpEvent()
    {
        cc.slopeLimit = 90.0f;
        float timeInAir = 0.0f;
        anim.SetBool("idle", false);
        do
        {
            float jumpForce = jumpFallOff.Evaluate(timeInAir);
            cc.Move(Vector3.up * jumpForce * jumper * Time.deltaTime);
            timeInAir += Time.deltaTime ;
            yield return null;
        } while (this.transform.position.y > 21.94 && cc.collisionFlags != CollisionFlags.Above);

        cc.slopeLimit = 45.0f;
        isJumping = false;
    
}

    void InputMagnitude()
    {
        float vert = 5f;
        //anim.SetBool("idle", true);

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    anim.SetBool("idle", false);
        //    anim.SetBool("jump", true);
        //    Debug.Log("jumped !");
        //    StartCoroutine(JumpEvent());



        //}
        //else
        //{
        //    anim.SetBool("jump", false);
        //    anim.SetBool("idle", true);


        //}

        //epower
        electricPower = cc.GetComponent<ePower>();
        //walking script
        anim.SetBool("idle", true);
        if (electricPower.movementBool == true)
        {
            anim.SetBool("e-movement", true);
            anim.SetBool("idle", false);

        }
        else if (electricPower.movementBool == true && idlSet == true)
        {

            anim.SetBool("e-movement", false);
        }
        else if (idlSet == true)
        {
            anim.SetBool("idle", false);
        }
        
        
        //end epower
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        anim.SetFloat("InputZ", InputZ, 0f, Time.deltaTime * 2f);
        anim.SetFloat("InputX", InputX, 0f, Time.deltaTime * 2f);

        //calc input magn
       
        
        speed = new Vector2(InputX, InputZ).sqrMagnitude;
        if (InputZ < 0)
        {
            speed = -speed;
        }
        if(speed > allowdplyrRot)
        {
            anim.SetFloat("InputMagnitude", speed, 0f, Time.deltaTime);
            PlayerMovementRot();
        }
        else if (speed < allowdplyrRot)
        {
            anim.SetFloat("InputMagnitude", speed, 0f, Time.deltaTime);
        }
        
    }
    void PlayerMovementRot()
    {
        InputX = Input.GetAxis("Horizontal");
        InputZ = Input.GetAxis("Vertical");

        var cameraM = Camera.main;
        var forward = cameraM.transform.forward;
        var right = cameraM.transform.right;
        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = (forward * InputZ) + (right * InputX);
        
        if (blockRotPlayer == false)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotSpeed);
        }
        

    }
    void defense()
    {
        if (Input.GetKey(KeyCode.R) )
        {
            idlSet = true;
            anim.SetBool("idle", false);
            anim.SetBool("idleBlock", true);

        }
        else if (Input.GetKeyUp(KeyCode.R))
        {
            idlSet = false;
            anim.SetBool("idleBlock", false);
        }
    }
    void fight()
    {
        if(Input.GetKeyDown(KeyCode.E)){
            punchCount = punchCount + 1;
        }
        if (Input.GetKey(KeyCode.E) && punchCount <2)
        {
            anim.SetBool("idle", false);
            anim.SetBool("punch2", false);
            
            anim.SetBool("punch3", false);
            anim.SetBool("punch1", true);
           
        }
        else if(Input.GetKey(KeyCode.E) && punchCount < 3)
        {
            anim.SetBool("idle", false);
            anim.SetBool("punch1", false);
            
            anim.SetBool("punch3", false);
            anim.SetBool("punch2", true);
            
        }
        else if (Input.GetKeyDown(KeyCode.E) && punchCount < 4)
        {
            anim.SetBool("idle", false);
            anim.SetBool("punch2", false);
            anim.SetBool("punch1", false);
            
            anim.SetBool("punch3", true);
            

        }
        else if(punchCount >= 4)
        {
            punchCount = 0;
        }
        else
        {
            anim.SetBool("punch1", false);
            anim.SetBool("punch2", false);
            anim.SetBool("punch3", false);
        }




    }
    void jump()
    {

        
        
        
    }

    void movement()
    {
        electricPower = cc.GetComponent<ePower>();
        //walking script
        anim.SetBool("idle", true);
        if (electricPower.movementBool == true)
        {
            anim.SetBool("e-movement", true);
            anim.SetBool("idle", false);

        }
        else if(electricPower.movementBool == true && idlSet == true)
        {
           
            anim.SetBool("e-movement", false);
        }
        else if(idlSet == true)
        {
            anim.SetBool("idle", false);
        }
            
        
        

       
    }
}
