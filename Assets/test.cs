using Fusion;
using UnityEngine;

public class test : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var mainCamera = Camera.main;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray,out var hit,Mathf.Infinity))
        {
            Debug.Log(hit.transform.name);

        }
        else
        {
            Debug.Log("No Hit");
        }
    }
}
