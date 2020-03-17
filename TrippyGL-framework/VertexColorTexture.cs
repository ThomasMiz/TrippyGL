using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace TrippyGL
{
    /// <summary>
    /// Represents a vertex with <see cref="Vector3"/> Position, <see cref="Color4b"/> Color and <see cref="Vector2"/> TexCoords.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexColorTexture : IVertex, IEquatable<VertexColorTexture>
    {
        /// <summary>The size of a <see cref="VertexColorTexture"/> measured in bytes.</summary>
        public const int SizeInBytes = (3 + 1 + 2) * 4;

        /// <summary>The vertex's position.</summary>
        public Vector3 Position;

        /// <summary>The vertex's color.</summary>
        public Color4b Color;

        /// <summary>The vertex's texture coordinates.</summary>
        public Vector2 TexCoords;

        /// <summary>
        /// Creates a <see cref="VertexColorTexture"/> with the specified position, color and texture coordinates.
        /// </summary>
        public VertexColorTexture(Vector3 position, Color4b color, Vector2 texCoords)
        {
            Position = position;
            Color = color;
            TexCoords = texCoords;
        }

        /// <summary>
        /// Creates a <see cref="VertexColorTexture"/> with the specified position and texture coordinates, and white color.
        /// </summary>
        public VertexColorTexture(Vector3 position, Vector2 texCoords)
        {
            Position = position;
            Color = new Color4b(255, 255, 255, 255);
            TexCoords = texCoords;
        }

        public static bool operator ==(VertexColorTexture left, VertexColorTexture right) => left.Equals(right);

        public static bool operator !=(VertexColorTexture left, VertexColorTexture right) => !left.Equals(right);

        public int AttribDescriptionCount => 3;

        public void WriteAttribDescriptions(Span<VertexAttribDescription> descriptions)
        {
            descriptions[0] = new VertexAttribDescription(ActiveAttribType.FloatVec3);
            descriptions[1] = new VertexAttribDescription(ActiveAttribType.FloatVec4, true, VertexAttribPointerType.UnsignedByte);
            descriptions[2] = new VertexAttribDescription(ActiveAttribType.FloatVec2);
        }

        public override string ToString()
        {
            return string.Concat("(", Position.X.ToString(), ", ", Position.Y.ToString(), ", ", Position.Z.ToString(), ") (", Color.R.ToString(), ", ", Color.G.ToString(), ", ", Color.B.ToString(), ", ", Color.A.ToString(), ") (", TexCoords.X.ToString(), ", ", TexCoords.Y.ToString(), ")");
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Position.GetHashCode();
                hashCode = (hashCode * 397) ^ Color.GetHashCode();
                hashCode = (hashCode * 397) ^ TexCoords.GetHashCode();
                return hashCode;
            }
        }

        public bool Equals(VertexColorTexture other)
        {
            return Position == other.Position
                && Color == other.Color
                && TexCoords == other.TexCoords;
        }

        public override bool Equals(object obj)
        {
            if (obj is VertexColorTexture vertexColorTexture)
                return Equals(vertexColorTexture);
            return false;
        }
    }
}