﻿using System;

namespace TrippyGL
{
    public struct Rectangle
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public static bool operator ==(Rectangle x, Rectangle y)
        {
            return x.X == y.X && x.Y == y.Y && x.Width == y.Width && x.Height == y.Height;
        }

        public static bool operator !=(Rectangle x, Rectangle y)
        {
            return x.X != y.X || x.Y != y.Y || x.Width != y.Width || x.Height != y.Height;
        }

        public Rectangle(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public bool Equals(Rectangle other)
        {
            return other == this;
        }

        public override bool Equals(object obj)
        {
            if (obj is Rectangle r)
                return Equals(r);
            return false;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Width.GetHashCode() ^ Height.GetHashCode();
        }

        public override string ToString()
        {
            return String.Concat("{X=", X.ToString(), ", Y=", Y.ToString(), ", Width=" + Width.ToString(), ", Height=", Height.ToString(), "}");
        }
    }
}