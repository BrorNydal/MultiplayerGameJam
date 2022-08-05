using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;


public enum HitDirection
{
    None,
    Left, 
    Right,
    Up,
    Down,
}

public class PlayerHit : MonoBehaviour
{
    [SerializeField] HitDirection requiredDirection;

    [Space, SerializeField] UnityEvent<Collision2D, HitDirection> HitEvent;
    [SerializeField] UnityEvent<Collision2D, HitDirection> OnAnimationComplete;

    Tween hitAnimation = null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Vector2 hitNormal = -collision.GetContact(0).normal;
        HitDirection dir = HitDirection.Right;
        if (hitNormal.y > 0.9f)
            dir = HitDirection.Down;
        else if (hitNormal.y < -0.9f)
            dir = HitDirection.Up;
        else if (hitNormal.x > 0.9f)
            dir = HitDirection.Left;

        Hit(collision, dir);
    }

    private void Hit(Collision2D collision, HitDirection direction)
    {
        if (requiredDirection != HitDirection.None && direction != requiredDirection) return;

        HitEvent?.Invoke(collision, direction);
    }

    public void PlayHitAnimation(Collision2D collision, HitDirection direction)
    {
        Vector2 to = Vector2.zero;
        switch (direction)
        {
            case HitDirection.Left:
                to = Vector2.left;
                break;
            case HitDirection.Right:
                to = Vector2.right;
                break;
            case HitDirection.Up:
                to = Vector2.up;
                break;
            case HitDirection.Down:
                to = Vector2.down;
                break;
        }

        hitAnimation = transform.DOLocalMove(transform.localPosition + (Vector3)to, 0.1f).SetEase(Ease.Flash).SetLoops(2, LoopType.Yoyo).OnComplete(() =>
        {
            OnAnimationComplete?.Invoke(collision, direction);
            hitAnimation.Kill();
        });
    }
}
