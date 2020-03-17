using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;

namespace TrippyGL
{
    /// <summary>
    /// A list of <see cref="ShaderUniform"/> belonging to a <see cref="ShaderProgram"/>.
    /// This class also does some controlling over these uniforms to make everything run nicely.
    /// </summary>
    public class ShaderUniformList
    {
        /// <summary>The <see cref="ShaderProgram"/> the uniforms belong to.</summary>
        public readonly ShaderProgram Program;

        /// <summary>All of the (non-block) uniforms from the <see cref="ShaderProgram"/>.</summary>
        private readonly ShaderUniform[] uniforms;

        /// <summary>The amount of <see cref="ShaderUniform"/>-s in the <see cref="ShaderProgram"/>.</summary>
        public int Count => uniforms.Length;

        /// <summary>Gets the unsorted <see cref="ShaderUniform"/>-s from this list.</summary>
        public ReadOnlyMemory<ShaderUniform> Uniforms => uniforms;

        /// <summary>A not-always-correct list with all the textures currently applied to the sampler uniforms.</summary>
        private readonly List<Texture> textureList;

        /// <summary>
        /// Whether the <see cref="textureList"/> is up to date or needs to be remade.<para/>
        /// sampler <see cref="ShaderUniform"/>-s set this to true when their value has changed so the list gets
        /// remade the next time it is needed.
        /// </summary>
        internal bool isTextureListDirty;

        /// <summary>Whether this list contains at least one sampler-type (or sampler-array-type) <see cref="ShaderUniform"/>.</summary>
        private readonly bool hasSamplerUniforms;

        /// <summary>
        /// Gets a <see cref="ShaderUniform"/> by name. If there's no such name, returns an empty <see cref="ShaderUniform"/>.
        /// </summary>
        /// <param name="name">The name (as declared in the shaders) of the <see cref="ShaderUniform"/> to get.</param>
        public ShaderUniform this[string name]
        {
            get
            {
                for (int i = 0; i < uniforms.Length; i++)
                    if (uniforms[i].Name == name)
                        return uniforms[i];

                return default;
            }
        }

        private ShaderUniformList(ShaderProgram program, int totalUniformCount, int totalUniformBlockCount)
        {
            Program = program;
            uniforms = new ShaderUniform[totalUniformCount - totalUniformBlockCount];

            int samplerUniformsTextureCount = 0;
            int arrIndex = 0;
            for (int i = 0; i < totalUniformCount; i++)
            {
                string name = GL.GetActiveUniform(program.Handle, i, out int size, out ActiveUniformType type);
                int location = GL.GetUniformLocation(program.Handle, name);

                if (location < 0) //If the location is -1, then it's probably a uniform block so let's not add it to the uniform list
                    continue;

                ShaderUniform uniform = new ShaderUniform(program, location, name, size, type);
                uniforms[arrIndex++] = uniform;
                if (uniform.IsSamplerType)
                    samplerUniformsTextureCount += uniform.Size;
            }

            if (samplerUniformsTextureCount == 0) // If there are no sampler uniforms, then we mark this as false and don't
                hasSamplerUniforms = false; // create any of the sampler uniform variables nor do any of their processes
            else
            {
                hasSamplerUniforms = true;
                textureList = new List<Texture>(samplerUniformsTextureCount);
                isTextureListDirty = true;
            }
        }

        /// <summary>
        /// When using sampler uniforms, this will make sure they all work together properly.
        /// This is called by <see cref="ShaderProgram.EnsurePreDrawStates"/> after the program is ensured to be in use.
        /// </summary>
        internal void EnsureSamplerUniformsSet()
        {
            // Quick explanation of this method:
            // This method binds all the textures needed for the ShaderProgram's sampler-type uniforms to different texture units.
            // Then, it tells each ShaderUniform to ensure the texture units it's samplers are using are the correct one for their textures.
            // This is necessary because else, when using multiple samplers, you can't ensure they will all be using the correct texture.

            if (hasSamplerUniforms)
            {
                if (isTextureListDirty)
                    RemakeTextureList();

                Program.GraphicsDevice.BindAllTextures(textureList);

                for (int i = 0; i < uniforms.Length; i++)
                    if (uniforms[i].IsSamplerType)
                        uniforms[i].ApplyUniformTextureValues();
            }
        }

        /// <summary>
        /// Recreates the <see cref="textureList"/> list. This is, clears it and then adds all the
        /// sampler uniform's texture values avoiding duplicates, then marks the list as not dirty.
        /// </summary>
        private void RemakeTextureList()
        {
            textureList.Clear();

            for (int i = 0; i < uniforms.Length; i++)
            {
                if (!uniforms[i].IsSamplerType)
                    continue;

                ReadOnlySpan<Texture> textures = uniforms[i].Textures;
                for (int c = 0; c < textures.Length; c++)
                {
                    Texture t = textures[c];
                    if (t != null && !textureList.Contains(t))
                        textureList.Add(t);
                }
            }

            isTextureListDirty = false;
        }

        public override string ToString()
        {
            return string.Concat(nameof(Count) + "=", Count.ToString());
        }

        /// <summary>
        /// Creates a <see cref="ShaderUniformList"/> and queries the uniforms for a given <see cref="ShaderProgram"/>.<para/>
        /// The <see cref="ShaderProgram"/> must already have had it's block uniforms queried prior to this.
        /// If there are no uniforms, this method returns 
        /// </summary>
        internal static ShaderUniformList CreateForProgram(ShaderProgram program)
        {
            GL.GetProgram(program.Handle, GetProgramParameterName.ActiveUniforms, out int totalUniformCount);
            int totalUniformBlockCount = program.BlockUniforms == null ? 0 : program.BlockUniforms.TotalUniformCount;

            if (totalUniformCount - totalUniformBlockCount == 0)
                return null;
            return new ShaderUniformList(program, totalUniformCount, totalUniformBlockCount);
        }
    }
}