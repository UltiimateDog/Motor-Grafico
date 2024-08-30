using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Threading;
using System.Diagnostics;

namespace Exame2
{   

    public partial class Form1 : Form
    {
        private Canvas canvas;
        private Scene scene;
        public int filter = 0; 
        private List<float> angle = new List<float> { 0, 0, 0 };
        private List<Matrix4x4> matrix = new List<Matrix4x4> { Matrix4x4.CreateRotationX(0), Matrix4x4.CreateRotationY(0), Matrix4x4.CreateRotationZ(0) };
        private float pi = (float)Math.PI;
        private List<KeyFrame> keyFrames = new List<KeyFrame>();
        private bool isAnimating = false;
        private Stopwatch animationTimer = new Stopwatch();
        private float animationDuration = 10.0f;
        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
        private Vector2 Lerp(Vector2 a, Vector2 b, float t)
        {
            return new Vector2(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));
        }

        public Form1()
        {
            InitializeComponent();
            canvas = new Canvas(pictureBox1.Size);
            scene = new Scene(canvas);
            scene.Models.Add(new Model("MON1.obj"));
            timer1.Tick += timer1_Tick;
            timer1.Start();


            pictureBox1.ClientSizeChanged += PictureBox1_ClientSizeChanged;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isAnimating)
            {
                float currentTime = (float)animationTimer.Elapsed.TotalSeconds;
                if (currentTime > animationDuration)
                {
                    isAnimating = false;
                    animationTimer.Stop();
                    timer1.Stop();
                    MessageBox.Show("Animation completed.");
                    return;
                }

                // Find the appropriate keyframes for interpolation
                KeyFrame previousFrame = null, nextFrame = null;
                foreach (var frame in keyFrames.OrderBy(kf => kf.Time))
                {
                    if (frame.Time <= currentTime)
                        previousFrame = frame;
                    if (frame.Time >= currentTime)
                    {
                        nextFrame = frame;
                        break;
                    }
                }

                if (previousFrame != null && nextFrame != null)
                {
                    float lerpFactor = (currentTime - previousFrame.Time) / (nextFrame.Time - previousFrame.Time);
                    Vector3 interpolatedRotation = Vector3.Lerp(previousFrame.Rotation, nextFrame.Rotation, lerpFactor);
                    float interpolatedScale = Lerp(previousFrame.Scale, nextFrame.Scale, lerpFactor);
                    Vector2 interpolatedTranslation = Vector2.Lerp(previousFrame.Translation, nextFrame.Translation, lerpFactor);

                    angle[0] = interpolatedRotation.X;
                    angle[1] = interpolatedRotation.Y;
                    angle[2] = interpolatedRotation.Z;
                    canvas.scale = interpolatedScale;
                    canvas.movements[0] = (int)interpolatedTranslation.X;
                    canvas.movements[1] = (int)interpolatedTranslation.Y;
                }
            }

            // Update rotation matrices and render the scene
            matrix[0] = Matrix4x4.CreateRotationY(angle[0]);
            matrix[1] = Matrix4x4.CreateRotationX(angle[1]);
            matrix[2] = Matrix4x4.CreateRotationZ(angle[2]);
            RenderScene();

            switch(filter)
            {
                case 1:
                    canvas.bits = BitProcess.Invert(canvas.bits);
                    break;
                case 2:
                    canvas.bits = BitProcess.Sepia(canvas.bits);
                    break;
                case 3:
                    canvas.bits = BitProcess.Gray(canvas.bits);
                    break;
            }
        }




        private void ButtonX_Click(object sender, EventArgs e)
        {
            filter = 1;
        }

        private void ButtonY_Click(object sender, EventArgs e)
        {
            filter = 2;
        }

        private void ButtonZ_Click(object sender, EventArgs e)
        {
            filter = 3;
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void PictureBox1_ClientSizeChanged(object sender, EventArgs e)
        {

            if (pictureBox1.ClientSize.Width > 0 && pictureBox1.ClientSize.Height > 0 && scene != null)
            {
                canvas = new Canvas(pictureBox1.ClientSize);
                RenderScene();
            }

        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.L:
                    canvas.movements[0] += 5;
                    break;
                case Keys.J:
                    canvas.movements[0] -= 5;
                    break;
                case Keys.K:
                    canvas.movements[1] += 5;
                    break;
                case Keys.I:
                    canvas.movements[1] -= 5;
                    break;
                case Keys.A:
                    angle[0] += 0.09f;
                    break;
                case Keys.W:
                    angle[1] += 0.09f;
                    break;
                case Keys.Q:
                    angle[2] += 0.09f;
                    break;
                case Keys.D:
                    angle[0] -= 0.09f;
                    break;
                case Keys.S:
                    angle[1] -= 0.09f;
                    break;
                case Keys.E:
                    angle[2] -= 0.09f;
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }


        private void RenderScene()
        {
            
            canvas.Render(scene, matrix); 
            pictureBox1.Image = canvas.bitmap;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            canvas.scale = (float)trackBar1.Value / 200;
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {
            canvas.power = trackBar2.Value;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            canvas.color = Color.White; 
        }

        private void button6_Click(object sender, EventArgs e)
        {
            canvas.color = Color.Purple;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            canvas.color = Color.Blue;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            canvas.color = Color.Green;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            canvas.color = Color.Yellow;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            canvas.color = Color.Red;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Document to Upload";
            openFileDialog.Filter = "All Files|*.*"; // You can change the filter as needed

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFileName = openFileDialog.FileName;
                // Here you can perform actions with the selected file, such as uploading it to a server
                MessageBox.Show($"Selected file: {selectedFileName}", "File Selected");

                //scene.Models.Clear();
                scene.Models.Add(new Model(selectedFileName));
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {

        }

        private void AddKeyFrame_Click(object sender, EventArgs e)
        {
            float currentTime = TIMELINEBAR.Value * animationDuration / TIMELINEBAR.Maximum;
            Vector3 currentRotation = new Vector3(angle[0], angle[1], angle[2]);
            float currentScale = canvas.scale;
            Vector2 currentTranslation = new Vector2(canvas.movements[0], canvas.movements[1]);
            keyFrames.Add(new KeyFrame(currentTime, currentRotation, currentScale, currentTranslation));
            MessageBox.Show($"Keyframe added at {currentTime} seconds with scale {currentScale} and translation ({currentTranslation.X}, {currentTranslation.Y})");
        }



        private void StartBtn_Click(object sender, EventArgs e)
        {
            if (keyFrames.Count < 2)
            {
                MessageBox.Show("Add at least two keyframes before starting the animation.");
                return;
            }
            isAnimating = true;
            animationTimer.Restart();
            timer1.Start();
        }

        private void TIMELINEBAR_Scroll(object sender, EventArgs e)
        {
            if (!isAnimating) // Only allow manual scrubbing when not animating
            {
                float selectedTime = TIMELINEBAR.Value * animationDuration / TIMELINEBAR.Maximum;
                // Implement interpolation logic similar to the timer tick method to adjust canvas parameters
            }
        }



        private void RestartBtn_Click(object sender, EventArgs e)
        {
            // Clear keyframes and stop any animation
            keyFrames.Clear();
            isAnimating = false;
            animationTimer.Stop();

            // Reset transformations and canvas
            angle[0] = 0f;
            angle[1] = 0f;
            angle[2] = 0f;
            canvas.scale = 0.2f;
            canvas.movements[0] = 0;
            canvas.movements[1] = 0;
            canvas.FastClear();
            pictureBox1.Image = canvas.bitmap;

            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            filter = 0;
        }
    }
}
