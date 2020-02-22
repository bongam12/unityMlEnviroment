using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(reinforcementNet))]
public class carControl : MonoBehaviour
{
    private Vector3 startPosition, startRotation;
    private reinforcementNet network;

    [Range(-1f, 1f)]
    public float a, t;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    [Header("Network Options")]
    public int LAYERS = 1;
    public int NEURONS = 10;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    private float aSensor, bSensor, cSensor;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
        network = GetComponent<reinforcementNet>();


    }

    public void ResetWithNetwork(reinforcementNet net)
    {
        network = net;
        Reset();
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
    }
    private void noFlying()
    {
        if (transform.position.y > 13.9f)
        {
            Death();
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        Death();
    }

    private void FixedUpdate()
    {

        InputSensors();
        lastPosition = transform.position;


        (a, t) = network.RunNetwork(aSensor, bSensor, cSensor);


        MoveCar(a, t);

        timeSinceStart += Time.deltaTime;

        CalculateFitness();
        noFlying();
        //a = 0;
        //t = 0;


    }

    private void Death()
    {
        GameObject.FindObjectOfType<gameManager>().Death(overallFitness, network);
    }

    private void CalculateFitness()
    {

        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultipler) + (avgSpeed * avgSpeedMultiplier) + (((aSensor + bSensor + cSensor) / 3) * sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 40)
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
        Vector3 a = -(transform.up + transform.right);
        Vector3 b = (new Vector3(2f,0,0f));
        Vector3 c = -(transform.up - transform.right);

        Ray r = new Ray(transform.position, a);
        RaycastHit hit;

        if (Physics.Raycast(r, out hit))
        {
            aSensor = hit.distance / 20;
            print("a sense: " + aSensor);
            Debug.DrawLine(r.origin, hit.point, Color.green);
        }

        r.direction = b;

        if (Physics.Raycast(r, out hit))
        {
            bSensor = hit.distance / 20;
            print("b sense: " + bSensor);
            Debug.DrawLine(r.origin, hit.point, Color.green);
        }

        r.direction = c;

        if (Physics.Raycast(r, out hit))
        {
            cSensor = hit.distance / 20;
            print("c sense: " + cSensor);
            Debug.DrawLine(r.origin, hit.point, Color.green);
        }

    }

    private Vector3 inp;
    public void MoveCar(float v, float h)
    {
        //speed
        inp = Vector3.Lerp(Vector3.zero, new Vector3(0, -(v * 11.4f), 0), 0.02f);
        inp = transform.TransformDirection(inp);
        transform.position += inp;
        //wheel angles
        transform.eulerAngles += new Vector3(0, (h * 90) * 0.02f, 0);
    }
}
