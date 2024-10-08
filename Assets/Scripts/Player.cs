using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 7f;

    private bool isWalking;

    private void Update()
    {
        Vector2 inputVector = new Vector2(0, 0);

        if(Input.GetKey(KeyCode.W)){
            inputVector.y = 1;
        }

        if(Input.GetKey(KeyCode.S)){
            inputVector.y = -1;
        }

        if(Input.GetKey(KeyCode.A)){
            inputVector.x = -1;
        }

        if(Input.GetKey(KeyCode.D)){
            inputVector.x = 1;
        }

        inputVector = inputVector.normalized;

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);
        transform.position += moveDir * moveSpeed * Time.deltaTime ;

        // transform.rotation 이것을 쓰게되면 Quaternion어쩌고 를 계산해야하고 복잡하므로 아래 방법을 사용
        // transform.forward = moveDir;

        isWalking = moveDir != Vector3.zero;
        // 위 처럼 코드 작성시 너무 180회전이 스무스하지 않으므로 아래 코드 사용
        float rotateSpeed = 10f;
        transform.forward = Vector3.Slerp(transform.forward, moveDir, Time.deltaTime * rotateSpeed);

    }

    public bool IsWalking(){
        return isWalking;
    }


}
