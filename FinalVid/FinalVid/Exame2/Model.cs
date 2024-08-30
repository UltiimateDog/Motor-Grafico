using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

public class Model
{
    public List<Mesh> Meshes { get; set; } = new List<Mesh>();

    public Model(string path)
    {
        LoadModel(path);
    }

    private void LoadModel(string path)
    {
        Mesh mesh = new Mesh();
        foreach (var line in File.ReadAllLines(path))
        {
            if (line.StartsWith("v "))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                mesh.Vertices.Add(new Vector3(
                    float.Parse(parts[1], CultureInfo.InvariantCulture),
                    float.Parse(parts[2], CultureInfo.InvariantCulture),
                    float.Parse(parts[3], CultureInfo.InvariantCulture)));
            }
            else if (line.StartsWith("f"))
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                mesh.Indexes.Add(new int[] {
                    int.Parse(parts[1].Split('/')[0]) - 1,
                    int.Parse(parts[2].Split('/')[0]) - 1,
                    int.Parse(parts[3].Split('/')[0]) - 1
                });
            }
        }
        Meshes.Add(mesh);
    }
}
