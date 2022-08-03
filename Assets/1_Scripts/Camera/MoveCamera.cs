using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    [SerializeField] float speed = 1f;
    [SerializeField] float goal = 10f;

    bool goalReached = false;
    bool pause = false;

    private void Update()
    {
        if(goalReached || pause) return;

        transform.position += Vector3.right * speed * Time.deltaTime;

        if(transform.position.x > goal)
            goalReached = true;
    }

    public void IncreaseSpeed(float amount)
    {

    }

    public void DecreaseSpeed(float amount)
    {

    }

    public void Pause()
    {
        pause = true;
    }
}
