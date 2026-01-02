using System;
using Unity.VisualScripting;
using UnityEngine;

public class GlobalTime : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float TotalTime=120;
    
    private float eventTime=20;
    private float reductionAmont=2;
    private bool IsEvenTime=false;
    public event Action OnTimeRached;
    [SerializeField] private float currentTime;


    // Update is called once per frame
    void Start()
    {
        currentTime= TotalTime;
    }
    void Update()
    {
        Timer();      
    }
    void Timer()
    {
        currentTime-= Time.deltaTime;
        //eventTime-=Time.deltaTime;
        if (currentTime % eventTime == 0)
        {
            Debug.Log("[GlabalTime] evenTime");
            OnTimeRached.Invoke();
        }  
    }
    // public  static class getTiemer
    // {
        
    // }
}
