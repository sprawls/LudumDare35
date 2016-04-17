using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class PullToward : MonoBehaviour {

    //Properties
    /// <summary>
    /// Character needed to be pressed
    /// </summary>
    public char charToActivate;

    private Vector2 pullPosition;

    /// <summary>
    /// The body to be affected this script.
    /// </summary>
    public JelloSpringBody body;

    /// <summary>
    /// The force to add to the body.
    /// </summary>
    public float force = 100f;

    /// <summary>
    /// The number of points adjacent to the selected point to be affected by this scipt.
    /// </summary>
    public int numAdjacentPoints = 3;

    /// <summary>
    /// How fast the body will rotate to try and line up with the pulled back point.
    /// </summary>
    public float rotateSpeed = 2f;

    /// <summary>
    /// The drop off value of how much the adjacent points are affected by the pulling action.
    /// </summary>
    public float adjacentDropoff = 1f;

    /// <summary>
    /// Wheter or not to rotate the body to align with the pulling action.
    /// </summary>
    public bool rotate = true;

    /// <summary>
    /// Constrain the pulling action to a cone of this degree if rotate is false.
    /// </summary>
    public float coneAngle = 90f;

    /// <summary>
    /// How far the selected point is pulled back.
    /// </summary>
    public float pullBackDistance = 0f;

    /// <summary>
    /// The furthest away the selected point may be pulled.
    /// </summary>
    public float maxPullBack = 20f;

    /// <summary>
    /// Whether the adjacent points will ignore collisions (only one frame)
    /// </summary>
    public bool adjacentPointsIgnoreCollision = false;

    /// <summary>
    /// Whether the body must be touching the ground be eligable for the pulling action
    /// </summary>
    public bool requireGrounded = false;

    /// <summary>
    /// Wheter the body is touching ground
    /// </summary>
    private bool grounded = false;

    public enum JumpState { WaitingForFirstJump, AirJumping, FirstJumpOccured, Unable };

    /// <summary>
    /// The state of the jump.
    /// </summary>
    private JumpState jumpState = JumpState.Unable;

    /// <summary>
    /// The point masses adjacent to the selected point mass.
    /// </summary>
    private List<JelloPointMass> adjacentPointMasses = new List<JelloPointMass>();

    /// <summary>
    /// A cache of the adjacent point masses shape matching multipliers.
    /// Used to restore back to old values after the selected point is released.
    /// </summary>
    private List<float> oldMultipliers = new List<float>();

    /// <summary>
    /// The selected point mass
    /// </summary>
    private JelloPointMass pointmass;

    /// <summary>
    /// Whether the body is being pulled.
    /// </summary>
    private bool pulling = false;

    /// <summary>
    /// The index of the selected point mass.
    /// </summary>
    private int pmIndex = -1;

    /// <summary>
    /// The body's position when the pull initiated.
    /// Used to keep the body in place while being pulled.
    /// </summary>
    private Vector2 position;



    // Use this for initialization
    void Start() {
        //Get pull position 
        pullPosition = transform.position;

        //cache the body
        body = GameObject.FindGameObjectWithTag("Player").GetComponent<JelloSpringBody>();

        //subscribe to the JelloCollisionEvent delegate.
        body.JelloCollisionEvent += ProcessCollisionEvent;

        //Set key to activate to upper
        charToActivate = char.ToUpper(charToActivate);
    }

    // Update is called once per frame
    void Update() {
        if (!pulling && CheckKeyDown()) {
           
            //Process the input!
            StartCoroutine(ProcessPull());
        }
    }

    /// <summary>
    /// Check if given key is down and retun the result
    /// </summary>
    /// <returns></returns>
    bool CheckKeyDown() {
        
        switch (charToActivate) {
            case 'A':
                if (Input.GetKey(KeyCode.A)) return true;
                break;
            case 'B':
                if (Input.GetKey(KeyCode.B)) return true;
                break;
            case 'C':
                if (Input.GetKey(KeyCode.C)) return true;
                break;
            case 'D':
                if (Input.GetKey(KeyCode.D)) return true;
                break;
            case 'E':
                if (Input.GetKey(KeyCode.E)) return true;
                break;
            case 'F':
                if (Input.GetKey(KeyCode.F)) return true;
                break;
            case 'G':
                if (Input.GetKey(KeyCode.G)) return true;
                break;
            case 'H':
                if (Input.GetKey(KeyCode.H)) return true;
                break;
            case 'I':
                if (Input.GetKey(KeyCode.I)) return true;
                break;
            case 'J':
                if (Input.GetKey(KeyCode.J)) return true;
                break;
            case 'K':
                if (Input.GetKey(KeyCode.K)) return true;
                break;
            case 'L':
                if (Input.GetKey(KeyCode.L)) return true;
                break;
            case 'M':
                if (Input.GetKey(KeyCode.M)) return true;
                break;
            case 'N':
                if (Input.GetKey(KeyCode.N)) return true;
                break;
            case 'O':
                if (Input.GetKey(KeyCode.O)) return true;
                break;
            case 'P':
                if (Input.GetKey(KeyCode.P)) return true;
                break;
            case 'Q':
                if (Input.GetKey(KeyCode.Q)) return true;
                break;
            case 'R':
                if (Input.GetKey(KeyCode.R)) return true;
                break;
            case 'S':
                if (Input.GetKey(KeyCode.S)) return true;
                break;
            case 'T':
                if (Input.GetKey(KeyCode.T)) return true;
                break;
            case 'U':
                if (Input.GetKey(KeyCode.U)) return true;
                break;
            case 'V':
                if (Input.GetKey(KeyCode.V)) return true;
                break;
            case 'W':
                if (Input.GetKey(KeyCode.W)) return true;
                break;
            case 'X':
                if (Input.GetKey(KeyCode.X)) return true;
                break;
            case 'Y':
                if (Input.GetKey(KeyCode.Y)) return true;
                break;
            case 'Z':
                if (Input.GetKey(KeyCode.Z)) return true;
                break;
            default :
                break;          
        }
        return false;
    }

    /// <summary>
    /// Processes the pull.
    /// </summary>
    /// <returns>IEnumerator.</returns>
    IEnumerator ProcessPull() {
        //Grab the closest point mass and process adjacent points.
        GrabPointMass();

        //could be true later if you are still pulling the body, but do not have a specific point grabbed.
        //this would happen if when you are holding down the mouse button, its position is within the jello body.
        bool waitingForNewGrab = false;

        //keep processing the pull as long as the mouse button is depressed.
        while (CheckKeyDown()) {
        

            //wake up the body if its being grabbed.
            body.IsAwake = true;

            //If our mouse position is outside of the body's transformed base shape
            if (!JelloShapeTools.Contains(body.Shape.EdgeVertices, body.transform.InverseTransformPoint(pullPosition))) {
                //if our mouse position was inside of the base shape last step, grab a new point mass.
                if (waitingForNewGrab) {
                    GrabPointMass();
                    waitingForNewGrab = false;
                }

                //the base shape position (local and global) respective to the selected point mass.
                Vector2 pt = body.Shape.EdgeVertices[pmIndex];
                Vector2 ptGlobal = (Vector2)body.transform.TransformPoint(pt);

                //if we want the body to rotate to align with the pull
                if (rotate) {
                    //find the difference in angle between the two vector
                    Vector2 dir1 = pullPosition - body.Position; 																						//body position to mouse position
                    Vector2 dir2 = (Vector2)body.transform.TransformPoint(body.Shape.EdgeVertices[pmIndex]) - body.Position; 	//body position to xformed base shape position
                    float ang = Vector2.Angle(dir1, dir2);

                    //correct our body angle only a bit at a time for smooth rotations.
                    ang = Mathf.Clamp(ang, 0f, ang * rotateSpeed * Time.fixedDeltaTime);

                    if (JelloVectorTools.CrossProduct(dir1, dir2) < 0f)
                        ang *= -1f;
                    body.Angle -= ang;
                } else {
                    //we dont want the body to rotate to align and will constrain the point mass to a cone.

                    //find the two shape positions next to our selected shape position.
                    Vector2 prev = body.Shape.EdgeVertices[pmIndex > 0 ? pmIndex - 1 : body.Shape.EdgeVertexCount - 1];
                    Vector2 next = body.Shape.EdgeVertices[pmIndex + 1 < body.Shape.EdgeVertexCount ? pmIndex + 1 : 0];

                    //vectors to/from adjacent ponts
                    Vector2 fromPrev = pt - prev;
                    Vector2 toNext = next - pt;
                    //normal created by adjacent vectors
                    //this is the bisector of the angle created by the prev to pt to next vectors
                    //and will be used as the bisector of the constraining cone.
                    Vector2 ptNorm = JelloVectorTools.getPerpendicular(fromPrev + toNext);
                    //correct normal direction by shape winding.
                    ptNorm = body.Shape.winding == JelloClosedShape.Winding.Clockwise ? ptNorm : -ptNorm;

                    //convert to global coordinates
                    ptNorm = (Vector2)body.transform.TransformDirection(ptNorm);
                    //find the angle between the our mouse and the bisector.
                    float ang = Vector2.Angle(ptNorm, pullPosition - ptGlobal);

                    //if we exceed the constraint of the cone.
                    if (ang > coneAngle * 0.5f) //0.5 because the bisector cuts the cone angle in half. 
                    {
                        //find the vector representing the edge of the cone
                        Vector2 limitVector;
                        if (JelloVectorTools.CrossProduct(ptNorm, pullPosition - ptGlobal) < 0f)//which side of the bisector are we on
                            limitVector = JelloVectorTools.rotateVector(ptNorm, -coneAngle * 0.5f);
                        else
                            limitVector = JelloVectorTools.rotateVector(ptNorm, coneAngle * 0.5f);

                        //move our position to the closest point on the limit vector. Handle max pull back at the same time.
                        pullPosition = JelloVectorTools.getClosestPointOnSegment(pullPosition, ptGlobal, ptGlobal + limitVector.normalized * maxPullBack);
                    }
                }

                //how far away from xformed base shape position we are.
                pullBackDistance = (pullPosition - ptGlobal).sqrMagnitude;
                if (pullBackDistance != 0f) {
                    //if we exceed the max pullback, set to the max pullback.
                    if (pullBackDistance > maxPullBack * maxPullBack) {
                        pullPosition = ptGlobal + (pullPosition - ptGlobal).normalized * maxPullBack; // still do this when angle is not right.
                        pullBackDistance = maxPullBack * maxPullBack;
                    }
                }

                //explicitly set the selected pointmass position and velocity.
                pointmass.Position = pullPosition;
                pointmass.velocity = Vector2.zero;
            } else//our mouse is down, but is inside the perimeter of the body. 
            {
                ReleasePointMass();//release the selected point mass and restore its ajacent point masses
                waitingForNewGrab = true;//wait for a new grab. would occur if the mouse was dragged outside of the body again. 
            }

            //sync up with fixed update
            yield return new WaitForFixedUpdate();

            Vector2 direction = ((Vector2)body.transform.TransformPoint(body.Shape.EdgeVertices[pmIndex]) - pointmass.Position).normalized;
            body.AddForce(direction * pullBackDistance * force );
            body.AddForce(direction * pullBackDistance * force , body.Shape.EdgeVertices[pmIndex], true);


        }

        //mouse button has been released!
        //release the selected point mass.
        ReleasePointMass();
        //release the body and apply force if we had a point mass selected at the time.
        ReleaseBody(!waitingForNewGrab);
    }


    /// <summary>
    /// Releases the body from a pull.
    /// </summary>
    /// <param name="addForce">If set to <c>true</c> add force.</param>
    public void ReleaseBody(bool addForce) {
        //allow the body to move freely again
        body.IsKinematic = false;
        //no longer pulling thebody
        pulling = false;

    }


    /// <summary>
    /// Grabs the point mass.
    /// </summary>
    public void GrabPointMass() {
        //i am now pulling the body.
        pulling = true;

        //clear old values from last pull
        adjacentPointMasses.Clear();
        oldMultipliers.Clear();

        //set the position to lock it in place while pulling the body
        position = body.Position;

        //get the closest point mass to the mouse positions and cache some information about it.
        pmIndex = body.getClosestPointMass(pullPosition, false); //only grab edge point masses
        pointmass = body.getEdgePointMass(pmIndex);
        oldMultipliers.Add(pointmass.shapeMatchingMultiplier);
        pointmass.shapeMatchingMultiplier = 0f;
        adjacentPointMasses.Add(pointmass);


        int adjIndex = 0;
        int numFromStart = 0;
        //grab adjacent point masses further in the array.
        for (int i = pmIndex; i < pmIndex + numAdjacentPoints; i++) {
            adjIndex = i > body.EdgePointMassCount - 1 ? i - body.EdgePointMassCount : i;

            if (adjIndex > body.EdgePointMassCount - 1 || adjacentPointMasses.Contains(body.getEdgePointMass(adjIndex)))
                continue;

            //cache the point mass info and modify the shape matching values by how close it is to the grabbed point mass
            adjacentPointMasses.Add(body.getEdgePointMass(adjIndex));
            oldMultipliers.Add(body.getEdgePointMass(adjIndex).shapeMatchingMultiplier);
            numFromStart++;
            body.getEdgePointMass(adjIndex).shapeMatchingMultiplier = numFromStart / (numAdjacentPoints + 1 + adjacentDropoff);
        }
        numFromStart = 0;
        //grab adjacent point masses before the current index in the array.
        for (int i = pmIndex; i > pmIndex - numAdjacentPoints; i--) {
            adjIndex = i < 0 ? i + body.EdgePointMassCount : i;
            if (adjIndex < 0 || adjacentPointMasses.Contains(body.getEdgePointMass(adjIndex)))
                continue;

            //cache the point mass info and modify the shape matching values by how close it is to the grabbed point mass
            adjacentPointMasses.Add(body.getEdgePointMass(adjIndex));
            oldMultipliers.Add(body.getEdgePointMass(adjIndex).shapeMatchingMultiplier);
            numFromStart++;
            body.getEdgePointMass(adjIndex).shapeMatchingMultiplier = numFromStart / (numAdjacentPoints + 1 + adjacentDropoff);
        }
        //find any internal point masses connected to the selected point mass (via spring) and cache/modify its shape matching multiplier.
        for (int i = 0; i < body.SpringCount; i++) {
            if (body.getSpring(i).pointMassA == pmIndex || body.getSpring(i).pointMassB == pmIndex) {
                if (body.getSpring(i).pointMassA >= body.EdgePointMassCount) {
                    adjacentPointMasses.Add(body.getPointMass(body.getSpring(i).pointMassA));
                    oldMultipliers.Add(body.getPointMass(body.getSpring(i).pointMassA).shapeMatchingMultiplier);
                    body.getPointMass(body.getSpring(i).pointMassA).shapeMatchingMultiplier = 1 / (2 + adjacentDropoff);
                } else if (body.getSpring(i).pointMassB >= body.EdgePointMassCount) {
                    adjacentPointMasses.Add(body.getPointMass(body.getSpring(i).pointMassB));
                    oldMultipliers.Add(body.getPointMass(body.getSpring(i).pointMassB).shapeMatchingMultiplier);
                    body.getPointMass(body.getSpring(i).pointMassB).shapeMatchingMultiplier = 1 / (2 + adjacentDropoff);
                }
            }
        }
    }


    /// <summary>
    /// Releases the point mass.
    /// </summary>
    public void ReleasePointMass() {
        //restore all values back to their pre-pull values.
        for (int i = 0; i < adjacentPointMasses.Count; i++)
            adjacentPointMasses[i].shapeMatchingMultiplier = oldMultipliers[i];
    }

    /// <summary>
    /// Processes the collision event.
    /// </summary>
    /// <param name="jelloCollision">Jello collision.</param>
    private void ProcessCollisionEvent(JelloCollision jelloCollision) {
        //only process events if we are pulling and adjacent points need to ignore collisions temporarily 
        // -OR-
        //we need to calculate if the body is grounded.
        if ((!adjacentPointsIgnoreCollision || !pulling) && (!requireGrounded || grounded))
            return;

        //loop throug each contact in the collision.
        for (int i = 0; i < jelloCollision.contacts.Length; i++) {
            //adjacent ignore collisions
            if (adjacentPointsIgnoreCollision && pulling) {
                if (body == jelloCollision.contacts[i].bodyA) {
                    //if the penetrating point of this contact is one of our adjacent points, ignore this contact!
                    if (adjacentPointMasses.Contains(jelloCollision.contacts[i].bodyA.getPointMass(jelloCollision.contacts[i].bodyApm)))
                        jelloCollision.contacts[i].ignore = true;
                } else {
                    //if either point of the penetrated edge of htis contact is one of our adjacent points, ignore this contact!
                    if (adjacentPointMasses.Contains(jelloCollision.contacts[i].bodyB.getPointMass(jelloCollision.contacts[i].bodyBpmA)) ||
                       adjacentPointMasses.Contains(jelloCollision.contacts[i].bodyB.getPointMass(jelloCollision.contacts[i].bodyBpmB))) {
                        jelloCollision.contacts[i].ignore = true;
                    }
                }
            }

            //grounded logic
            if (!grounded && !jelloCollision.contacts[i].ignore) {
                //if the hit point of this contact is below the center of the body, count as grounded.
                //This works well for the JelloCharacter in the SpiffyDemoScene but may not be the best for all cases.
                if (jelloCollision.contacts[i].hitPoint.y < transform.position.y) {
                    //clear the number of air jumps performed and set grounded to true.
                    grounded = true;

                    //if we dont need to look for collisions to ignore, then we could break because we now know that at least one point of our body is grounde.
                    if (!adjacentPointsIgnoreCollision || !pulling)
                        break;
                }
            }
        }
    }

}
