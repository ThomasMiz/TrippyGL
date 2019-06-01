﻿using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace TrippyGL
{
    /// <summary>
    /// A 2D sized OpenGL texture, stored in GPU memory as a grid of pixels
    /// </summary>
    public class Texture2D : Texture
    {
        /// <summary>The width of this texture</summary>
        public readonly int Width;

        /// <summary>The height of this texture</summary>
        public readonly int Height;

        /// <summary>The amount of samples this texture has. Most common value is 0</summary>
        public readonly int Multisample;

        internal Texture2D(int width, int height, int multisample, PixelInternalFormat pixelFormat = PixelInternalFormat.Rgba, PixelType pixelType = PixelType.UnsignedByte) : base(multisample == 0 ? TextureTarget.Texture2D : TextureTarget.Texture2DMultisample, pixelFormat, pixelType)
        {
            //BindToCurrentTextureUnit(); //texture is already bound by base constructor
            ValidateTextureSize(width, height);
            this.Width = width;
            this.Height = height;
            this.Multisample = multisample;

            if (this.Multisample == 0)
                GL.TexImage2D(this.TextureType, 0, this.PixelFormat, this.Width, this.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, IntPtr.Zero);
            else
                GL.TexImage2DMultisample(TextureTargetMultisample.Texture2DMultisample, this.Multisample, this.PixelFormat, this.Width, this.Height, true);

            GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(this.TextureType, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        internal Texture2D(string file, TextureTarget textureTarget) : base(textureTarget, PixelInternalFormat.Rgba, PixelType.UnsignedByte)
        {
            //BindToCurrentTextureUnit(); //texture is already bound by base constructor
            this.Multisample = 0;
            using (Bitmap bitmap = new Bitmap(file))
            {
                this.Width = bitmap.Width;
                this.Height = bitmap.Height;
                ValidateTextureSize(this.Width, this.Height);

                BitmapData data = bitmap.LockBits(new Rectangle(0, 0, this.Width, this.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                GL.TexImage2D(this.TextureType, 0, this.PixelFormat, this.Width, this.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, this.PixelType, data.Scan0);
                bitmap.UnlockBits(data);
            }
            GL.TexParameter(this.TextureType, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(this.TextureType, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        }

        /// <summary>
        /// Creates a Texture2D and loads all it's data from an image in the specified file
        /// </summary>
        /// <param name="file">The file containing the texture pixels data</param>
        public Texture2D(string file) : this(file, TextureTarget.Texture2D)
        {

        }

        /// <summary>
        /// Sets the data of the entire texture by copying from the specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read
        /// </summary>
        /// <param name="dataPtr">The pointer for reading the data</param>
        /// <param name="rectX">The X coordinate of the first pixel to write</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write</param>
        /// <param name="pixelDataFormat">The format of the pixel data in dataPtr. Accepted values are: Red, Rg, Rgb, Bgr, Rgba, Bgra, DepthComponent and StencilIndex</param>
        public void SetData(IntPtr dataPtr, int rectX, int rectY, int rectWidth, int rectHeight, OpenTK.Graphics.OpenGL4.PixelFormat pixelDataFormat)
        {
            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

            EnsureBoundAndActive();
            GL.TexSubImage2D(this.TextureType, 0, rectX, rectY, rectWidth, rectHeight, pixelDataFormat, this.PixelType, dataPtr);
        }

        /// <summary>
        /// Sets the data of a specified area of the texture, copying the new data from a specified array
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="data">The new texture data</param>
        /// <param name="dataOffset">The index of the first element in the data array to start reading from</param>
        /// <param name="rectX">The X coordinate of the first pixel to write</param>
        /// <param name="rectY">The Y coordinate of the first pixel to write</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to write</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to write</param>
        public void SetData<T>(T[] data, int dataOffset, int rectX, int rectY, int rectWidth, int rectHeight) where T : struct
        {
            ValidateSetOperation(data, dataOffset, rectX, rectY, rectWidth, rectHeight);

            EnsureBoundAndActive();
            GL.TexSubImage2D(this.TextureType, 0, rectX, rectY, rectWidth, rectHeight, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, ref data[dataOffset]);
        }

        /// <summary>
        /// Sets the data of the entire texture, copying the new data from a given array
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="colorData">The new texture data</param>
        public void SetData<T>(T[] colorData) where T : struct
        {
            SetData(colorData, 0, 0, 0, this.Width, this.Height);
        }

        /// <summary>
        /// Gets the data of the entire texture and copies it to a specified pointer.
        /// The pointer is not checked nor deallocated, memory exceptions may happen if you don't ensure enough memory can be read
        /// </summary>
        /// <param name="dataPtr">The pointer for writting the data</param>
        /// <param name="rectX">The X coordinate of the first pixel to read</param>
        /// <param name="rectY">The Y coordinate of the first pixel to read</param>
        /// <param name="rectWidth">The width of the rectangle of pixels to read</param>
        /// <param name="rectHeight">The height of the rectangle of pixels to read</param>
        /// <param name="pixelDataFormat">The format of the pixel data in dataPtr. Accepted values are: Red, Rg, Rgb, Bgr, Rgba, Bgra, DepthComponent and StencilIndex</param>
        public void GetData(IntPtr dataPtr, int rectX, int rectY, int rectWidth, int rectHeight, OpenTK.Graphics.OpenGL4.PixelFormat pixelDataFormat)
        {
            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

            EnsureBoundAndActive();
            GL.GetTexImage(this.TextureType, 0, pixelDataFormat, this.PixelType, dataPtr);
        }

        /// <summary>
        /// Gets the data of a specified area of the texture, copying the texture data to a specified array
        /// </summary>
        /// <typeparam name="T">The type of struct to save the data as. This struct's format should match the texture pixel's format</typeparam>
        /// <param name="data">The array in which to write the texture data</param>
        /// <param name="dataOffset">The index of the first element in the data array to start writing from</param>
        public void GetData<T>(T[] data, int dataOffset) where T : struct
        {
            if (Multisample != 0)
                throw new InvalidOperationException("You can't write the data of a multisampled texture");

            if (data == null)
                throw new ArgumentNullException("data", "Color data array can't be null");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", "dataOffset must be in the range [0, data.Length)");

            EnsureBoundAndActive();
            GL.GetTexImage(this.TextureType, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Rgba, this.PixelType, ref data[dataOffset]);
        }

        /// <summary>
        /// Saves this texture as an image file.
        /// You can't save multisampled textures
        /// </summary>
        /// <param name="file">The location in which to store the file</param>
        /// <param name="imageFormat">The format</param>
        public void SaveAsImage(string file, SaveImageFormat imageFormat)
        {
            if (Multisample != 0)
                throw new NotSupportedException("You can't save multisampled images. If this is a framebuffer, try blitting it to a non-multisampled framebuffer and saving that one");

            if (String.IsNullOrEmpty(file))
                throw new ArgumentException("You must specify a file name", "file");

            ImageFormat format;

            switch (imageFormat)
            {
                case SaveImageFormat.Png:
                    format = ImageFormat.Png;
                    break;
                case SaveImageFormat.Jpeg:
                    format = ImageFormat.Jpeg;
                    break;
                case SaveImageFormat.Bmp:
                    format = ImageFormat.Bmp;
                    break;
                case SaveImageFormat.Tiff:
                    format = ImageFormat.Tiff;
                    break;
                default:
                    throw new ArgumentException("Please use a proper value from SaveImageFormat", "imageFormat");
            }

            using (Bitmap b = new Bitmap(this.Width, this.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                BitmapData data = b.LockBits(new Rectangle(0, 0, this.Width, this.Height), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                EnsureBoundAndActive();
                GL.GetTexImage(this.TextureType, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
                b.UnlockBits(data);

                b.Save(file, ImageFormat.Png);
            }
        }

        private protected void ValidateTextureSize(int width, int height)
        {
            if (width <= 0 || width > maxTextureSize)
                throw new ArgumentOutOfRangeException("width", width, "Texture width must be in the range (0, MAX_TEXTURE_SIZE]");

            if (height <= 0 || height > maxTextureSize)
                throw new ArgumentOutOfRangeException("height", height, "Texture height must be in the range (0, MAX_TEXTURE_SIZE]");
        }

        private protected void ValidateSetOperation<T>(T[] data, int dataOffset, int rectX, int rectY, int rectWidth, int rectHeight) where T : struct
        {
            if (Multisample != 0)
                throw new InvalidOperationException("You can't write the data of a multisampled texture");

            if (data == null)
                throw new ArgumentNullException("data", "Color data array can't be null");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", "dataOffset must be in the range [0, data.Length)");

            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

            if (data.Length - dataOffset > rectWidth * rectHeight)
                throw new ArgumentException("Too much data was specified for the texture area to write", "data");
        }

        private protected void ValidateGetOperation<T>(T[] data, int dataOffset, int rectX, int rectY, int rectWidth, int rectHeight) where T : struct
        {
            if (Multisample != 0)
                throw new InvalidOperationException("You can't read the data of a multisampled texture");

            if (data == null)
                throw new ArgumentNullException("data", "Color data array can't be null");

            if (dataOffset < 0 || dataOffset >= data.Length)
                throw new ArgumentOutOfRangeException("dataOffset", "dataOffset must be in the range [0, data.Length)");

            ValidateRectOperation(rectX, rectY, rectWidth, rectHeight);

            if (data.Length - dataOffset < rectWidth * rectHeight)
                throw new ArgumentException("The provided data array isn't big enough for the specified texture area starting from dataOffset", "data");
        }

        private protected void ValidateRectOperation(int rectX, int rectY, int rectWidth, int rectHeight)
        {
            if (rectX < 0 || rectY >= this.Height)
                throw new ArgumentOutOfRangeException("rectX", rectX, "rectX must be in the range [0, Width)");

            if (rectY < 0 || rectY >= this.Height)
                throw new ArgumentOutOfRangeException("rectY", rectY, "rectY must be in the range [0, Height)");

            if (rectWidth > this.Width - rectX)
                throw new ArgumentOutOfRangeException("rectWidth", rectWidth, "rectWidth is too large");

            if (rectHeight > this.Height - rectY)
                throw new ArgumentOutOfRangeException("rectHeight", rectHeight, "rectHeight is too large");
        }
    }
}