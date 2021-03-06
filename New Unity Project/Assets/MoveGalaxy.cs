﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveGalaxy : MonoBehaviour {
    public float speed;

    private Transform mainPlane;
    private bool isFirst = true;
    private bool isLanding = false;
    private bool isClockWise = true;
    private GameObject landPlanet;
    private Vector3 LastAngle;
    private Vector3 LastDirection;
    private int waitCount = 0;
    private bool isGround = false;
    private int jumpCount = 2;
    private bool isJump = false;
    private bool is3D = true;
    private int spinCount = -1;
    private Vector3 pPosition;
    public AudioClip AC;
    private Transform curTransform;
    private bool isLandingOnObstacle = false;

    public delegate void TransformController(bool is3D);
    public static event TransformController RotateEventHandler;
    // Use this for initialization
    void Start() {
        gravity.LandEventHandler += new gravity.LandController(GetLand);
        gravity.LaunchEventHandler += new gravity.LandController(OnLaunch);
        planePosition.MoveEventHandler += new planePosition.MoveController(GetPlane);
        mainPlane = this.transform.parent;
    }

    // Update is called once per frame
    void Update() {
        curTransform = this.transform.parent;
        //Debug.Log(mainPlane);
    }

    private void FixedUpdate()
    {
        if (landPlanet == null)
        {
            isLanding = false;
        }
        else if (waitCount < 5) {
            waitCount++;
        }
        else {
            isLanding = true;
        }

        if (this.LastAngle == null) {
            //Debug.Log("initialize LastAngle");
            this.LastAngle = this.transform.position - landPlanet.transform.position;
        }
        Vector3 direction = this.transform.position - landPlanet.transform.position;

        //Debug.Log("Last:"+this.LastAngle);
        //Debug.Log("Now:"+direction);

        Vector3 temp = new Vector3(landPlanet.transform.position.x, pPosition.y, landPlanet.transform.position.z);

        Vector3 planeDirection = this.transform.position - temp;

        //Debug.Log("direction:"+planeDirection);
        //Debug.Log("OldDirection:" + direction);

        if (isLanding && is3D) {
            this.transform.parent = landPlanet.transform;
        }


        float angle = Vector3.Angle(planeDirection, LastAngle);
        //Debug.Log("Angle:"+angle);

        Vector3 VerticalDirection = new Vector3(1.0f, 0.0f, -direction.x / direction.z).normalized;

        

        if (this.LastDirection == null)
        {
            this.LastDirection = VerticalDirection;
        }
        //Debug.Log("Speed Direction:"+VerticalDirection);
        if (Vector3.Angle(this.LastDirection, VerticalDirection) > 90) {
            VerticalDirection = -VerticalDirection;
        }

        
        //Movement
        if (isLanding && Input.GetKey(KeyCode.D))
        {
            Vector3 clockwiseJudge = Vector3.Cross(new Vector3(planeDirection.x, 0.0f, planeDirection.z),
                                               VerticalDirection);


            if (clockwiseJudge.y > 0)
            {
                isClockWise = true;
                //Debug.Log("clockwise!");
            }
            else
            {
                isClockWise = false;
                //Debug.Log("inverse!");
            }
            
            if (Input.GetKeyDown(KeyCode.Space) && jumpCount > 0)
            {
                isJump = true;
                isGround = false;
                //GetComponent<Rigidbody>().AddForce(30 * direction);
                GetComponent<Rigidbody>().velocity = speed * VerticalDirection + 0.5f * direction;
                jumpCount--;
            }
            else if (isGround || isLandingOnObstacle)
            {
                //Debug.Log("Normal Move");
                GetComponent<Rigidbody>().velocity = speed * VerticalDirection;
            }


        }
        else if (isLanding && Input.GetKey(KeyCode.A) && jumpCount > 0)
        {
            Vector3 clockwiseJudge = Vector3.Cross(new Vector3(planeDirection.x, 0.0f, planeDirection.z),
                                               VerticalDirection);


            if (clockwiseJudge.y < 0)
            {
                isClockWise = true;
                //Debug.Log("clockwise!");
            }
            else
            {
                isClockWise = false;
                //Debug.Log("inverse!");
            }

           
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isJump = true;
                isGround = false;
                //GetComponent<Rigidbody>().AddForce(30 * direction);
                GetComponent<Rigidbody>().velocity = -speed * VerticalDirection + 0.5f * direction;
                jumpCount--;
            }
            else if (isGround || isLandingOnObstacle)
            {
                GetComponent<Rigidbody>().velocity = -speed * VerticalDirection;
            }
        }

        //Stop
        if (isLanding && (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.A)))
        {
            isFirst = true;
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }

        if (isLanding && is3D) {
            if (isClockWise)
            {
                //Debug.Log(angle);
                //Debug.Log("clockWise!");
                GetComponent<Rigidbody>().transform.Rotate(new Vector3(0.0f, angle, 0.0f));
                this.transform.Find("Camera").transform.Rotate(new Vector3(0.0f, 0.0f, angle));
            }
            else
            {
                //Debug.Log(angle);
                //Debug.Log("deClockWise!");
                GetComponent<Rigidbody>().transform.Rotate(new Vector3(0.0f, -angle, 0.0f));
                this.transform.Find("Camera").transform.Rotate(new Vector3(0.0f, 0.0f, -angle));
            }
        }

        //Switch Camera
        if (Input.GetKeyDown(KeyCode.F1) && !is3D)
        {
            this.transform.parent = curTransform.transform;
            RotateEventHandler(true);
            is3D = true;
            spinCount++;
        }
        else if(Input.GetKeyDown(KeyCode.F2) && is3D)
        {
            this.transform.parent = mainPlane.transform;
            RotateEventHandler(false);
            is3D = false;
            spinCount++;
        }

        if (spinCount >= 0 && is3D)
        {
           
           this.transform.Rotate(Vector3.back * 5);
           spinCount++;
        }
        else if(spinCount >= 0 && !is3D)
        {
            
            this.transform.Rotate(Vector3.forward * 5);
            spinCount++;
        }

        if (spinCount == 18) {
            spinCount = -1;
        }

        //jump
        if (!isJump && Input.GetKeyDown(KeyCode.Space)) {
            GetComponent<Rigidbody>().velocity = 0.5f * direction;
        }

        //Jump
        /*if (isLanding && Input.GetKeyDown(KeyCode.Space)) {
            if (!Input.GetKey(KeyCode.A) || !Input.GetKey(KeyCode.D))
            {

            }
            else
            {
                GetComponent<Rigidbody>().AddForce(25 * direction);
            }
            
        }*/


        this.LastAngle = planeDirection;
        this.LastDirection = VerticalDirection;
    }


    void GetLand(GameObject game) {
        jumpCount = 2;
        isGround = true;
        landPlanet = game;
        isJump = false;
    }

    void OnLaunch(GameObject game)
    {
        if (this.landPlanet == game) {
            this.landPlanet = null;
        }
    }

    void GetPlane(Vector3 pPosition) {
        this.pPosition = pPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.name == "Game Objective") {
            Debug.Log("获得一个方块！:" + other.name);
            //AudioSource.PlayClipAtPoint(AC, transform.localPosition);
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("collision:" + collision.gameObject.name);
        if (collision.gameObject.name.Split('_')[0].Equals("decorate")) {
            isLandingOnObstacle = true;
        }
    }
}
