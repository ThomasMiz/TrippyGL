﻿using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.IO;
using System.Runtime.InteropServices;
using TrippyGL;

namespace TrippyTesting
{
    class Game2 : GameWindow
    {
        System.Diagnostics.Stopwatch stopwatch;
        Random r = new Random();
        float time;

        ShaderProgram program;
        ShaderUniform worldUniform, viewUniform, projUniform, sampUniform, timeUniform;

        VertexBuffer<VertexJeje> vertexBuffer;

        Texture2D yarn, jeru, texture, invernadero, fondo;

        public Game2() : base(1280, 720, new GraphicsMode(new ColorFormat(8,8,8,8), 24, 0, 8, ColorFormat.Empty, 2), "T R I P P Y", GameWindowFlags.Default, DisplayDevice.Default, 4, 0, GraphicsContextFlags.Default)
        {
            VSync = VSyncMode.On;
            TrippyLib.Init();

            Console.WriteLine(String.Concat("GL Version: ", TrippyLib.GLMajorVersion, ".", TrippyLib.GLMinorVersion));
            Console.WriteLine("GL Version String: " + TrippyLib.GLVersion);
            Console.WriteLine("GL Vendor: " + TrippyLib.GLVendor);
            Console.WriteLine("GL Renderer: " + TrippyLib.GLRenderer);
            Console.WriteLine("GL ShadingLanguageVersion: " + TrippyLib.GLShadingLanguageVersion);
            Console.WriteLine("GL TextureUnits: " + TrippyLib.MaxTextureImageUnits);
            Console.WriteLine("GL MaxTextureSize: " + TrippyLib.MaxTextureSize);
            Console.WriteLine("GL MaxSamples:" + TrippyLib.MaxSamples);
        }

        protected override void OnLoad(EventArgs e)
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            VertexJeje[] vboData = new VertexJeje[]
            {
                new VertexJeje(new Vector3(-0.5f, -0.5f, 0), new Color4b(255, 0, 0, 255), new Vector2(0, 1)),
                new VertexJeje(new Vector3(-0.5f, 0.5f, 0), new Color4b(0, 255, 0, 255), new Vector2(0, 0)),
                new VertexJeje(new Vector3(0.5f, -0.5f, 0), new Color4b(0, 0, 255, 255), new Vector2(1, 1)),
                //new VertexColorTexture(new Vector3(0.5f, 0.5f, 0), new Color4b(255, 255, 255, 255), new Vector2(1, 0)),
            };
            vertexBuffer = new VertexBuffer<VertexJeje>(vboData.Length, vboData, BufferUsageHint.DynamicDraw);

            program = new ShaderProgram();
            program.AddVertexShader(File.ReadAllText("data2/vs.glsl"));
            program.AddGeometryShader(File.ReadAllText("data2/gs.glsl"));
            program.AddFragmentShader(File.ReadAllText("data2/fs.glsl"));
            program.SpecifyVertexAttribs<VertexJeje>(new string[] {"vPosition", "vColor", "vTexCoords" });
            program.LinkProgram();
            Console.WriteLine("Program info log: \n" + GL.GetProgramInfoLog(program.Handle) + "\n[END OF LOG]");

            worldUniform = program.Uniforms["World"];
            viewUniform = program.Uniforms["View"];
            projUniform = program.Uniforms["Projection"];
            sampUniform = program.Uniforms["samp"];
            timeUniform = program.Uniforms["time"];

            Matrix4 ide = Matrix4.Identity;
            worldUniform.SetValueMat4(ref ide);
            viewUniform.SetValueMat4(ref ide);
            projUniform.SetValueMat4(ref ide);

            yarn = new Texture2D("data/YARN.png");
            jeru = new Texture2D("data/jeru.png");
            texture = new Texture2D("data/texture.png");
            invernadero = new Texture2D("data/invernadero.png");
            fondo = new Texture2D("data/fondo.png");

            Texture2D[] hehe = new Texture2D[]
            {
                yarn, fondo, invernadero, texture, jeru
            };
            sampUniform.SetValueTextureArray(hehe, 0, 0, hehe.Length);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            time = (float)stopwatch.Elapsed.TotalSeconds;
            ErrorCode c;
            while ((c = GL.GetError()) != ErrorCode.NoError)
            {
                Console.WriteLine("Error found: " + c);
            }
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.Disable(EnableCap.CullFace);

            GL.ClearColor(0.2f, 0.2f, 0.2f, 1f);
            GL.ClearDepth(1f);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            BlendMode.AlphaBlend.Apply();
            
            vertexBuffer.EnsureArrayBound();
            program.EnsureInUse();
            program.Uniforms.EnsureSamplerUniformsSet();
            timeUniform.SetValue1(time);
            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, vertexBuffer.StorageLength);

            SwapBuffers();

            int slp = (int)(15f - (stopwatch.Elapsed.TotalSeconds - time) * 1000f);
            if (slp >= 0)
                System.Threading.Thread.Sleep(slp);
        }

        protected override void OnUnload(EventArgs e)
        {
            program.Dispose();
            vertexBuffer.Dispose();

            yarn.Dispose();
            jeru.Dispose();
            texture.Dispose();
            invernadero.Dispose();
            fondo.Dispose();

            TrippyLib.Quit();
        }

        protected override void OnResize(EventArgs e)
        {
            GL.Viewport(0, 0, this.Width, this.Height);
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    struct VertexJeje : IVertex
    {
        public Vector3 Position;
        public byte R, G, B;
        Vector2 TexCoords;

        public VertexJeje(Vector3 position, Color4b color, Vector2 texCoords)
        {
            this.Position = position;
            this.R = color.R;
            this.G = color.G;
            this.B = color.B;
            this.TexCoords = texCoords;
        }

        public VertexAttribDescription[] AttribDescriptions
        {
            get
            {
                return new VertexAttribDescription[]
                {
                    new VertexAttribDescription(ActiveAttribType.FloatVec3),
                    new VertexAttribDescription(ActiveAttribType.FloatVec3, true, VertexAttribPointerType.UnsignedByte),
                    new VertexAttribDescription(ActiveAttribType.FloatVec2)
                };
            }
        }
    }
}