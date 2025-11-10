using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform target; // The object to orbit around
    public float orbitSpeed; // Speed of orbiting
    Vector3 offset; // Offset from the target object
//  target(Transform) : 수류탄들이 공전할 중심점으로, 플레이어의 Transform이 할당됩니다.
//? speed(float) : 공전하는 속도를 정하는 변수입니다.
//? offset (Vector3): 플레이어와 수류탄 사이의 거리를 나타내는 변수입니다.
    void Start()
    {
        offset = transform.position - target.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = target.position + offset;
        transform.RotateAround(target.position, Vector3.up, orbitSpeed * Time.deltaTime);
        offset = transform.position - target.position;
    }
}
