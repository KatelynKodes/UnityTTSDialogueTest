using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floatingDialogueBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    private Transform _mainCameraPosition;
    private Transform _characterPosition;

    [SerializeField]
    private Vector3 _offset;

    void Start()
    {
        _mainCameraPosition = Camera.main.transform;
        _characterPosition = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        // Makes the box always face the position of the camera.
        transform.rotation = Quaternion.LookRotation(transform.position - _mainCameraPosition.position);

        //Make the box move with the character its attached to
        transform.position = _characterPosition.position + _offset;
    }
}
