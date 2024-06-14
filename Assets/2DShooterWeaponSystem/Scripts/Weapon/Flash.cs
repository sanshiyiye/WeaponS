using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Flash : Bullet
{
    
    private Transform startParticles;
    
    private Transform endParticles;
    
    private Collider2D edge;
    
    public override void Init()
    {
        
        isAlive = true;
        
        edge = GetComponent<EdgeCollider2D>();   //得到物体身上的EdgeCollider2D组件
        ((EdgeCollider2D)edge).edgeRadius = 0.1f;
        
        if (circularFireMode)
        {
            
            bulletCircleRadius = 5f;
            bulletCircleWidth = 0.5f;
            segmentAmount = CalcSegmentAmount(360, bulletCircleWidth, bulletCircleRadius);
            linePos = new Vector3[PointAmount];
            

        }
        else
        {
            segmentAmount = 19;
            linePos = new Vector3[20];
        }
        
        bulletSpriteRenderer = GetComponent<LineRenderer>();
        var lineRenderer = ((LineRenderer)bulletSpriteRenderer);
        lineRenderer.positionCount = PointAmount;
        lineRenderer.startWidth = 1.0f;
        lineRenderer.endWidth = 1.0f;

    }
    
    
    
    public Vector3 [] linePos ;

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
        // UpdateParticlesPos();
        // UpdateEdgeCollider();
    }
    
    // 根据起始角度、弧度、弧半径、构成弧的各点的数量，算得各点的位置
    void SetCirclePointsPos(ref Vector3[] posArray, out Vector2 startPos, Vector2 center,
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
    void SetLinePointsPos(ref Vector3[] linePos,Vector3 gunPosition, float directionAngle, float length)
    {
        // linePos[0] = Vector2.zero;
        linePos[0].x = gunPosition.x;
        linePos[0].y = gunPosition.y;
        linePos[0].z = gunPosition.z;
            
        // linePos[1] = Vector2.zero;
        linePos[^1].x = linePos[0].x + Mathf.Cos(directionAngle) * length ;
        linePos[^1].y = linePos[0].y + Mathf.Sin(directionAngle) * length;
        
        int lineSize = linePos.Length;
        for(int i=1; i < lineSize-1; i++)
        {
            linePos[i] = (Vector3.Lerp(linePos[0], linePos[^1], (float)i / lineSize));
        }
        
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
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            collisionPoint = contact.point;
        }
    }

    private float fps = 0;

    public float _Speed = 2;
    
    private float sineRandom;
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
            
            fps += Time.deltaTime*_Speed;
            if (fps >= 1)
            {
                sineRandom = (float)Random.Range(0, linePos.Length * 10) / 10;
                fps = 0;
            }

            for (int i = 0; i < linePos.Length; i++)
            {
                Vector2 point = linePos[i];
                Vector3 v = Vector3.zero;

                if (useArc)
                {
                    // if (i != 0 && i != linePos.Length - 1)
                    {
                        if (X) v.x = Arcing(i);
                        if (Y) v.y = Arcing(i);
                        if (Z) v.z = Arcing(i);
                        point = Vector3.Lerp(linePos[i], linePos[i] + v, fps);
                    }
                   
                }

                if (useSine)
                {
                    v = Vector3.zero;
                    if (i != 0 && i != linePos.Length - 1)
                    {
                        if (X) v.x = Sine(i + sineRandom);
                        if (Y) v.y = Sine(i + sineRandom);
                        if (Z) v.z = Sine(i + sineRandom);
                        point += (Vector2)v;

                    }
                }

                if (useWiggle)
                {
                    if (i != 0 && i != linePos.Length - 1)
                    {
                        point += (Vector2)Wiggle(i);

                    }
                }
                
                ((LineRenderer)bulletSpriteRenderer).SetPosition(i, point);
    
                
            }
            
            
            transform.position = linePos[0];
            transform.eulerAngles = new Vector3(0.0f, 0.0f,  directionAngle * Mathf.Rad2Deg);

        }


        // UpdateShootingPos();
        // UpdateParticlesPos();
        // UpdateEdgeCollider();
       
    }

    public float _ArcingPowParam1 = 0.28f;


    public float _Adjust = 2;
    
    public bool X = true;
    public bool Y = true;
    public bool Z = true;
    
    // 这个值等于 frequencyIncrement 
    public float _SineScaleX = 1;
    
    // 这个值等于 amplitude
    public float _SineScaleY = 1;
    public float _RandomSize = 0.1f;

    public bool useArc;

    public bool useSine;

    public bool useWiggle;
    
    float Arcing(float param)
    {
        return _ArcingPowParam1 * Mathf.Pow((param - (float)linePos.Length / 2 ),2) + _Adjust;
    }
 
 
    float Sine(float param)
    {
        return Mathf.Sin((float)param / linePos.Length * 2 * 3.14f * _SineScaleX) * _SineScaleY;
    }
 
    Vector3 Wiggle(int listIndex)
    {
        Vector3 v = new Vector3();
        if (X) v.x = Random.Range(0, 10) * _RandomSize;
        if (Y) v.y = Random.Range(0, 10) * _RandomSize;
        if (Z) v.z = Random.Range(0, 10) * _RandomSize;
        return v;
    }
}
