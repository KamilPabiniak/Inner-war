using System;
using UnityEngine;

public class CameraMenuController : MonoBehaviour
{
    [Header("Settings")]
    public float maxTiltX = 13f;
    public float maxTiltY = 26f; 
    public float smoothSpeed = 5f; 
    
    [Header("Center zone")]
    public float centerZoneSize = 100f; 
    
    private Quaternion initialRotation; 
    private bool tiltEnabled = true;     
    private bool returning;
    
    void Start()
    {
        initialRotation = transform.rotation;
    }

    void Update()
    {
        if (returning)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation, 
                initialRotation, 
                smoothSpeed * Time.deltaTime
            );
            
            if (Quaternion.Angle(transform.rotation, initialRotation) < 0.1f)
            {
                transform.rotation = initialRotation;
                returning = false;
                tiltEnabled = false;
            }
            return;
        }
        
        if (!tiltEnabled) 
            return;
        
        Vector3 mousePos = Input.mousePosition;
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float centerX = screenWidth / 2f;
        float centerY = screenHeight / 2f;
        
        if (Mathf.Abs(mousePos.x - centerX) < centerZoneSize / 2f &&
            Mathf.Abs(mousePos.y - centerY) < centerZoneSize / 2f)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, initialRotation, smoothSpeed * Time.deltaTime);
            return;
        }
        
        float offsetX = (mousePos.x - centerX) / (screenWidth / 2f);
        float offsetY = (mousePos.y - centerY) / (screenHeight / 2f);
        
        Quaternion targetTilt = Quaternion.Euler(-offsetY * maxTiltX, offsetX * maxTiltY, 0f);
        Quaternion targetRotation = initialRotation * targetTilt;
        
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
    
    public void StopAndReturnToInitial()
    {
        tiltEnabled = false;
        returning = true;
    }
}