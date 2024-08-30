using Exame2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

public class Mesh
{
    public List<Vector3> Vertices { get; set; } = new List<Vector3>();
    public List<int[]> Indexes { get; set; } = new List<int[]>();

    private List<Light> meshLight = new List<Light>();

    public class Light
    {
        public Vector3 Direction { get; set; }
        public Color Color { get; set; }

        public Light(Vector3 direction, Color color)
        {
            Direction = direction;
            Color = color;
        }
    }

    public Mesh()
    {
        meshLight.Add(new Light(new Vector3(1, 1, -1), Color.White));
        meshLight.Add(new Light(new Vector3(-1, -1, 1), Color.White));
        meshLight.Add(new Light(new Vector3(1, 1, -1), Color.White));
        meshLight.Add(new Light(new Vector3(-1, -1, 1), Color.White));
    }

    public void Render(Canvas canvas, List<Matrix4x4> rotationMatrix, int power, Color color, List<int> movs)
    {
        Vector3 cameraDirection = new Vector3(0, 0, 1); // Asumiendo que la cámara mira hacia el -Z
        List<Tuple<float, Vector3[], Vector2[], float[]>> renderList = new List<Tuple<float, Vector3[], Vector2[], float[]>>();

        for (int faceIndex = 0; faceIndex < Indexes.Count; faceIndex++)
        {
            var face = Indexes[faceIndex];
            if (face.Length < 3) continue; // Necesitamos al menos 3 vértices para un triángulo

            Vector3[] worldVertices = new Vector3[3];
            Vector2[] screenVertices = new Vector2[3];
            float[] intensities = new float[3];
            float averageDepth = 0;

            // Transformar vértices al espacio de la pantalla y calcular intensidades
            for (int i = 0; i < 3; i++)
            {
                worldVertices[i] = Vector3.Transform(Vertices[face[i]], rotationMatrix[0]);
                worldVertices[i] = Vector3.Transform(worldVertices[i], rotationMatrix[1]);
                worldVertices[i] = Vector3.Transform(worldVertices[i], rotationMatrix[2]);
                averageDepth += worldVertices[i].Z; // Usar Z para profundidad
                screenVertices[i] = new Vector2(
                    (worldVertices[i].X * canvas.scale * canvas.Height + canvas.Width * 0.5f) + movs[0],
                    ((1 - worldVertices[i].Y * canvas.scale) * canvas.Height - canvas.Height * 0.5f) + movs[1]);
                intensities[i] = CalculateLightIntensityM(worldVertices[i], canvas, power);
            }
            averageDepth /= 3;

            // Calcular la normal del triángulo
            Vector3 v1 = worldVertices[1] - worldVertices[0];
            Vector3 v2 = worldVertices[2] - worldVertices[0];
            Vector3 normal = Vector3.Cross(v1, v2);
            normal = Vector3.Normalize(normal);

            // Verificar si el triángulo está frente a la cámara
            if (Vector3.Dot(normal, cameraDirection) < 0)
            {
                // El triángulo está frente a la cámara; agregarlo a la lista de renderización
                renderList.Add(new Tuple<float, Vector3[], Vector2[], float[]>(averageDepth, worldVertices, screenVertices, intensities));
            }
        }

        // Ordenar triángulos por profundidad, de más lejano a más cercano
        var orderedTriangles = renderList.OrderBy(x => -x.Item1);

        var orderedTrianglesList = orderedTriangles.ToList();

        // Renderizar triángulos ordenados
        for (int i = 0; i < orderedTrianglesList.Count; i++)
        {
            var item = orderedTrianglesList[i];
            Vector2[] vertices = item.Item3;
            float[] intensities = item.Item4;

            canvas.DrawShadedTriangle(
                new Point((int)vertices[0].X, (int)vertices[0].Y),
                new Point((int)vertices[1].X, (int)vertices[1].Y),
                new Point((int)vertices[2].X, (int)vertices[2].Y),
                color, intensities[0], intensities[1], intensities[2]);
        }
    }

    private float CalculateLightIntensityM(Vector3 point, Canvas canvas, int power)
    {
        float maxDistance = (float)Math.Sqrt(Math.Pow(10, 2) + Math.Pow(10, 2));

        float totalIntensity = 0f;

        for (int i = 0; i < meshLight.Count; i++)
        {
            // Calculate the distance from the point to the center
            float distance = Vector3.Distance(point, meshLight[i].Direction);

            // Use an exponential function to calculate the intensity based on distance
            // You can adjust the factor (-30) for the intensity falloff according to your needs
            float intensity = (float)Math.Exp(power * (distance / maxDistance));

            // Sum up intensities from all points
            totalIntensity += intensity;
        }

        return totalIntensity;
    }
}
