using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
  public LayerMask castingMask;

  public float walkSpeed = 7;

  public float mouseSensitivityX = 1;
  public float mouseSensitivityY = 1;

  public float clampLookY = 90;
  public bool invertLookY = false;
  public float mouseSnappiness = 20;
  public float gravity = -9.81f;

  public bool isGrounded = false;
  public bool inputKeyJump = false;
  public float jumpHeight = 2.5f;

  public float groundCheckY = 0.33f;
  float groundOffsetY = 0;

  public float sphereCastRadius = 0.25f;
  public float sphereCastDistance = 0.75f;



  CharacterController controller;
  Camera cam;

  float xRotation = 0;
  float inputLookX = 0;
  float inputLookY = 0;
  float inputMoveX = 0;
  float inputMoveY = 0;
  float lastSpeed = 0;
  Vector3 lastPos = Vector3.zero;

  float accMouseX = 0;
  float accMouseY = 0;

  Vector3 fauxGravity = Vector3.zero;



  // Start is called before the first frame update
  void Start()
  {
    controller = GetComponent<CharacterController>();
    cam = Camera.main;
    lastPos = transform.position;
    fauxGravity = Vector3.up * gravity;
    groundOffsetY = groundCheckY;

    Cursor.lockState = CursorLockMode.Locked;
  }

  void ProcessInputs()
  {
    inputLookX = Input.GetAxis("Mouse X");
    inputLookY = Input.GetAxis("Mouse Y");

    inputMoveX = Input.GetAxis("Horizontal");
    inputMoveY = Input.GetAxis("Vertical");

    inputKeyJump = Input.GetAxis("Jump") > 0 ? true : false;

  }

  void ProcessLook()
  {
    accMouseX = Mathf.Lerp(accMouseX, inputLookX, mouseSnappiness * Time.deltaTime);
    accMouseY = Mathf.Lerp(accMouseY, inputLookY, mouseSnappiness * Time.deltaTime);

    float mouseX = accMouseX * mouseSensitivityX * 100.0f * Time.deltaTime;
    float mouseY = accMouseY * mouseSensitivityY * 100.0f * Time.deltaTime;

    xRotation += (invertLookY == true ? mouseY : -mouseY);
    xRotation = Mathf.Clamp(xRotation, -clampLookY, clampLookY);

    cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
    transform.Rotate(Vector3.up * mouseX);

  }

  void ProcessMovement()
  {
    Vector3 calc;
    Vector3 move;
    float nextSpeed = walkSpeed;
    float currentSpeed = (transform.position - lastPos).magnitude / Time.deltaTime;
    currentSpeed = (currentSpeed < 0 ? 0 - currentSpeed : currentSpeed);

    //ground check
    GroundCheck();
    //slipping check
    //ceiling check

    //if sliding
    //if not:
    move = (transform.right * inputMoveX) + (transform.forward * inputMoveY);
    move = Vector3.ClampMagnitude(move, 1);


    float speed = 0;

    if (isGrounded)
    {
      fauxGravity.x = 0;
      fauxGravity.z = 0;

      if (fauxGravity.y < 0)
      {
        fauxGravity.y = Mathf.Lerp(fauxGravity.y, -1, 4 * Time.deltaTime);
      }

      if (inputKeyJump) {
        fauxGravity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
      }

      float lerpFactor = (lastSpeed > nextSpeed ? 4 : 2);
      speed = Mathf.Lerp(lastSpeed, nextSpeed, lerpFactor * Time.deltaTime);
    }
    else
    {
      speed = Mathf.Lerp(lastSpeed, nextSpeed, 0.125f * Time.deltaTime);
    }



    lastSpeed = speed;

    fauxGravity.y += gravity * Time.deltaTime;

    calc = move * speed * Time.deltaTime;
    calc += fauxGravity * Time.deltaTime;

    controller.Move(calc);
  }


  void GroundCheck()
  {
    Vector3 origin = new Vector3(transform.position.x, transform.position.y + groundOffsetY, transform.position.z);
    RaycastHit hit;

    if (Physics.SphereCast(origin, sphereCastRadius, Vector3.down, out hit, sphereCastDistance, castingMask))
    {
      isGrounded = true;
    }
    else
    {
      isGrounded = false;
    }
  }


  // Update is called once per frame
  void Update()
  {
    ProcessInputs();
    ProcessLook();
    ProcessMovement();
  }

  public Vector3Int GetChunkPosition() {
    Vector3 p = controller.transform.position / MarchingData.width;
    return new Vector3Int((int)p.x, (int)p.y, (int)p.z);
  }

  public Vector3 GetPosition() {
    return controller.transform.position;
  }
}
