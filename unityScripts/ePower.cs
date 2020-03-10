using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ePower : MonoBehaviour
{
    public Animator anim;
    public bool movementBool = false;
    public CharacterController cc;
    public float speed;
    
    public float jumper = 10f;
    public Rigidbody rb;
    private float veticalVel;
    private Vector3 moveD = Vector3.zero;
    private float gravity = 14f;
    public ParticleSystem e1;
    public ParticleSystem e1Point;
    public ParticleSystem trsn;
    public float count= 0;
    private NewBehaviourScript idlSt;
    public NewBehaviourScript inputX;
    public NewBehaviourScript inputZ;
    public NewBehaviourScript speeder;
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
        inputX = cc.GetComponent<NewBehaviourScript>();
        inputZ = cc.GetComponent<NewBehaviourScript>();
        speeder = cc.GetComponent<NewBehaviourScript>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        e1.enableEmission = false;
        e1Point.enableEmission = false;
        trsn.Stop();
        var main = trsn.main;
        main.duration = 1.0f;
        idlSt = cc.GetComponent<NewBehaviourScript>();
    }

    // Update is called once per frame
    void Update()
    {
        blast();
        e1Move();
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            anim.SetBool("powerUp", true);
            anim.SetBool("idle", false);
            anim.SetBool("e-movement", false);
            trsn.Play();
            count = count + 1;
            movementBool = true;
            e1.enableEmission = true;
            inputX.InputX = 0;
            inputZ.InputZ = 0;
            speeder.speed = 0;
            cc.Move(new Vector3(0, 0, 0));
        }
        else if(Input.GetKeyUp(KeyCode.RightShift))
        {
            anim.SetBool("powerUp", false);
            anim.SetBool("idle", false);
            anim.SetBool("e-movement", false);
            trsn.Stop();
            inputX.InputX = 0;
            inputZ.InputZ = 0;
            speeder.speed = 0;
            cc.Move(new Vector3(0, 0, 0));
            //  anim.SetBool("idle", true);

        }
        
        //else if(Input.GetKeyDown(KeyCode.RightShift) && count >= 2)
        //{
        //    anim.SetBool("idle", false);
        //    anim.SetBool("powerUp", true);
        //    count = count + 1;

        //    e1.enableEmission = true;
        //}

        movement();
    }

    void e1Move()
    {
        anim.SetBool("idle", false);
        anim.SetBool("e-movement", false);
        if (Input.GetKey(KeyCode.RightCommand))
        {
            idlSt.idlSet = true;
            anim.SetBool("idle", false);
            anim.SetBool("e-movement", false);
            anim.SetBool("efinger", true);

            e1Point.enableEmission = true;
           
            Debug.Log("blast!");



        }
       
        else
        {
            idlSt.idlSet = false;
            e1Point.enableEmission = false;
            anim.SetBool("efinger", false);
            

        }
    }
    void blast()
    {

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            anim.SetBool("idle", false);
            
            //jump script
            Debug.Log("blast!");
            anim.SetBool("eBlast", true);
           

        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            

            //jump script
            Debug.Log("blast off");
            anim.SetBool("idle", true);
            anim.SetBool("eBlast", false);
        }


    }


    void movement()
    {
        if (count >= 2)
        {
            anim.SetBool("idle", false);
            anim.SetBool("e-movement", false);
            anim.SetBool("powerUp", true);
            anim.SetBool("e-movement", false);
           

                //jump script
                Debug.Log("blast off end");
            count = 0;
            e1.enableEmission = false;
            movementBool = false;
            
        }

        if (movementBool == true)
        {
            //anim.SetBool("idle", false);
            
            //jump script
            Debug.Log("e-movement!");
            //anim.SetBool("e-movement", true);


            //movement

            Vector3 forward = transform.forward;
            moveD = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) + forward;
            Vector3 moveDirect = transform.TransformDirection(Vector3.forward) * speed;
            if (Input.GetAxis("Vertical") > 0)
            {
                Debug.Log("movement horizontal activated!");

                moveD += moveDirect;
            }
            else if (Input.GetAxis("Vertical") == 0 && Input.GetAxis("Horizontal") == 0)
            {
                Vector3 zero = new Vector3(0, 0, 0);
                moveD = zero;
            }
            //moveD *= speed;

            anim.SetFloat("Horizontal", Input.GetAxis("Horizontal"));
            anim.SetFloat("Vertical", Input.GetAxis("Vertical"));

            //end walking script
            //jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                anim.SetBool("e-movement", false);
                veticalVel = jumper;
                //jump script
                Debug.Log("Jump!");
                anim.SetBool("jump", true);

            }
            else if (cc.transform.position.y > .1f)
            {
                anim.SetBool("e-movement", false);
            }
            else if (cc.transform.position.y < .5f)
            {
                anim.SetBool("jump", false);

            
                anim.SetBool("e-movement", true);

          
            }


            if (cc.isGrounded)
            {
                veticalVel = -gravity * Time.deltaTime;
                //jump


            }
            else
            {
                veticalVel -= gravity * Time.deltaTime;
                Debug.Log("in Air!");

            }

            Vector3 move = new Vector3(0, veticalVel, 0);
            moveD += move;
            //cc.Move(move * Time.deltaTime);




            // Move the controller

           
            cc.Move(moveD * Time.deltaTime);

            //end jump


            //end movement
            

        }
       
        


    }
}
