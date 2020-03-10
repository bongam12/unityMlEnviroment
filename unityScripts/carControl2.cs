using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carControl2 : MonoBehaviour
{
    private Vector3 startPosition, startRotation;
    private reinforcement2Child network;
    public Transform rayFirst;
    [Range(-1f, 1f)]
    public float a, t, eatChoice;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;
    public float eatMultiplier = .2f;
    public float speedMultiplier;
    private float counter;
    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    private float timeLeft;
    private float aSensor, bSensor, cSensor, eatFloat;
    private float eatSensor;
    public Animator anim;

    private void Awake()
    {
        anim = this.GetComponent<Animator>();
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        speedMultiplier = 1f;


    }

    public void ResetWithNetwork(reinforcement2Child net)
    {

        network = net;

        Reset();
        Debug.Log("resetting");
    }



    public void Reset()
    {

        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;
        transform.position = startPosition;
        transform.eulerAngles = startRotation;
        Debug.Log("resetting2");
    }
    private void noFlying()
    {
        if (this.transform.position.y > 13.07f)
        {
            Death();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject);
        Debug.Log("colided");
        Death();
    }



    public void OnTriggerEnter(Collider other)
    {
        counter = 0;
        Debug.Log(other.tag);
        if (other.gameObject.tag == "fire")
        {
            Death();
        }
        if (other.gameObject.tag == "apple")
        {


            if (eatChoice < .42f && counter < 1)
            {
                //apple id number

                counter = counter + 1;
                eatFloat = .42f;
                eatSensor += .1f;

                speedMultiplier = 0f;
                anim.SetBool("pickFruit", true);
                timeLeft = 1f;


            }
            else
            {

                speedMultiplier = 1f;

            }

            //put eat object


        }
        else
        {

        }


    }



    private void FixedUpdate()
    {

        InputSensors();
        lastPosition = transform.position;
        //eatfloat == food in numbers

        (a, t, eatChoice) = network.RunNetwork2(aSensor, bSensor, cSensor, eatFloat);


        MoveCar(a, t);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();
        noFlying();

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("pickFruit"))
        {
            // do something\

            timeLeft -= Time.fixedDeltaTime;
            if (timeLeft < 0)
            {
                anim.SetBool("pickFruit", false);
                Debug.Log("found object");
                Debug.Log("eat object object");
                counter = 0;
                speedMultiplier = 1f;
            }




        }
        //a = 0;
        //t = 0;


    }

    private void Death()
    {
        GameObject.FindObjectOfType<gameManager>().Death2(overallFitness, network);

    }

    private void CalculateFitness()
    {

        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultipler) + (avgSpeed * avgSpeedMultiplier) + (eatSensor * eatMultiplier) + (((aSensor + bSensor + cSensor) / 3) * sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 50)
        {
            //if performance real bad --> alive 20s plus and low fitness
            Death();
        }

        if (overallFitness >= 1000)
        {
            //kill with too good fitness
            Death();
            //save weights too file
        }

    }





    private void InputSensors()
    {
        //raycasts for objects
        Vector3 fwd = rayFirst.transform.TransformDirection(Vector3.forward);
        Vector3 a = (-(transform.right) + fwd).normalized;


        Vector3 b = fwd;
        Vector3 c = (-(-transform.right) + fwd).normalized;

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / 20;
            print("a sense: " + aSensor);
            Debug.DrawLine(r.origin + Vector3.up * 1.3f, hit.point, Color.magenta);
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / 20;
            print("b sense: " + bSensor);
            Debug.DrawLine(r.origin + Vector3.up * 1.3f, hit.point, Color.cyan);

        }

        r.direction = c;

        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / 20;
            print("c sense: " + cSensor);
            Debug.DrawLine(r.origin + Vector3.up * 1.3f, hit.point, Color.green);
        }

    }

    private Vector3 inp;
    public void MoveCar(float v, float h)
    {
        //speed
        if (v > 0)
        {
            anim.SetBool("walking", true);
        }
        else
        {
            anim.SetBool("idle", true);
        }
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, (v * 5.4f)), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp * speedMultiplier;
        //wheel angles
        var desiredMoveDirection = new Vector3((h * 90) * 0.04f, 0, (v * 5.4f));

        Quaternion quaternion = Quaternion.Euler(200 * h * 0.04f, 0, 0);
        transform.rotation = Quaternion.Slerp(quaternion, Quaternion.LookRotation(desiredMoveDirection), 2f);
        //transform.rotation = new Vector3((h * 90) * 0.02f, 0, 0);
        Debug.Log(transform.eulerAngles);
    }
    }