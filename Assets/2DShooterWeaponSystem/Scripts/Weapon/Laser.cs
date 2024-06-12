using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Laser : Bullet
{
    
    private Transform startParticles;
    
    private Transform endParticles;
    
    private Collider2D edge;
    
    public override void Init()
    {
        bulletSpriteRenderer = GetComponent<LineRenderer>();

        // if (circularFireMode)
        // {
        //     startParticles = transform.Find("StartParticles");
        //     startParticles.gameObject.SetActive(true);
        //     var startParticleSystem = startParticles.GetComponentsInChildren<ParticleSystem>();
        //     for (int i = 0; i < startParticleSystem.Length; i++)
        //     {
        //         startParticleSystem[i].Play();
        //     
        //     }
        // }
        // else
        {
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
        }
        
        
        
        isAlive = true;
        
        edge = GetComponent<EdgeCollider2D>();   //得到物体身上的EdgeCollider2D组件
        ((EdgeCollider2D)edge).edgeRadius = 0.1f;
        
        if (circularFireMode)
        {
            bulletCircleRadius = 5f;
            bulletCircleWidth = 0.5f;
            segmentAmount = CalcSegmentAmount(360, bulletCircleWidth, bulletCircleRadius);
            linePos = new Vector2[PointAmount];
            

        }
        else
        {
            segmentAmount = 1;
            linePos = new Vector2[2];
        }
        
        var lineRenderer = ((LineRenderer)bulletSpriteRenderer);
        lineRenderer.positionCount = PointAmount;
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 1.0f;

        hasCollision = false;
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
    
    
    private Vector2 [] linePos ;

    public int segmentAmount = 100;
    private int PointAmount { get { return segmentAmount + 1; } }

    private float startAngle = 0f;

    private Vector2 startPos;
    
    private float angleSum = 360f;
    
    public override void SetBulletPosition(float x, float y)
    {
        bulletXPosition = x;
        bulletYPosition = y;
        
        if (circularFireMode)
        {
            SetCirclePointsPos(ref linePos,out startPos,gunPoint.point.transform.position,startAngle,angleSum, bulletCircleRadius, PointAmount);
        }
        else
        {
            SetLinePointsPos(ref linePos,gunPoint.point.transform.position,directionAngle,speed);

        }
        UpdateShootingPos();
        UpdateParticlesPos();
        UpdateEdgeCollider();
    }
    
    // 根据起始角度、弧度、弧半径、构成弧的各点的数量，算得各点的位置
    void SetCirclePointsPos(ref Vector2[] posArray, out Vector2 startPos, Vector2 center,
        float startAngle, float angleSum, float radius, int pointAmount)
    {
        // Vector2[] posArray = new Vector2[pointAmount];
        Vector2 currentPointPos = Vector2.zero;

        // 计算起点的角度(-90表示从右边中间为起点开始画弧)
        float angle = startAngle - 90;

        // float angle = startAngle;
        for (int i = 0; i < pointAmount; i++)
        {
            currentPointPos.x = center.x + Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            currentPointPos.y = center.y + Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            
            posArray[i] = currentPointPos;
            angle += angleSum / segmentAmount;
        }

        float initialAngle = startAngle - 90;

// 计算startAngle对应点的位置
        startPos = new Vector2(
            center.x + Mathf.Sin(Mathf.Deg2Rad * initialAngle) * radius,
            center.y + Mathf.Cos(Mathf.Deg2Rad * initialAngle) * radius
        );
    }
    // 根据弧度、弧线粗细、弧半径，计算片段的数量
    int CalcSegmentAmount(float angleSum, float width, float radius)
    {
        // 片段数量 = 弧长/片段长度， 片段长度=弧宽度
        float arcLength = Mathf.Deg2Rad * angleSum * radius;
        int amount =(int)(arcLength / width);
        return amount;
    }
    // 用linerenderer画弧,用EdgeCollider2D构建碰撞体
    void SetLinePointsPos(ref Vector2[] linePos,Vector3 gunPosition, float directionAngle, float length)
    {
        // linePos[0] = Vector2.zero;
        linePos[0].x = gunPosition.x;
        linePos[0].y = gunPosition.y;
            
        // linePos[1] = Vector2.zero;
        linePos[1].x = linePos[0].x + Mathf.Cos(directionAngle) * speed ;
        linePos[1].y = linePos[0].y + Mathf.Sin(directionAngle) * speed;
    }
    
    void UpdateEdgeCollider()
    {
        // 获取LineRenderer的点数
        var lineRenderer = ((LineRenderer)bulletSpriteRenderer);
        int pointCount = lineRenderer.positionCount;
        Vector3[] positions = new Vector3[pointCount];
        lineRenderer.GetPositions(positions); // 获取所有点的位置

        // 将Vector3数组转换为Vector2数组
        Vector2[] edgeColliderPoints = new Vector2[pointCount];
        for (int i = 0; i < pointCount; i++)
        {
            // 转换为局部坐标
            Vector3 localPos = transform.InverseTransformPoint(positions[i]);
            edgeColliderPoints[i] = new Vector2(localPos.x, localPos.y);
        }


        // 设置EdgeCollider2D的点
        ((EdgeCollider2D)edge).points = edgeColliderPoints;
    }
    
    public void SetEnable(bool b)
    {
        bulletSpriteRenderer.enabled = b;
    }

    public void UpdateShootingPos()
    {

        for (int i = 0; i < linePos.Length; i++)
        {
            ((LineRenderer)bulletSpriteRenderer).SetPosition(i, linePos[i]);
        }
        
      
    }

    public void UpdateParticlesPos()
    {
        if (!circularFireMode)
        {
            var dir = (linePos[1] - linePos[0]).normalized;

            if (startParticles)
            {
                startParticles.position = linePos[0];
                startParticles.transform.right = dir;
            }
        
            if (endParticles)
            {
                endParticles.position = linePos[1];
                endParticles.transform.right = dir;

            }
        }
        else
        {
            if (startParticles && startPos != null)
            {
                startParticles.position = startPos;
            }

            if (endParticles && collisionPoint != null)
            {
                endParticles.position = collisionPoint.Value;
            }
        }
    }
    
    private Vector3? collisionPoint;

    private bool hasCollision;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // 如果发生碰撞，存储碰撞点信息

        // 获取碰撞信息
        ContactPoint2D[] contacts = new ContactPoint2D[10]; // 假设最多有10个接触点
        int contactCount = edge.GetContacts(contacts);
        if(contactCount > 0) hasCollision = true;
        for (int i = 0; i < contactCount; i++)
        {
            // contacts[i] 是一个ContactPoint2D结构，包含了碰撞点和法线信息
            Vector2 point = contacts[i].point; // 碰撞点的世界坐标

            collisionPoint = point;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        hasCollision = true;
        foreach (ContactPoint2D contact in collision.contacts)
        {
            collisionPoint = contact.point;
        }
    }
    
    void Update()
    {
        if (!isAlive) return;
        // Debug.DrawRay(startPos, 100 * directionAngle * Vector3.one, Color.red );
        if (gunPoint == null) return;
        
        
        if (circularFireMode)
        {
            SetCirclePointsPos(ref linePos,out startPos ,gunPoint.point.transform.position,startAngle,angleSum, bulletCircleRadius, PointAmount);
            
        }
        else
        {

            var directionAngle = gunPoint.AimAngle+ bulletSpreadAngle;
            SetLinePointsPos(ref linePos,gunPoint.point.transform.position,directionAngle,speed);
            
            RaycastHit2D hit = Physics2D.Raycast(linePos[0], (linePos[1]-linePos[0]), speed, 15);
            
            if (hit.collider != null)
            {
                linePos[1] = hit.point;
            }    
            // if (hasCollision && collisionPoint != null)
            // {
            //     // Debug.Log("Using collision point: " + collisionPoint);
            //     linePos[1] = (Vector2)collisionPoint;
            // }
            // ((LineRenderer)bulletSpriteRenderer).SetPosition(1, endPos);
            transform.position = linePos[0];
            transform.eulerAngles = new Vector3(0.0f, 0.0f,  directionAngle * Mathf.Rad2Deg);

        }


        UpdateShootingPos();
        UpdateParticlesPos();
        UpdateEdgeCollider();
       
        //Debug.DrawRay(gunPoint.point.position,10*(endPos-startPos),Color.red);
    }
}
