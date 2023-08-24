using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.UI;

public class FloatingDialogueBoxBehavior : MonoBehaviour
{
    private Canvas _screenCanvas;
    private Transform _parentTransform;
    private Camera _camera;

    [SerializeField]
    private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        _screenCanvas = FindObjectOfType<Canvas>();
        _camera = FindObjectOfType<Camera>();
        _parentTransform = this.transform.parent;

        this.transform.SetParent(_screenCanvas.transform);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _camera.transform.position);
        transform.position = _parentTransform.position + offset;
    }
}
