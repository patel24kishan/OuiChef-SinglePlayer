using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour,IKitchenObjectParent
{
    
    public static Player Instance { get; private set; }

    public event EventHandler onKitchenObjectPickup;
    public event EventHandler<OnSelectedCounterChangedEventArgs> OnSelectedCounterChanged;
    public class OnSelectedCounterChangedEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }
    
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotateSpeed = 10f;
    [SerializeField] private LayerMask counterLayerMask;
    [SerializeField] private GameInput gameInput;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    
    private KitchenObject kitchenObject;

    private Vector3 lastInteractDir;
    private bool isWalking;
    private BaseCounter selectedCounter;


    private void Awake()
    {
        if (Instance!=null)
        {
            Debug.LogError("There is more than one player Instance ");
        }
        Instance = this;
    }

    private void Start()
    {
    gameInput.OnInteractAction += GameInput_OnInteractAction;
    gameInput.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }
    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        
        if (selectedCounter != null)
        {
          selectedCounter.Interact(this);   
        }
    }
    private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e)
    {
        if (!KitchenGameManager.Instance.IsGamePlaying())
        {
            return;
        }
        
        if (selectedCounter != null)
        {
            selectedCounter.InteractAlternate(this);   
        }
    }
    private void Update()
    {
       HandleMovement();
       HandleInteraction();
    }

    public bool IsWalking()
    {
        return isWalking;
    }

    private void HandleInteraction()
    {
        Vector2 inputVector = gameInput.GetMovementInputNormalized();

        //Player Move
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero)
        {
            lastInteractDir = moveDir;
        }

        float inteactDistance = 2f;
        if (Physics.Raycast(transform.position, lastInteractDir, out RaycastHit raycasthit, inteactDistance,counterLayerMask))
        {
            if (raycasthit.transform.TryGetComponent(out BaseCounter baseCounter))
            {
                if (baseCounter!=selectedCounter)
                {
                   SetSelectedCounter(baseCounter);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        } else
        {
            SetSelectedCounter(null);
        }
        //Debug.Log(selectedCounter);
    }

    private void HandleMovement()
    {
        Vector2 inputVector = gameInput.GetMovementInputNormalized();

        //Player Move
        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            //Attempt only x
            Vector3 moveDirX = new Vector3(moveDir.x, 0f, 0).normalized;
            canMove = (moveDir.x< -0.5f || moveDir.x>0.5f) && !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius, moveDirX, moveDistance);

            if (canMove)
            {
                //can only move in X
                moveDir = moveDirX;
            }
            else
            {
                //Attempt only  Z
                Vector3 moveDirZ = new Vector3(0f, 0f, moveDir.z).normalized;
                canMove =  (moveDir.z< -0.5f || moveDir.z>0.5f) && !Physics.CapsuleCast(transform.position,transform.position+Vector3.up*playerHeight,playerRadius, moveDirZ, moveDistance);
                
                if (canMove)
                {
                    //can only move in z
                    moveDir = moveDirZ;
                }else
                {
                    //Cannot move in any direction
                }
            }
            
        }
        
        if (canMove)
        {
            transform.position += moveDir *moveDistance;
        }

        //set isWalking
        isWalking = moveDir != Vector3.zero;
        
        //Player Rotate
        transform.forward = Vector3.Slerp(transform.forward,moveDir,Time.deltaTime*rotateSpeed); 
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;
        OnSelectedCounterChanged?.Invoke(this,new OnSelectedCounterChangedEventArgs
        {
            selectedCounter = selectedCounter
        });
    }

    public Transform GetkitchenobjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        if (kitchenObject!=null)
        {
            onKitchenObjectPickup?.Invoke(this,EventArgs.Empty);
        }
        
        this.kitchenObject = kitchenObject;
    }


    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }


    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }
}
