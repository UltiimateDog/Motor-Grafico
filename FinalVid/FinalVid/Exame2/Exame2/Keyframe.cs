using System.Numerics;

public class KeyFrame
{
    public float Time { get; set; } // Time in seconds
    public Vector3 Rotation { get; set; } // Rotation angles (X, Y, Z)
    public float Scale { get; set; } // Scale factor
    public Vector2 Translation { get; set; } // Translation offsets (X, Y)

    public KeyFrame(float time, Vector3 rotation, float scale, Vector2 translation)
    {
        Time = time;
        Rotation = rotation;
        Scale = scale;
        Translation = translation;
    }
}
