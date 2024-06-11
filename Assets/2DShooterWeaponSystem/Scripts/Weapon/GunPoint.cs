using UnityEngine;

public class GunPoint
{

    public GunPoint() : base()
    {
        
    }

    public GunPoint(Transform point, float AimAngle)
    {
        this.point = point;
        this.AimAngle = AimAngle;
    }
    
    public Transform point;

    

    public float AimAngle;
    
}
