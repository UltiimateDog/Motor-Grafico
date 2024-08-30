using Exame2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

public class Scene
{
    public List<Model> Models { get; set; } = new List<Model>();
    private Canvas canvas;


    public Scene(Canvas canvas)
    {
        this.canvas = canvas;
    }

public void Render(Canvas canvas, List<Matrix4x4> rotationMatrix, int power, Color color, List<int> movs)
    {
        if (canvas.Width <= 0 || canvas.Height <= 0)
        {
            return;
        }

        for (int i = 0; i < Models.Count; i++)
        {
            var model = Models[i];
            if (i == 0)
            {
                for (int j = 0; j < model.Meshes.Count; j++)
                {
                    var mesh = model.Meshes[j];
                    mesh.Render(canvas, rotationMatrix, power, color, movs);
                }
            } else
            {
                for (int j = 0; j < model.Meshes.Count; j++)
                {
                    var mesh = model.Meshes[j];
                    mesh.Render(canvas, new List<Matrix4x4> { Matrix4x4.CreateRotationZ(0), Matrix4x4.CreateRotationZ(0), Matrix4x4.CreateRotationZ(0) }, power, color, new List<int> { 0, 0, 0});
                }
            }
        }
    }
}
