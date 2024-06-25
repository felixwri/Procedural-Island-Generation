using UnityEngine;

public class Point
{
    public float x;
    public float y;
    public float z;
    public int id = 0;

    public Point(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public Point(Vector3 vector)
    {
        this.x = vector.x;
        this.y = vector.y;
        this.z = vector.z;
    }

    public static implicit operator Vector3(Point p)
    {
        return new Vector3(p.x, p.y, p.z);
    }

    public static implicit operator Point(Vector3 v)
    {
        return new Point(v);
    }
}
