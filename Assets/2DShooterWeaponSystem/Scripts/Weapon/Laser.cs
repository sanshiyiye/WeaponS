using Unity.VisualScripting;
using UnityEngine;

public class Laser : Bullet
{

    void Awake()
    {
        bulletSpriteRenderer = GetComponent<LineRenderer>();
    }


    public override float BulletXPosition
    {
        get
        {
            // 自定义get逻辑
            return base.bulletXPosition ;
        }
        set
        {
            // 自定义set逻辑
            base.bulletXPosition = value;
            
        }
    }

    public override float BulletYPosition
    {
        get
        {
            // 自定义get逻辑
            return base.bulletYPosition ;
        }
        set
        {
            // 自定义set逻辑
            base.bulletYPosition = value;
            
        }
        
    }
    
    public override float DirectionAngle
    {
        get
        {
            // 自定义get逻辑
            return base.directionAngle ;
        }
        set
        {
            // 自定义set逻辑
            base.directionAngle = value;
            
        }
        
    }

    
    public static (float, float) MoveToNewPosition(Vector2 startPos, float distance, float directionAngle)
    {
        // 将角度转换为弧度
        float directionAngleRad = directionAngle *  Mathf.Rad2Deg;

        // 计算新的位置
        float newX = startPos.x + distance * Mathf.Cos(directionAngleRad);
        float newY = startPos.y + distance * Mathf.Sin(directionAngleRad);

        return (newX, newY);
    }
    
    private Vector3 startPos;

    private Vector3 endPos;
    
    public override void SetBulletPosition(float x, float y)
    {
        bulletXPosition = x;
        bulletYPosition = y;
        startPos = new Vector3(bulletXPosition, bulletYPosition, 0f);
        ((LineRenderer)bulletSpriteRenderer).SetPosition(0, startPos);
        // var (endX, endY) =  MoveToNewPosition(startPos, speed, directionAngle);
        // endPos.x = endX;
        // endPos.y = endY;
        endPos.x = startPos.x + Mathf.Cos(directionAngle) * speed ;
        endPos.y = startPos.y + Mathf.Sin(directionAngle) * speed;
        ((LineRenderer)bulletSpriteRenderer).SetPosition(1, endPos);

    }
    
    public void SetEnable(bool b)
    {
        bulletSpriteRenderer.enabled = b;
    }

    public void SetPositions(Vector3 start, Vector3 end)
    {
        ((LineRenderer)bulletSpriteRenderer).SetPosition(0, start);
        ((LineRenderer)bulletSpriteRenderer).SetPosition(1, end);
    }

    void Update()
    {
        // Debug.DrawRay(startPos, 100 * directionAngle * Vector3.one, Color.red );
    }
}
