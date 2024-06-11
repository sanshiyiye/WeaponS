using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Laser : Bullet
{

    // protected override void  Start()
    // {
        // bulletSpriteRenderer = GetComponent<LineRenderer>();
    // }

    private Transform startParticles;
    
    private Transform endParticles;


    private Collider2D edge;
    public override void Init()
    {
        bulletSpriteRenderer = GetComponent<LineRenderer>();
        
        startParticles = transform.Find("StartParticles");
        startParticles.gameObject.SetActive(true);
        var startParticleSystem = startParticles.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < startParticleSystem.Length; i++)
        {
            startParticleSystem[i].Play();
            
        }
        endParticles = transform.Find("EndParticles");
        endParticles.gameObject.SetActive(true);
        var endParticleSystem = endParticles.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < endParticleSystem.Length; i++)
        {
            endParticleSystem[i].Play();

        }
        
        
        isAlive = true;
        
        edge = GetComponent<EdgeCollider2D>();   //得到物体身上的EdgeCollider2D组件
        
        
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

    private Vector3 startPos;

    private Vector3 endPos;
    
    public override void SetBulletPosition(float x, float y)
    {
        bulletXPosition = x;
        bulletYPosition = y;
        //-------shooting------
        startPos.x = bulletXPosition;
        startPos.y = bulletYPosition;
        
        var directionAngle = gunPoint.AimAngle+ bulletSpreadAngle;
        endPos.x = startPos.x + Mathf.Cos(directionAngle) * speed ;
        endPos.y = startPos.y + Mathf.Sin(directionAngle) * speed;

        var linePos = new List<Vector2>();
        linePos.Add(Vector2.zero);
        var distance = Vector3.Distance(endPos, startPos);
        linePos.Add( new Vector3(distance, 0));
        ((EdgeCollider2D)edge).points = linePos.ToArray();    //将存起来的二维向量点赋值到碰撞器实现碰撞器初始化
        UpdateShootingPos();
    }
    
    public void SetEnable(bool b)
    {
        bulletSpriteRenderer.enabled = b;
    }

    public void UpdateShootingPos()
    {
        ((LineRenderer)bulletSpriteRenderer).SetPosition(0, startPos);
        var dir = (endPos - startPos).normalized;

        if (startParticles)
        {
            startParticles.position = startPos;
            startParticles.transform.right = dir;
        }
        
        ((LineRenderer)bulletSpriteRenderer).SetPosition(1, endPos);
        if (endParticles)
        {
            endParticles.position = endPos;
            endParticles.transform.right = dir;

        }

      
    }

    private ContactPoint2D? collision;
    void OnCollisionEnter2D(Collision2D target)
    {
        Debug.Log("==========================type:====" + target.collider.GetType());
        // 检查是否与EdgeCollider2D发生碰撞
        foreach (ContactPoint2D contact in target.contacts)
        {
            // 获取每个接触点的位置
            collision = contact;

        }
    }
    void OnCollisionExit2D(Collision2D target)
    {
        if (target != null&& collision != null && collision.Value.collider == target.collider)
        {
            collision = null;

        }
    }
    
    // void OnTriggerEnter2D(Collider2D other)
    // {
    //     // 检查进入触发器的对象是否有特定的标签（可选）
    //     if (other.CompareTag("Wall") || other.CompareTag("Obj"))
    //     {
    //         Debug.Log("Player entered the trigger area");
    //         // 在这里添加你的处理逻辑
    //         
    //     }
    //     else
    //     {
    //         Debug.Log("Something else entered the trigger area");
    //     }
    // }
    
    void Update()
    {
        if (!isAlive)
            return;
        // Debug.DrawRay(startPos, 100 * directionAngle * Vector3.one, Color.red );
        if (gunPoint != null)
        {
            startPos = gunPoint.point.transform.position;
            // ((LineRenderer)bulletSpriteRenderer).SetPosition(0, startPos);

            if (collision != null)
            {
                endPos = collision.Value.point;
                
            }
            else
            {
                var directionAngle = gunPoint.AimAngle+ bulletSpreadAngle;
                endPos.x = startPos.x + Mathf.Cos(directionAngle) * speed ;
                endPos.y = startPos.y + Mathf.Sin(directionAngle) * speed;
            }

            // ((LineRenderer)bulletSpriteRenderer).SetPosition(1, endPos);
            transform.position = startPos;
            
            transform.eulerAngles = new Vector3(0.0f, 0.0f,  directionAngle * Mathf.Rad2Deg);
            
            UpdateShootingPos();
        }
        //Debug.DrawRay(gunPoint.point.position,10*(endPos-startPos),Color.red);
    }
}
