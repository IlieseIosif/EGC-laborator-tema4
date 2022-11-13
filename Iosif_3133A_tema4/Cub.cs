using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using OpenTK.Input;

namespace Iosif_3133A_tema4
{
    class Cub : GameWindow
    {
        private int[,,,] varfCub;
        private KeyboardState AntKeyboardState;
        private bool individualTriangle = false;
        private int currentCubeFace = 0;
        private int currentTriangle = 0;
        private int[,] currentVertexOnCurrentTraiangle;

        private const float MIN_VALUE = 0f;
        private const float MAX_VALUE = 1f;
        private const float distantaAditionala = 30;

        private float[] AntRGB;
        private float[] RGB;
        private float[,] culoareFata;
        private float[,,,] culoareVarfTriunghi;
        private float[,,,] culoareVarfLaRandare;

        private float camerX = 30;
        private float camerY = 30;
        private float camerZ = 60;

        private float angleXZ = 0f;
        private float angleYZ = 0f;

        Random random;

        Matrix4 camera;

        public Cub():base(800, 600)
        {
            VSync = VSyncMode.On;
            varfCub = new int[6, 2, 3, 3];
            LoadCubeVertexes();

            currentVertexOnCurrentTraiangle = new int[6, 2];

            AntRGB = new float[3];
            RGB = new float[3];
            culoareFata = new float[6, 3];
            culoareVarfTriunghi = new float[6, 2, 3, 3];//6 fete,2 triunghi-uri per fata,3 varfuri per triunghi,3 culori per varf
            culoareVarfLaRandare = new float[6, 2, 3, 3];

            random = new Random();

            cubeRandomization();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            GL.ClearColor(Color.White);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.Hint(HintTarget.PolygonSmoothHint, HintMode.Fastest);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Width, Height);

            double aspect_ratio = Width / (double)Height;

            Matrix4 perspectiva = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, (float)aspect_ratio, 1, 250);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspectiva);

            camera = Matrix4.LookAt(camerX, camerY, camerZ, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref camera);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            int i, j;
            KeyboardState CurrentKeyboardState = Keyboard.GetState();

            //individual triangle==true - putem modifica individual culoare varfului fiecarui triunghi, altfel putem modifica individual culoarea fiecarei fete a cubului 
            if (CurrentKeyboardState[Key.I] && !AntKeyboardState[Key.I])
            {
                individualTriangle = !individualTriangle;

                loadRenderValues();//fiecare din cele 2 moduri are asociat o matrice de culori ce trebuie incarcata in matricea de culori folosita la randare
            }

            if (CurrentKeyboardState[Key.F])//schimbam triunghiul curent/fata curenta
            {
                if (CurrentKeyboardState[Key.Right] && !AntKeyboardState[Key.Right])
                {
                    if (individualTriangle)
                    {
                        currentTriangle = (currentTriangle + 1) % 12;
                    }
                    else
                    {
                        currentCubeFace = (currentCubeFace + 1) % 6;
                    }
                }

                if (CurrentKeyboardState[Key.Left] && !AntKeyboardState[Key.Left])
                {
                    if (individualTriangle)
                    {
                        if (currentTriangle == 0)
                        {
                            currentTriangle = 12;
                        }

                        currentTriangle = currentTriangle - 1;
                    }
                    else
                    {
                        if (currentCubeFace == 0)
                        {
                            currentCubeFace = 6;
                        }

                        currentCubeFace = currentCubeFace - 1;
                    }
                }
            }

            if (CurrentKeyboardState[Key.V])//schimbam varful curent din triunghiul curent
            {
                if (individualTriangle)
                {
                    if (CurrentKeyboardState[Key.Right] && !AntKeyboardState[Key.Right])
                    {
                        currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2] = (currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2] + 1) % 3;
                    }

                    if (CurrentKeyboardState[Key.Left] && !AntKeyboardState[Key.Left])
                    {
                        if (currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2] == 0)
                        {
                            currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2] = 3;
                        }

                        currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2] = currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2] - 1;
                    }
                }
            }

            if (CurrentKeyboardState[Key.M] && !AntKeyboardState[Key.M])//randomizare varf curent din triunghiul curent/fata curenta
            {
                if (individualTriangle)
                {
                    vertexRandomization(currentTriangle / 2, currentTriangle % 2, currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2]);
                }
                else
                {
                    cubeFaceRandomization(currentCubeFace);
                }
            }

            //se preia culoarea RGB a varfului curent din triunghiul curent sau culoarea RGB a fetei curente
            if (individualTriangle)
            {
                for (i = 0; i < 3; i++)
                {
                    RGB[i] = culoareVarfTriunghi[currentTriangle / 2, currentTriangle % 2, currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2], i];
                }
            }
            else
            {
                for (i = 0; i < 3; i++)
                {
                    RGB[i] = culoareFata[currentCubeFace, i];
                }
            }

            if (CurrentKeyboardState[Key.R])//crestem/micsoram cantitatea de culoare rosie
            {
                if (CurrentKeyboardState[Key.Plus] && RGB[0] < MAX_VALUE)
                {
                    RGB[0] += 0.01f;
                }
                else if (CurrentKeyboardState[Key.Minus] && RGB[0] > MIN_VALUE)
                {
                    RGB[0] -= 0.01f;
                }
            }

            if (CurrentKeyboardState[Key.G])//crestem/micsoram cantitatea de culoare verde
            {
                if (CurrentKeyboardState[Key.Plus] && RGB[1] < MAX_VALUE)
                {
                    RGB[1] += 0.01f;
                }
                else if (CurrentKeyboardState[Key.Minus] && RGB[1] > MIN_VALUE)
                {
                    RGB[1] -= 0.01f;
                }
            }

            if (CurrentKeyboardState[Key.B])//crestem/micsoram cantitatea de culoare albastra
            {
                if (CurrentKeyboardState[Key.Plus] && RGB[2] < MAX_VALUE)
                {
                    RGB[2] += 0.01f;
                }
                else if (CurrentKeyboardState[Key.Minus] && RGB[2] > MIN_VALUE)
                {
                    RGB[2] -= 0.01f;
                }
            }

            if (CurrentKeyboardState[Key.C])//rotire cub pe directia axei Ox sau pe directia axei Oy
            {
                if (CurrentKeyboardState[Key.Right])
                {
                    angleXZ += MathHelper.Pi;
                }
                else
                    if (CurrentKeyboardState[Key.Left])
                {
                    angleXZ -= MathHelper.Pi;
                }
                else
                        if (CurrentKeyboardState[Key.Up])
                {
                    angleYZ -= MathHelper.Pi;
                }
                else
                            if (CurrentKeyboardState[Key.Down])
                {
                    angleYZ += MathHelper.Pi;
                }
            }

            if (S_aModificatCuloarea())//daca s-a modificat vreo culoare, vom salva respectiva modificare, dupa care vom afisa la consola valorile RGB ale fetei/varfului triunghiului curent 
            {
                if (individualTriangle)
                {
                    for (i = 0; i < 3; i++)
                    {
                        culoareVarfLaRandare[currentTriangle / 2, currentTriangle % 2, currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2], i] = culoareVarfTriunghi[currentTriangle / 2, currentTriangle % 2, currentVertexOnCurrentTraiangle[currentTriangle / 2, currentTriangle % 2], i] = RGB[i];
                    }

                    printCurrentTriangleRGBValues();
                }
                else
                {
                    for (i = 0; i < 3; i++)
                    {
                        culoareVarfLaRandare[currentCubeFace, 0, 0, i] = culoareVarfLaRandare[currentCubeFace, 0, 1, i] = culoareVarfLaRandare[currentCubeFace, 0, 2, i] = RGB[i];
                        culoareVarfLaRandare[currentCubeFace, 1, 0, i] = culoareVarfLaRandare[currentCubeFace, 1, 1, i] = culoareVarfLaRandare[currentCubeFace, 1, 2, i] = culoareFata[currentCubeFace, i] = RGB[i];
                    }

                    printCurrentCubFaceRGBValues();
                }
            }

            AntKeyboardState = CurrentKeyboardState;
            RGB.CopyTo(AntRGB, 0);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.PushMatrix();
            GL.Rotate(angleXZ, 0, 1, 0);
            GL.Rotate(angleYZ, 1, 0, 0);
            DrawCube();         
            GL.PopMatrix();

            SwapBuffers();
        }

        /// <summary>
        /// Incarca coordonatele cubului dintr-un fisier text.
        /// </summary>
        private void LoadCubeVertexes()
        {
            String numeFisier = ConfigurationManager.AppSettings["numeFisier"];
            String caleFisier = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;
            String caleCompleta = caleFisier + "//" + numeFisier;

            using (StreamReader streamReader = new StreamReader(caleCompleta))
            {
                int i, j, k;

                for (i = 0; i < 6; i++)
                {
                    j = 0;
                    while (j / 3 != 1)
                    {
                        k = 0;

                        foreach (String coord in streamReader.ReadLine().Trim().Split(' '))
                        {
                            varfCub[i, 0, j, k] = varfCub[i, 1, j, k] = int.Parse(coord);
                            k++;
                        }

                        j++;
                    }

                    k = 0;

                    foreach (String coord in streamReader.ReadLine().Trim().Split(' '))
                    {
                        varfCub[i, 1, 0, k++] = int.Parse(coord);
                    }
                }
            }
        }

        /// <summary>
        /// Foloseste coordonatele si culorile varfurilor cubului pentru a-l desena.
        /// </summary>
        private void DrawCube()
        {
            GL.Begin(PrimitiveType.Triangles);

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        GL.Color3(culoareVarfLaRandare[i, j, k, 0], culoareVarfLaRandare[i, j, k, 1], culoareVarfLaRandare[i, j, k, 2]);
                        GL.Vertex3(varfCub[i, j, k, 0], varfCub[i, j, k, 1], varfCub[i, j, k, 2]);
                    }
                }
            }

            GL.End();
        }

        /// <summary>
        /// Verifica daca s-a modificat culoarea fetei curente/varfului triunghiului curent.
        /// </summary>
        /// <returns>true daca s-a modificat culoarea avuta in vizor, altfel false</returns>
        private bool S_aModificatCuloarea()
        {
            for (int i = 0; i < 3; i++)
            {
                if (RGB[i] != AntRGB[i])
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Afiseaza valorile RGB ale varfurilor triunghi-ului curent.
        /// </summary>
        private void printCurrentTriangleRGBValues()
        {
            for(int i=0;i<3;i++)
            {
                Console.WriteLine(String.Format("VERTEX[{0:D}]: R ={1,3:D}%, G ={2,3:D}%, B ={3,3:D}%"
                    , i
                    , (int)(culoareVarfTriunghi[currentTriangle / 2, currentTriangle % 2,i, 0] * 100)
                    , (int)(culoareVarfTriunghi[currentTriangle / 2, currentTriangle % 2, i, 1] * 100)
                    , (int)(culoareVarfTriunghi[currentTriangle / 2, currentTriangle % 2, i, 2] * 100)
                    ));
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Afiseaza valorile RGB ale fetei curente a cubului.
        /// </summary>
        private void printCurrentCubFaceRGBValues()
        {
            Console.WriteLine(String.Format("CUBE_FACE[{0:D}]: R ={1,3:D}%, G ={2,3:D}%, B ={3,3:D}%\n"
                    , currentCubeFace
                    , (int)(culoareFata[currentCubeFace, 0] * 100)
                    , (int)(culoareFata[currentCubeFace, 1] * 100)
                    , (int)(culoareFata[currentCubeFace, 2] * 100)
                    ));
        }

        /// <summary>
        /// Functie folosita pentru a randomiza cubul (randomizarea fiecarei fete si randomizarea fiecarui triunghi).
        /// </summary>
        private void cubeRandomization()
        {
            for(int i=0;i<6;i++)
            {
                for(int j=0;j<2;j++)
                {
                    triangleRandomization(i, j);
                }

                cubeFaceRandomization(i);
            }

            loadRenderValues();
        }

        /// <summary>
        /// Culoarea fetei curente va fi alcatuite din compunerea culorilor RGB generate aleatoriu.
        /// </summary>
        /// <param name="cubeFace">fata cubului ce se doreste a fi randomizata</param>
        private void cubeFaceRandomization(int cubeFace)
        {
            for(int i=0;i<3;i++)
            {
                culoareFata[cubeFace, i] = (float)random.NextDouble();
            }
        }

        /// <summary>
        /// Culoarea triunghiului curent din fata curenta va fi alcatuita din compunerea culorilor RGB generate aleatoriu.
        /// </summary>
        /// <param name="numarFata">fata cubului ce contine triunghiul a carui cauloare va fi randomizata</param>
        /// <param name="numarTriunghi">triunghiul din fata numarFata</param>
        private void triangleRandomization(int numarFata, int numarTriunghi)
        {
            for (int i=0;i<3;i++)
            {
                culoareVarfTriunghi[numarFata, numarTriunghi, 0, i] = (float)random.NextDouble();

                for (int j=1;j<3;j++)
                {
                    culoareVarfTriunghi[numarFata, numarTriunghi, j, i]
                        = culoareVarfTriunghi[numarFata, numarTriunghi, j - 1, i];
                }
            }
        }

        /// <summary>
        /// Culoarea varfului curent, din triunghiul curent, din fata curenta, va fi alcatuita din compunerea culorilor RGB generate aleatoriu.
        /// </summary>
        /// <param name="numarFata">fata cubului ce contine triunghiul a carui varf vom randomiza culoarea</param>
        /// <param name="numarTriunghi">triunghiul din fata numarFata</param>
        /// <param name="numarVarf">varful din triunghiul numarTriunghi</param>
        private void vertexRandomization(int numarFata, int numarTriunghi, int numarVarf)
        {
            for (int i = 0; i < 3; i++)
            {
                culoareVarfTriunghi[numarFata, numarTriunghi, numarVarf, i] = (float)random.NextDouble();
            }
        }

        /// <summary>
        /// Preia valorile RGB specifice modului curent (individual triangle/individual face) si le incarca in matricea de culori folosita la randare.
        /// </summary>
        private void loadRenderValues()
        {
            int i, j, k, l;

            if(individualTriangle)
            {
                for(i=0;i<6;i++)
                {
                    for(j=0;j<2;j++)
                    {
                        for(k=0;k<3;k++)
                        {
                            for(l=0;l<3;l++)
                            {
                                culoareVarfLaRandare[i, j, k, l] = culoareVarfTriunghi[i, j, k, l];
                            }
                        }
                    }
                }
            }
            else 
            { 
                for(i=0;i<6;i++)
                {
                    for(j=0;j<3;j++)
                    {
                        for(k=0;k<3;k++)
                        {
                            culoareVarfLaRandare[i, 0, j, k] 
                                = culoareVarfLaRandare[i, 1, j, k]
                                = culoareFata[i, k];
                        }
                    }
                }
            }
        }
    }
}
