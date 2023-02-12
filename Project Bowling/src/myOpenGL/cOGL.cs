using System;
using System.Collections.Generic;
using System.Windows.Forms;

//2
using System.Drawing;

namespace OpenGL
{



    class CVector4Df
    {
        // the vector of floats
        public float[] m_v = new float[4];



        // construct the NULL point v=[0,0,0,1]
        public CVector4Df()
        {
            m_v[0] = 0.0f;
            m_v[1] = 0.0f;
            m_v[2] = 0.0f;
            m_v[3] = 1.0f;
        }

        // construc v=[x,y,z,w]
        public CVector4Df(float x, float y, float z, float w = 1.0f)
        {
            m_v[0] = x;
            m_v[1] = y;
            m_v[2] = z;
            m_v[3] = w;
        }

        // copy constructor
        public CVector4Df(CVector4Df source)
        {
            m_v[0] = source.m_v[0];
            m_v[1] = source.m_v[1];
            m_v[2] = source.m_v[2];
            m_v[3] = source.m_v[3];
        }

    };

    // to manage some transformations
    class CMatrix4x4f
    {
        // the 4x4 values
        public float[] m_vector = new float[16];


        // default constructor. The the Identity matrix
        public CMatrix4x4f()
        {
            SetIdentity();
        }


        // copy constructor
        public CMatrix4x4f(CMatrix4x4f rightMatrix)
        {
            for (int i = 0; i < 16; i++)
                m_vector[i] = rightMatrix.m_vector[i];
        }

        // set as identity matrix
        public void SetIdentity()
        {
            for (int i = 0; i < 16; i++)
                m_vector[i] = 0.0f;
            m_vector[0] = m_vector[5] = m_vector[10] = m_vector[15] = 1.0f;
        }

        // matrix to rotate angle degrees around axis
        public void SetRotate(float angle, char axis)
        {
            float s = (float)Math.Sin(angle), c = (float)Math.Cos(angle);
            SetIdentity();
            switch (axis)
            {
                case 'X':
                case 'x': m_vector[5] = m_vector[10] = c; m_vector[6] = -s; m_vector[9] = s; break;
                case 'Y':
                case 'y': m_vector[0] = m_vector[10] = c; m_vector[2] = -s; m_vector[8] = s; break;
                case 'Z':
                case 'z': m_vector[0] = m_vector[5] = c; m_vector[1] = -s; m_vector[4] = s; break;
            }
        }

        // matrix * vector operation
        public static CVector4Df operator *(CMatrix4x4f a, CVector4Df b)
        {
            CVector4Df res = new CVector4Df();
            int fila, i, j;
            for (i = 0; i < 4; i++)
            {
                fila = 4 * i;
                res.m_v[i] = 0.0f;
                for (j = 0; j < 4; j++)
                    res.m_v[i] += a.m_vector[fila + j] * b.m_v[j];
            }
            return res;
        }

    };






    class cOGL
    {
        Control p;
        int Width;
        int Height;

        // to launch the ball, set to true
        Boolean animate = false;

        int ballColorIndex = 3;

        // z position of the ball
        float zBall = -5.0f;

        float ballXAngle = 0.0f;

        // boolean for each possible key
        bool[] g_pressed = new bool[256];

        public int intOptionA=1;
        public bool is2x3=false;

        public int force = 50;

        GLUquadric obj;


        // eye position
        CVector4Df g_eyePos;

        // eye view direction
        CVector4Df g_eyeDir;

        // rotation around y axis
        float g_rotY = 0.0f;

        // movement speed
        float g_speed = 0.0f;

        // eye speed increasement per frame
        float SPEED_INC = 0.0006f;

        // eye rotation increasement per frame
        float ROTATE_INC = 0.006f;

        // maximum speed of the user 
        public float MAX_SPEED = 0.1f;

        public int MAX_STEP = 180;

        public int[] step;

        public cOGL(Control pb)
        {
            p=pb;
            Width = p.Width;
            Height = p.Height; 
            InitializeGL();
            obj = GLU.gluNewQuadric();
            g_eyePos = new CVector4Df(0.0f, 1.0f, 3.0f, 1);
            g_eyeDir = new CVector4Df(0, 0, -1, 0);
            step = new int[10];
            for (int i = 0; i < 10; i++)
                step[i] = 0;
        }

        ~cOGL()
        {
            GLU.gluDeleteQuadric(obj); 
            WGL.wglDeleteContext(m_uint_RC);
        }

		uint m_uint_HWND = 0;

        public uint HWND
		{
			get{ return m_uint_HWND; }
		}
		
        uint m_uint_DC   = 0;

        public uint DC
		{
			get{ return m_uint_DC;}
		}
		uint m_uint_RC   = 0;

        public uint RC
		{
			get{ return m_uint_RC; }
		}
        
        float angle = 0.0f;


        void StudyKey()
        {

            // accelerate 
            if (g_pressed[(int)System.Windows.Forms.Keys.W])
            {
                g_speed += SPEED_INC;
                if (g_speed > MAX_SPEED) g_speed = MAX_SPEED;
            }
            else if (g_speed > 0)             // desaccelerate 
            {
                g_speed -= SPEED_INC;
                if (g_speed < 0) g_speed = 0;
            }

            // desaccelerate
            if (g_pressed[(int)System.Windows.Forms.Keys.S])
            {
                g_speed -= SPEED_INC;
                if (g_speed < -MAX_SPEED) g_speed = -MAX_SPEED;
            }
            else if (g_speed < 0)
            {
                g_speed += SPEED_INC;
                if (g_speed > 0) g_speed = 0;
            }

            // rotation: keys A and D
            if (g_pressed[(int)System.Windows.Forms.Keys.A])
            {
                CMatrix4x4f m = new CMatrix4x4f();
                m.SetRotate(-2 * ROTATE_INC, 'y');
                g_eyeDir = m * g_eyeDir;
                g_rotY -= 2.0f * ROTATE_INC * 180.0f / 3.14159f;
            }

            if (g_pressed[(int)System.Windows.Forms.Keys.D])
            {
                CMatrix4x4f m = new CMatrix4x4f();
                m.SetRotate(2 * ROTATE_INC, 'y');
                g_eyeDir = m * g_eyeDir;
                g_rotY += 2 * ROTATE_INC * 180.0f / 3.14159f;
            }
        }

        public void keyboardDown(System.Windows.Forms.Keys key)
        {
            if (key >= 0 && key <= System.Windows.Forms.Keys.OemClear)
                g_pressed[(int)key] = true;
        }

        public void keyboardUp(System.Windows.Forms.Keys key)
        {
            //printf("key %d pressed\n", key);
            if (key >= 0 && key < System.Windows.Forms.Keys.OemClear)
                g_pressed[(int)key] = false;
        }
        void DrawColorCube()
        {
            // cube
            GL.glBegin(GL.GL_QUADS);


            //1

            GL.glColor3f(0.0f, 0.0f, 0.0f);
            GL.glVertex3f(0.0f, 0.0f, 0.0f);

            GL.glColor3f(0.0f, 1.0f, 0.0f);
            GL.glVertex3f(0.0f, 1.0f, 0.0f);

            GL.glColor3f(1.0f, 1.0f, 0.0f);
            GL.glVertex3f(1.0f, 1.0f, 0.0f);

            GL.glColor3f(1.0f, 0.0f, 0.0f);
            GL.glVertex3f(1.0f, 0.0f, 0.0f);

            //2

            GL.glColor3f(0.0f, 0.0f, 0.0f);
            GL.glVertex3f(0.0f, 0.0f, 0.0f);

            GL.glColor3f(0.0f, 0.0f, 1.0f);
            GL.glVertex3f(0.0f, 0.0f, 1.0f);

            GL.glColor3f(0.0f, 1.0f, 1.0f);
            GL.glVertex3f(0.0f, 1.0f, 1.0f);

            GL.glColor3f(0.0f, 1.0f, 0.0f);
            GL.glVertex3f(0.0f, 1.0f, 0.0f);


            //3

            GL.glColor3f(0.0f, 0.0f, 0.0f);
            GL.glVertex3f(0.0f, 0.0f, 0.0f);

            GL.glColor3f(1.0f, 0.0f, 0.0f);
            GL.glVertex3f(1.0f, 0.0f, 0.0f);

            GL.glColor3f(1.0f, 0.0f, 1.0f);
            GL.glVertex3f(1.0f, 0.0f, 1.0f);

            GL.glColor3f(0.0f, 0.0f, 1.0f);
            GL.glVertex3f(0.0f, 0.0f, 1.0f);


            //4

            GL.glColor3f(1.0f, 0.0f, 0.0f);
            GL.glVertex3f(1.0f, 0.0f, 0.0f);

            GL.glColor3f(1.0f, 0.0f, 1.0f);
            GL.glVertex3f(1.0f, 0.0f, 1.0f);

            GL.glColor3f(1.0f, 1.0f, 1.0f);
            GL.glVertex3f(1.0f, 1.0f, 1.0f);

            GL.glColor3f(1.0f, 1.0f, 0.0f);
            GL.glVertex3f(1.0f, 1.0f, 0.0f);


            //5

            GL.glColor3f(1.0f, 1.0f, 1.0f);
            GL.glVertex3f(1.0f, 1.0f, 1.0f);

            GL.glColor3f(1.0f, 1.0f, 0.0f);
            GL.glVertex3f(1.0f, 1.0f, 0.0f);

            GL.glColor3f(0.0f, 1.0f, 0.0f);
            GL.glVertex3f(0.0f, 1.0f, 0.0f);

            GL.glColor3f(0.0f, 1.0f, 1.0f);
            GL.glVertex3f(0.0f, 1.0f, 1.0f);


            //6

            GL.glColor3f(1.0f, 1.0f, 1.0f);
            GL.glVertex3f(1.0f, 1.0f, 1.0f);

            GL.glColor3f(0.0f, 1.0f, 1.0f);
            GL.glVertex3f(0.0f, 1.0f, 1.0f);

            GL.glColor3f(0.0f, 0.0f, 1.0f);
            GL.glVertex3f(0.0f, 0.0f, 1.0f);

            GL.glColor3f(1.0f, 0.0f, 1.0f);
            GL.glVertex3f(1.0f, 0.0f, 1.0f);


            GL.glEnd();
        }

 



        public void drawFloor()
        {
            GL.glBindTexture(GL.GL_TEXTURE_2D, Textures[0]);
            GL.glBegin(GL.GL_QUADS);
                GL.glNormal3f(0, 1, 0);

                GL.glTexCoord2f(0, 0);
                GL.glVertex3f(-1, 0, 6);

                GL.glTexCoord2f(1, 0);
                GL.glVertex3f(+1, 0, 6);

                GL.glTexCoord2f(1, 4);
                GL.glVertex3f(+1, 0, -18);

                GL.glTexCoord2f(0, 4);
                GL.glVertex3f(-1, 0, -18);
            GL.glEnd();

            int delta = 10;
            GL.glBindTexture(GL.GL_TEXTURE_2D, Textures[1]);
            for (int i=-180; i<0; i+=delta)
            {
                float x0 = (float)Math.Cos(3.14159 * i / 180.0);
                float y0 = (float)Math.Sin(3.14159 * i / 180.0);
                float x1 = (float)Math.Cos(3.14159 * (i+delta) / 180.0);
                float y1 = (float)Math.Sin(3.14159 * (i+delta) / 180.0);

                y0 *= 0.5f;
                y1 *= 0.5f;
                x0 *= 0.5f;
                x1 *= 0.5f;
                GL.glBegin(GL.GL_QUADS);
                    GL.glNormal3f(-x0, -y0, 0);
                    GL.glTexCoord2f(i/180.0f, 0);
                    GL.glVertex3f(-1.25f+x0, y0, 6);

                    GL.glNormal3f(-x1, -y1, 0);
                    GL.glTexCoord2f((i+1) / 180.0f, 0);
                    GL.glVertex3f(-1.25f+x1, y1, 6);

                    GL.glTexCoord2f((i + 1) / 180.0f, 1);
                    GL.glNormal3f(-x1, -y1, 0);
                    GL.glVertex3f(-1.25f + x1, y1, -18);

                    GL.glNormal3f(-x0, -y0, 0);
                    GL.glTexCoord2f(i / 180.0f, 1);
                    GL.glVertex3f(-1.25f + x0, y0, -18);


                    GL.glNormal3f(-x0, -y0, 0);
                    GL.glTexCoord2f(i / 180.0f, 0);
                    GL.glVertex3f(1.25f + x0, y0, 6);

                    GL.glNormal3f(-x1, -y1, 0);
                    GL.glTexCoord2f((i + 1) / 180.0f, 0);
                    GL.glVertex3f(1.25f + x1, y1, 6);

                    GL.glTexCoord2f((i + 1) / 180.0f, 1);
                    GL.glNormal3f(-x1, -y1, 0);
                    GL.glVertex3f(1.25f + x1, y1, -18);

                    GL.glNormal3f(-x0, -y0, 0);
                    GL.glTexCoord2f(i / 180.0f, 1);
                    GL.glVertex3f(1.25f + x0, y0, -18);

                GL.glEnd();
            }
        }

        public struct Point
        {
            Point(float x, float y)
            {
                this.x = x;
                this.y = y;
            }
            public float x;
            public float y;
        }

        void drawPin(int index, float X)
        {
            float w = 0.2f/5.0f;
            Point [] p = new Point[]
            {
               new Point{x= 0.00f,  y= 0.00f },
               new Point{x= 1.50f,  y= 0.00f },
               new Point{x= 2.75f,  y= 0.75f },
               new Point{x= 4.00f,  y= 2.00f },
               new Point{x= 4.80f,  y= 4.00f },
               new Point{x= 4.80f,  y= 6.60f },
               new Point{x= 4.40f,  y= 8.50f },
               new Point{x= 3.50f,  y=10.00f },
               new Point{x= 1.80f,  y=11.00f },
               new Point{x= 1.00f,  y=12.15f },
               new Point{x= 0.80f,  y=13.85f },
               new Point{x= 1.30f,  y=15.60f },
               new Point{x= 1.90f,  y=16.70f },
               new Point{x= 2.00f,  y=18.00f },
               new Point{x= 1.30f,  y=19.30f },
               new Point{x= 0.50f,  y=19.80f },
               new Point{x= 0.00f,  y=20.00f }
            };

            Point[] n = new Point[]
            {
               new Point{x= 0.00f,  y= -1.00f },
               new Point{x= 0.50f,  y= -1.00f },
               new Point{x= 1.00f,  y= -1.00f },
               new Point{x= 0.70f,  y= -0.30f },
               new Point{x= 0.85f,  y= -0.15f },
               new Point{x= 0.90f,  y= 0.10f },
               new Point{x= 1.00f,  y= 1.00f },
               new Point{x= 0.40f,  y= 0.60f },
               new Point{x= 1.00f,  y= 1.00f },
               new Point{x= 0.70f,  y= 0.30f },
               new Point{x= 1.00f,  y= 0.00f },
               new Point{x= 0.75f,  y=-0.25f },
               new Point{x= 0.80f,  y= 0.20f },
               new Point{x= 1.00f,  y= 0.00f },
               new Point{x= 1.00f,  y= 1.00f },
               new Point{x= 0.30f,  y= 0.70f },
               new Point{x= 0.00f,  y= 1.00f }
            };


            for (int i=0; i<p.Length; i++)
            {
                p[i].x *= w * 0.7f;
                p[i].y *= w;
            }

            int delta = 20;
            GL.glBindTexture(GL.GL_TEXTURE_2D, Textures[2]);
            GL.glPushMatrix();

            if (step[index] != 0)
            {
                GL.glTranslatef(X, 10.0f*w, 0);

                
                if (step[index] > MAX_STEP/2)
                    GL.glTranslatef(0, (MAX_STEP-step[index]) / (float)MAX_STEP, 0);
                else
                    GL.glTranslatef(0, (step[index]) / (float)MAX_STEP, 0);
                
                if (index == 0 || index == 1 || index == 3 || index == 6 || index == 7)
                    GL.glRotatef(-step[index] / (float)MAX_STEP * 360.0f, 0, 0, 1);
                else
                    GL.glRotatef(step[index] / (float)MAX_STEP * 360.0f, 0, 0, 1);
                
                GL.glTranslatef(-X,  -10.0f*w, 0);
                step[index]--;
            }


            GL.glBegin(GL.GL_QUADS);
            for (int i=0; i<360; i+= delta)
            {
                for (int j = 0; j < p.Length; j++)
                {
                    float x0 = (float)(Math.Cos(i * 3.14159 / 180.0) * p[j].x);
                    float y0 = p[j].y;
                    float z0 = (float)(Math.Sin(i * 3.14159 / 180.0) * p[j].x);

                    float nx = (float)(Math.Cos(i * 3.14159 / 180.0) * n[j].x);
                    float ny = n[j].y;
                    float nz = (float)(Math.Sin(i * 3.14159 / 180.0) * n[j].x);

                    float norm = (float)Math.Sqrt(nx * nx * ny * ny + nz * nz);
                    nx /= norm;
                    ny /= norm;
                    nz /= norm;

                    GL.glNormal3f(nx, ny, nz);
                    GL.glTexCoord2f(i / 360.0f, p[j].y);
                    GL.glVertex3f(x0, y0, z0);




                    x0 = (float)(Math.Cos((i + delta) * 3.14159 / 180.0) * p[j].x);
                    y0 = p[j].y;
                    z0 = (float)(Math.Sin((i + delta) * 3.14159 / 180.0) * p[j].x);

                    nx = (float)(Math.Cos((i + delta) * 3.14159 / 180.0) * n[j].x);
                    ny = n[j].y;
                    nz = (float)(Math.Sin((i + delta) * 3.14159 / 180.0) * n[j].x);

                    norm = (float)Math.Sqrt(nx * nx * ny * ny + nz * nz);
                    nx /= norm;
                    ny /= norm;
                    nz /= norm;

                    GL.glNormal3f(nx, ny, nz);
                    GL.glTexCoord2f((i + delta) / 360.0f, p[j].y);
                    GL.glVertex3f(x0, y0, z0);


                    int J = (j + 1) % p.Length;
                    x0 = (float)(Math.Cos((i + delta) * 3.14159 / 180.0) * p[J].x);
                    y0 = p[J].y;
                    z0 = (float)(Math.Sin((i + delta) * 3.14159 / 180.0) * p[J].x);

                    nx = (float)(Math.Cos((i + delta) * 3.14159 / 180.0) * n[J].x);
                    ny = n[J].y;
                    nz = (float)(Math.Sin((i + delta) * 3.14159 / 180.0) * n[J].x);

                    norm = (float)Math.Sqrt(nx * nx * ny * ny + nz * nz);
                    nx /= norm;
                    ny /= norm;
                    nz /= norm;

                    GL.glNormal3f(nx, ny, nz);
                    GL.glTexCoord2f((i + delta) / 360.0f, p[J].y);
                    GL.glVertex3f(x0, y0, z0);



                    x0 = (float)(Math.Cos(i * 3.14159 / 180.0) * p[J].x);
                    y0 = p[J].y;
                    z0 = (float)(Math.Sin(i * 3.14159 / 180.0) * p[J].x);

                    nx = (float)(Math.Cos(i * 3.14159 / 180.0) * n[J].x);
                    ny = n[J].y;
                    nz = (float)(Math.Sin(i * 3.14159 / 180.0) * n[J].x);

                    norm = (float)Math.Sqrt(nx * nx * ny * ny + nz * nz);
                    nx /= norm;
                    ny /= norm;
                    nz /= norm;

                    GL.glNormal3f(nx, ny, nz);
                    GL.glTexCoord2f(i / 360.0f, p[J].y);
                    GL.glVertex3f(x0, y0, z0);

                }

            }
            GL.glEnd();
            GL.glPopMatrix();
        }

        void drawBall()
        {
            float[] a = { 0.1f, 0.1f, 0.1f, 1.00f };
            float[] d = { 1.0f, 1.0f, 1.0f, 1.00f };
            float[] s = { 1.0f, 1.0f, 1.0f, 1.00f };
            float sh = 20.0f;

            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_SPECULAR, s);
            GL.glMaterialf(GL.GL_FRONT_AND_BACK, GL.GL_SHININESS, sh);
            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_DIFFUSE, d);
            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_AMBIENT, a);

            GL.glBindTexture(GL.GL_TEXTURE_2D, Textures[ballColorIndex]);

            GL.glEnable(GL.GL_TEXTURE_GEN_S);
            GL.glEnable(GL.GL_TEXTURE_GEN_T);

            GL.glTexGeni(GL.GL_S, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_SPHERE_MAP);
            GL.glTexGeni(GL.GL_T, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_SPHERE_MAP);

            GLU.gluQuadricTexture(obj, 1);
            GLU.gluSphere(obj, 0.25, 16, 16);

            GL.glDisable(GL.GL_TEXTURE_GEN_S);
            GL.glDisable(GL.GL_TEXTURE_GEN_T);
        }

        public void changeBallColor() {
            if(ballColorIndex == 3) {
                ballColorIndex = 0;
            } else {
                ballColorIndex = 3;
            }
        }

        public void launch()
        {
            animate = true;
       
        }

        public void restart()
        {
            animate = false;
            zBall = -5.0f;
            ballXAngle = 0.0f;
        }

        public void Draw()
        {
            if (m_uint_DC == 0 || m_uint_RC == 0)
                return;

            int i, j;
            GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT);
            GL.glLoadIdentity();
            StudyKey();

            g_eyePos = new CVector4Df(g_eyePos.m_v[0] + g_eyeDir.m_v[0] * g_speed, g_eyePos.m_v[1] + g_eyeDir.m_v[1] * g_speed, g_eyePos.m_v[2] + g_eyeDir.m_v[2] * g_speed, 1.0f);
            GLU.gluLookAt(g_eyePos.m_v[0], g_eyePos.m_v[1], g_eyePos.m_v[2], g_eyePos.m_v[0] + g_eyeDir.m_v[0], g_eyePos.m_v[1] + g_eyeDir.m_v[1], g_eyePos.m_v[2] + g_eyeDir.m_v[2], 0, 1, 0);




            float[] a = { 0.1f, 0.1f, 0.1f, 1.00f };
            float [] d = { 1.0f, 1.0f, 1.0f, 1.00f };
            float [] s = { 0.4f, 0.4f, 0.4f, 1.00f };
            float sh = 1.0f;

            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_SPECULAR, s);
            GL.glMaterialf(GL.GL_FRONT_AND_BACK, GL.GL_SHININESS, sh);
            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_DIFFUSE, d);
            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_AMBIENT, a);



            // putting the light "over"
            float[] pos = { 0, 10, -6, 1.0f };
            GL.glLightfv(GL.GL_LIGHT0, GL.GL_POSITION, pos);
            GL.glTranslatef(0.0f, -2.0f, -10);						



            // enable light
            GL.glEnable(GL.GL_LIGHT0);
            GL.glEnable(GL.GL_LIGHTING);
            GL.glEnable(GL.GL_TEXTURE_2D);

            // color by default = white
            GL.glColor3f(1.0f, 1.0f, 1.0f);

            drawFloor();


            float[] pin_a = { 0.1f, 0.1f, 0.1f, 1.00f };
            float[] pin_d = { 0.8f, 0.8f, 0.8f, 1.00f };
            float[] pin_s = { 0.5f, 0.5f, 0.5f, 1.00f };
            float pin_sh = 20.0f;

            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_SPECULAR, pin_s);
            GL.glMaterialf(GL.GL_FRONT_AND_BACK, GL.GL_SHININESS, pin_sh);
            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_DIFFUSE, pin_d);
            GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_AMBIENT, pin_a);

            GL.glTranslatef(0.0f, 0.0f, -16.0f);
            drawPin(0,0);

            GL.glTranslatef(-0.2f, 0.0f, -0.4f);
            drawPin(1,-0.2f);
            GL.glTranslatef(+0.4f, 0.0f, 0.0f);
            drawPin(2, +0.2f);

            GL.glTranslatef(-0.6f, 0.0f, -0.4f);
            drawPin(3, -0.4f);
            GL.glTranslatef(+0.4f, 0.0f, 0.0f);
            drawPin(4, 0);
            GL.glTranslatef(+0.4f, 0.0f, 0.0f);
            drawPin(5, 0.4f);

            GL.glTranslatef(-1.0f, 0.0f, -0.4f);
            drawPin(6, -0.6f);
            GL.glTranslatef(+0.4f, 0.0f, 0.0f);
            drawPin(7, -0.2f);
            GL.glTranslatef(+0.4f, 0.0f, 0.0f);
            drawPin(8, 0.2f);
            GL.glTranslatef(+0.4f, 0.0f, 0.0f);
            drawPin(9, 0.6f);

            GL.glLoadIdentity();
            GLU.gluLookAt(g_eyePos.m_v[0], g_eyePos.m_v[1], g_eyePos.m_v[2], g_eyePos.m_v[0] + g_eyeDir.m_v[0], g_eyePos.m_v[1] + g_eyeDir.m_v[1], g_eyePos.m_v[2] + g_eyeDir.m_v[2], 0, 1, 0);
            if (animate)
            {
                float delta_ball = 0.1f;
                float delta_ball_angle = 1.0f;
                zBall -= delta_ball * force / 100.0f;
                ballXAngle += delta_ball_angle;
            }

            if (zBall > -28)
            {
                GL.glTranslatef(0.0f, -2.0f + 0.25f, zBall);
                GL.glRotatef(ballXAngle, 1.0f, 0.0f, 0.0f);
                drawBall();
                if (zBall <= -26.0f && zBall >= -26.4f)
                {
                    if (step[0] == 0) step[0] = MAX_STEP;
                }
                else if (zBall <= -26.4f && zBall >= -26.8f)
                {
                    if (step[1] == 0) step[1] = MAX_STEP;
                    if (step[2] == 0) step[2] = MAX_STEP;
                }
                else if (zBall <= -26.8f && zBall >= -27.2f)
                {
                    if (step[3] == 0) step[3] = MAX_STEP;
                    if (step[4] == 0) step[4] = MAX_STEP;
                    if (step[5] == 0) step[5] = MAX_STEP;
                }
                else if (zBall <= -27.2f && zBall >= -27.6f)
                {
                    if (step[6] == 0) step[6] = MAX_STEP;
                    if (step[7] == 0) step[7] = MAX_STEP;
                    if (step[8] == 0) step[8] = MAX_STEP;
                    if (step[9] == 0) step[9] = MAX_STEP;
                }
            }


            /*
	        if ( intOptionA !=4 )
              if ( intOptionA !=11 )
	           GL.glRotatef(fYRot,1.0f,2.0f,3.0f);	
	          else
	           GL.glRotatef(fYRot,1.0f,2.0f,3.0f);	
            else
	        {
	           GL.glRotatef(-50,1.0f,0.0f,0.0f);	
	           GL.glRotatef(fYRot,0.0f,0.0f,1.0f);	
	        }
	        //! TEXTURE a 
            Vector av, bv, pv, qv, nv;
	        float a=60.0f;
	        float b=60.0f;
	        GL.glEnable(GL.GL_LIGHT0);
	        GL.glEnable(GL.GL_LIGHTING);
            GL.glEnable(GL.GL_TEXTURE_2D);
            GL.glColor3f(1.0f,1.0f,1.0f); // try other combinations
            
            if (intOptionA<4)
		        GL.glBindTexture (GL.GL_TEXTURE_2D, Textures[0]);
            else
		        GL.glBindTexture (GL.GL_TEXTURE_2D, Textures[1]);
            switch( intOptionA )
	        {
		        case 1:
   	                GL.glDisable(GL.GL_LIGHTING);// in this case
			        GL.glBegin(GL.GL_QUADS);
                       if( is2x3 )
			           {
			            GL.glTexCoord2f(0,0);
				        GL.glVertex3f( -3.0f, -3.0f, -0.1f);	
				        GL.glTexCoord2f(0,3);
				        GL.glVertex3f( -3.0f, 3.0f, -0.1f);	
				        GL.glTexCoord2f(2,3);
				        GL.glVertex3f( 3.0f, 3.0f, -0.1f);	
				        GL.glTexCoord2f(2,0);
				        GL.glVertex3f( 3.0f, -3.0f, -0.1f);	
			           }
			           else
			           {
			            GL.glTexCoord2f(0,0);
				        GL.glVertex3f( -3.0f, -3.0f, -0.1f);	
				        GL.glTexCoord2f(0,1);
				        GL.glVertex3f( -3.0f, 3.0f, -0.1f);	
				        GL.glTexCoord2f(1,1);
				        GL.glVertex3f( 3.0f, 3.0f, -0.1f);	
				        GL.glTexCoord2f(1,0);
				        GL.glVertex3f( 3.0f, -3.0f, -0.1f);	
			           }
			        GL.glEnd();
		            GL.glDisable(GL.GL_TEXTURE_2D);//!!!
		            DrawColorCube();    
			        break;
		        case 2:	    
		        case 3:
                    //GL.glDisable(GL.GL_LIGHTING);
                    d = intOptionA == 2 ? 0 : d + 0.2f;
                    GL.glBegin(GL.GL_QUADS);
                    for (i = 0; i < a; i++)
                        for (j = 0; j < b; j++)
                        {
                            av.X = -3.0f + i * 6 / a;
                            av.Y = -3.0f + j * 6 / b;
                            av.Z = SurfaceFunction_f_of_xy(i - 30, j - 30, intOptionA, d);

                            bv.X = -3.0f + (i + 1) * 6 / a;
                            bv.Y = -3.0f + (j + 1) * 6 / b;
                            bv.Z = SurfaceFunction_f_of_xy(i + 1 - 30, j + 1 - 30, intOptionA, d);

                            pv = bv - av;  // find vector from the first to the second

                            bv.X = -3.0f + i * 6 / a;
                            bv.Y = -3.0f + (j + 1) * 6 / b;
                            bv.Z = SurfaceFunction_f_of_xy(i - 30, j + 1 - 30, intOptionA, d);

                            qv = bv - av;                 // find vector from the first to the third

                            nv=pv.CrossProduct(qv); // calculate the normal 
                            nv=nv.Normalize(); // normalize it, give the normal a length of 1
                            GL.glNormal3d(nv.X, nv.Y, nv.Z);
                            GL.glTexCoord2f(i / a, j / b);
                            GL.glVertex3f(-3.0f + i * 6 / a, -3.0f + j * 6 / b,
                                    SurfaceFunction_f_of_xy(i - 30, j - 30, intOptionA, d));
                            GL.glTexCoord2f(i / a, (j + 1) / b);
                            GL.glVertex3f(-3.0f + i * 6 / a, -3.0f + (j + 1) * 6 / b,
                                    SurfaceFunction_f_of_xy(i - 30, j + 1 - 30, intOptionA, d));
                            GL.glTexCoord2f((i + 1) / a, (j + 1) / b);
                            GL.glVertex3f(-3.0f + (i + 1) * 6 / a, -3.0f + (j + 1) * 6 / b,
                                    SurfaceFunction_f_of_xy(i + 1 - 30, j + 1 - 30, intOptionA, d));
                            GL.glTexCoord2f((i + 1) / a, j / b);
                            GL.glVertex3f(-3.0f + (i + 1) * 6 / a, -3.0f + j * 6 / b,
                                    SurfaceFunction_f_of_xy(i + 1 - 30, j - 30, intOptionA, d));
                        }
                    GL.glEnd();
			        break;
	            case 4: 
	            case 5: 
	            case 6: 
	            case 7: 
	            case 8: 
	            case 9: 
	            case 10:
                    //GL.glDisable(GL.GL_LIGHTING);// in this case
                    GLU.gluQuadricTexture(obj, 1);
			        switch(intOptionA)
			        {
			        case 4:
				        GLU.gluSphere( obj, 2, 32, 32 );
				        break;
			        case 5:
				        GL.glTranslatef(0.0f, 0.0f, -2);
                        GL.glRotatef(45,0.0f, 0.0f, -2);
                        GLU.gluCylinder(obj, 2, 2, 4, 32, 32);
                        GL.glRotatef(-45, 0.0f, 0.0f, -2);
                        GL.glTranslatef(0.0f, 0.0f, 2);						
				        break;
			        case 9:
				        GLU.gluPartialDisk( obj, 1,4, 20, 20,0,90);
				        break;
			        case 10:
				        GLU.gluDisk( obj, 1,4, 60, 20);
				        break;
			        case 6:
				        //If enabled, the s texture coordinate is computed 
				        //using the texture generation
				        //function defined with GL.glTexGen.
				        //If disabled, the current s texture coordinate is used.
				        GL.glEnable (GL.GL_TEXTURE_GEN_S);
				        //If enabled, the t texture coordinate is computed 
				        //using the texture generation
				        //function defined with GL.glTexGen. '
				        //If disabled, the current t texture coordinate is used.
				        GL.glEnable (GL.GL_TEXTURE_GEN_T);
			            GL.glTexGeni (GL.GL_S, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_OBJECT_LINEAR);
                        GL.glTexGeni(GL.GL_T, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_OBJECT_LINEAR);
        				
				        Draw3Dsystem();

				        GL.glDisable (GL.GL_TEXTURE_GEN_S);
				        GL.glDisable (GL.GL_TEXTURE_GEN_T);
				        break;
			        case 7:
				        //If enabled, the s texture coordinate is computed 
				        //using the texture generation
				        //function defined with GL.glTexGen.
				        //If disabled, the current s texture coordinate is used.
				        GL.glEnable (GL.GL_TEXTURE_GEN_S);
				        //If enabled, the t texture coordinate is computed 
				        //using the texture generation
				        //function defined with GL.glTexGen. '
				        //If disabled, the current t texture coordinate is used.
				        GL.glEnable (GL.GL_TEXTURE_GEN_T);

                        GL.glTexGeni(GL.GL_S, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_SPHERE_MAP);
                        GL.glTexGeni(GL.GL_T, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_SPHERE_MAP);
        				
				        Draw3Dsystem();

				        GL.glDisable (GL.GL_TEXTURE_GEN_S);
				        GL.glDisable (GL.GL_TEXTURE_GEN_T);
				        break;
			        case 8:
				        //If enabled, the s texture coordinate is computed 
				        //using the texture generation
				        //function defined with GL.glTexGen.
				        //If disabled, the current s texture coordinate is used.
				        GL.glEnable (GL.GL_TEXTURE_GEN_S);
				        //If enabled, the t texture coordinate is computed 
				        //using the texture generation
				        //function defined with GL.glTexGen. '
				        //If disabled, the current t texture coordinate is used.
				        GL.glEnable (GL.GL_TEXTURE_GEN_T);

                        GL.glTexGeni(GL.GL_S, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_EYE_LINEAR);
                        GL.glTexGeni(GL.GL_T, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_EYE_LINEAR);
        				
				        Draw3Dsystem();

				        GL.glDisable (GL.GL_TEXTURE_GEN_S);
				        GL.glDisable (GL.GL_TEXTURE_GEN_T);
				        break;
			        }

                    break;
		        case 11:
                    //before transforms
                    //  hence it is in const position
                    //GL.glDisable(GL.GL_LIGHTING);// in this case
                    GL.glBindTexture(GL.GL_TEXTURE_2D, Textures[2]);
			        //If enabled, the s texture coordinate is computed 
			        //using the texture generation
			        //function defined with GL.glTexGen.
			        //If disabled, the current s texture coordinate is used.
			        GL.glEnable (GL.GL_TEXTURE_GEN_S);
			        //If enabled, the t texture coordinate is computed 
			        //using the texture generation
			        //function defined with GL.glTexGen. '
			        //If disabled, the current t texture coordinate is used.
			        GL.glEnable (GL.GL_TEXTURE_GEN_T);

                    GL.glTexGeni(GL.GL_S, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_SPHERE_MAP);
                    GL.glTexGeni(GL.GL_T, GL.GL_TEXTURE_GEN_MODE, (int)GL.GL_SPHERE_MAP);

                    // for it #include <glut.h> and glu32.lib...
                    
                    GLUT.glutSolidTorus(0.25, 0.5, 6, 4);
                    GL.glDisable (GL.GL_TEXTURE_GEN_S);
			        GL.glDisable (GL.GL_TEXTURE_GEN_T);
			        break;

	        }
            GL.glDisable(GL.GL_TEXTURE_2D);


            fYRot +=5.0f;																				

	        // go into the screen
	        if (iDirection!=0)
		        fZ += 0.1f;
	        else
		        fZ -= 0.1f;

	        // do we need to change directions
	        if (fZ < -50.0f && iDirection==0)
		        iDirection = 1;
	        else if (fZ > -3.0f && iDirection!=0)
		        iDirection = 0;

            */
            GL.glFlush();

            WGL.wglSwapBuffers(m_uint_DC);

        }

		protected virtual void InitializeGL()
		{
			m_uint_HWND = (uint)p.Handle.ToInt32();
			m_uint_DC   = WGL.GetDC(m_uint_HWND);

            // Not doing the following WGL.wglSwapBuffers() on the DC will
			// result in a failure to subsequently create the RC.
			WGL.wglSwapBuffers(m_uint_DC);

			WGL.PIXELFORMATDESCRIPTOR pfd = new WGL.PIXELFORMATDESCRIPTOR();
			WGL.ZeroPixelDescriptor(ref pfd);
			pfd.nVersion        = 1; 
			pfd.dwFlags         = (WGL.PFD_DRAW_TO_WINDOW |  WGL.PFD_SUPPORT_OPENGL |  WGL.PFD_DOUBLEBUFFER); 
			pfd.iPixelType      = (byte)(WGL.PFD_TYPE_RGBA);
			pfd.cColorBits      = 32;
			pfd.cDepthBits      = 32;
			pfd.iLayerType      = (byte)(WGL.PFD_MAIN_PLANE);

			int pixelFormatIndex = 0;
			pixelFormatIndex = WGL.ChoosePixelFormat(m_uint_DC, ref pfd);
			if(pixelFormatIndex == 0)
			{
				MessageBox.Show("Unable to retrieve pixel format");
				return;
			}

			if(WGL.SetPixelFormat(m_uint_DC,pixelFormatIndex,ref pfd) == 0)
			{
				MessageBox.Show("Unable to set pixel format");
				return;
			}
			//Create rendering context
			m_uint_RC = WGL.wglCreateContext(m_uint_DC);
			if(m_uint_RC == 0)
			{
				MessageBox.Show("Unable to get rendering context");
				return;
			}
			if(WGL.wglMakeCurrent(m_uint_DC,m_uint_RC) == 0)
			{
				MessageBox.Show("Unable to make rendering context current");
				return;
			}


            initRenderingGL();
        }

        public void OnResize()
        {
            Width = p.Width;
            Height = p.Height;
            GL.glViewport(0, 0, Width, Height);
            Draw();
        }

        protected virtual void initRenderingGL()
		{
			if(m_uint_DC == 0 || m_uint_RC == 0)
				return;
			if(this.Width == 0 || this.Height == 0)
				return;
            GL.glClearColor(0.0f, 0.0f, 0.0f, 0.0f);
            GL.glEnable(GL.GL_DEPTH_TEST);
            GL.glDepthFunc(GL.GL_LEQUAL);

            GL.glViewport(0, 0, this.Width, this.Height);
            //GL.glClearColor(0, 0, 0, 0); 
			GL.glMatrixMode ( GL.GL_PROJECTION );
			GL.glLoadIdentity();

            //! TEXTURE 1a 
            //GL.glEnable(GL.GL_COLOR_MATERIAL);
            //float[] emis ={ 0.3f, 0.3f, 0.3f, 1 };
            //GL.glMaterialfv(GL.GL_FRONT_AND_BACK, GL.GL_EMISSION, emis);
            //! TEXTURE 1a 

            
            
	        GL.glShadeModel(GL.GL_SMOOTH);								
	        GLU.gluPerspective(45.0f,(float)Width/(float)Height,0.45f,100.0f);


            GL.glMatrixMode ( GL.GL_MODELVIEW );
			GL.glLoadIdentity();

            //! TEXTURE 1a 
            GenerateTextures();
            //! TEXTURE 1b 
        }


        //! TEXTURE b
        public static int ntextures = 4;
        public uint[] Textures = new uint[ntextures];
        
        void GenerateTextures()
        {
            GL.glGenTextures(ntextures, Textures);
            string[] imagesName ={ "floor-wood.bmp", "metal.bmp", "pin.bmp", "ball.bmp" };
            for (int i = 0; i < ntextures; i++)
            {
                Bitmap image = new Bitmap(imagesName[i]);
                image.RotateFlip(RotateFlipType.RotateNoneFlipY); //Y axis in Windows is directed downwards, while in OpenGL-upwards
                System.Drawing.Imaging.BitmapData bitmapdata;
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);

                bitmapdata = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                GL.glBindTexture(GL.GL_TEXTURE_2D, Textures[i]);
                //2D for XYZ
                                                 //The level-of-detail number. Level 0 is the base image level
                                                        //The number of color components in the texture. 
                                                        //Must be 1, 2, 3, or 4, or one of the following 
                                                        //    symbolic constants: 
                                                        //                GL_ALPHA, GL_ALPHA4, 
                                                        //                GL_ALPHA8, GL_ALPHA12, GL_ALPHA16, GL_LUMINANCE, GL_LUMINANCE4, 
                                                        //                GL_LUMINANCE8, GL_LUMINANCE12, GL_LUMINANCE16, GL_LUMINANCE_ALPHA, 
                                                        //                GL_LUMINANCE4_ALPHA4, GL_LUMINANCE6_ALPHA2, GL_LUMINANCE8_ALPHA8, 
                                                        //                GL_LUMINANCE12_ALPHA4, GL_LUMINANCE12_ALPHA12, GL_LUMINANCE16_ALPHA16, 
                                                        //                GL_INTENSITY, GL_INTENSITY4, GL_INTENSITY8, GL_INTENSITY12, 
                                                        //                GL_INTENSITY16, GL_R3_G3_B2, GL_RGB, GL_RGB4, GL_RGB5, GL_RGB8, 
                                                        //                GL_RGB10, GL_RGB12, GL_RGB16, GL_RGBA, GL_RGBA2, GL_RGBA4, GL_RGB5_A1, 
                                                        //                GL_RGBA8, GL_RGB10_A2, GL_RGBA12, or GL_RGBA16.


                GL.glTexImage2D(GL.GL_TEXTURE_2D, 0, (int)GL.GL_RGB8, image.Width, image.Height,
                                                            //The width of the border. Must be either 0 or 1.
                                                                //The format of the pixel data
                                                                                //The data type of the pixel data
                                                                                                     //A pointer to the image data in memory
                                                              0, GL.GL_BGR_EXT, GL.GL_UNSIGNED_byte, bitmapdata.Scan0);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MIN_FILTER, (int)GL.GL_LINEAR);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_MAG_FILTER, (int)GL.GL_LINEAR);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_S, (int)GL.GL_REPEAT);
                GL.glTexParameteri(GL.GL_TEXTURE_2D, GL.GL_TEXTURE_WRAP_T, (int)GL.GL_REPEAT);

                // Combine color + texture (MODULATE)
                GL.glTexEnvf(GL.GL_TEXTURE_ENV, GL.GL_TEXTURE_ENV_MODE, GL.GL_MODULATE);

                image.UnlockBits(bitmapdata);
                image.Dispose();
            }
        }
    }

}


