using System;
using System.Collections.Generic;
using System.IO;
using NbCore;
using NbCore.Platform.Graphics;
using NbCore.Platform.Windowing;

using OpenTK.Windowing.Common;

namespace NibbleTextureViewer
{
    public struct RenderTextureData
    {
        public int depth_id;
        public int mipmap_id;
        public NbCore.Math.NbVector4 channelToggle;
    }

    public class RenderLayer : ApplicationLayer
    {
        private NbTexture _texture;
        private NbCore.Math.NbVector2i _size;
        private NbCore.Math.NbVector2 offset;
        private float _scale = 1.0f;
        private RenderTextureData _renderData;
        private NbShader _shaderArray;
        private NbShader _shaderVolume;
        private NbShader _shaderSingle;
        private bool _captureInput = true;

        public RenderLayer(NbWindow win, Engine engine) : base(win, engine)
        {
            //Compile Necessary Shaders
            //NbShaderConfig conf = EngineRef.CreateShaderConfig(
            //    EngineRef.GetShaderSourceByFilePath("Shaders/texture_shader_vs.glsl"),
            //    EngineRef.GetShaderSourceByFilePath("Shaders/texture_shader_fs.glsl"),
            //    null, null, null, NbShaderMode.DEFAULT, "Texture");


            //NbShaderConfig conf_multitex = EngineRef.CreateShaderConfig(
            //    EngineRef.GetShaderSourceByFilePath("Shaders/texture_shader_vs.glsl"),
            //    EngineRef.GetShaderSourceByFilePath("Shaders/texture_shader_fs.glsl"),
            //    null, null, null, NbShaderMode.DEFAULT, "MultiTexture");

            //NbShaderConfig conf_voltex = EngineRef.CreateShaderConfig(
            //    EngineRef.GetShaderSourceByFilePath("Shaders/texture_shader_vs.glsl"),
            //    EngineRef.GetShaderSourceByFilePath("Shaders/texture_shader_fs.glsl"),
            //    null, null, null, NbShaderMode.DEFAULT, "VolumeTexture");


            //Fetch shaders

            NbShaderConfig conf = EngineRef.GetShaderConfigByName("Texture");
            
            _shaderSingle = EngineRef.GetShaderByHash(EngineRef.CalculateShaderHash(conf));
            _shaderArray = EngineRef.GetShaderByHash(EngineRef.CalculateShaderHash(conf, new List<string> {"_F55_MULTITEXTURE"}));
            _shaderVolume = EngineRef.GetShaderByHash(EngineRef.CalculateShaderHash(conf, new List<string> { "_D_VOLUME_TEXTURE" }));

        }

        public void OnRenderTextureDataChanged(object sender, RenderTextureData data)
        {
            _renderData = data;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Managed state disposal
            }

            if (_texture != null)
                _texture.Dispose();

            _shaderArray.Dispose();
            base.Dispose(disposing);
        }

        public void CaptureInput(object sender, bool state)
        {
            _captureInput = !state;
            //Console.WriteLine($"CAPTURE INPUT IN RENDER LAYER: {_captureInput}");
        }

        public void SetTexture(NbTexture tex)
        {
            _texture = tex;
        }

        public void SetViewportsize(NbCore.Math.NbVector2i vec)
        {
            _size = vec;
        }

        public void OnResize(NbResizeArgs args)
        {
            _size = new NbCore.Math.NbVector2i(args.Width, args.Height);
        }

        public override void OnFrameUpdate(double dt)
        {
            
        }

        public override void OnRenderFrameUpdate(double dt)
        {
            GraphicsAPI renderer = EngineRef.GetSystem<NbCore.Systems.RenderingSystem>().Renderer;

            //Compile updated shaders
           //Re-Compile requested shaders
            while (EngineRef.GetSystem<NbCore.Systems.RenderingSystem>().ShaderMgr.ShaderCompilationQueue.Count > 0)
            {
                NbShader shader = EngineRef.GetSystem<NbCore.Systems.RenderingSystem>().ShaderMgr.ShaderCompilationQueue.Dequeue();
                EngineRef.CompileShader(shader);
            }
            
            renderer.SetBlend(true);
            renderer.Viewport(_size.X, _size.Y);
            renderer.ClearColor(new NbCore.Math.NbVector4(0.1f, 0.1f, 0.1f, 0.0f));
            renderer.ClearDrawBuffer(NbBufferMask.Color | NbBufferMask.Depth);

            if (_texture != null)
            {
                NbShader _shader;
                if (_texture.Data.target == NbTextureTarget.Texture3D)
                    _shader = _shaderVolume;
                else if ((_texture.Data.target == NbTextureTarget.Texture2DArray))
                    _shader = _shaderArray;
                else
                    _shader = _shaderSingle;


                if (_captureInput)
                {
                    //Process Input:
                    if (WindowRef.IsMouseButtonDown(NbMouseButton.LEFT))
                    {
                        offset.X += WindowRef.MouseDelta.X / (_size.X * ((float)_texture.Data.Width / _texture.Data.Height));
                        offset.Y += WindowRef.MouseDelta.Y / _size.Y;
                    }

                    _scale = Math.Max(0.05f, _scale + WindowRef.MouseScrollDelta.Y * 0.08f);

                    //Console.WriteLine($"{offset.X}, {offset.Y}, {_scale}");
                }

                //Set Shader State
                _shader.ClearCurrentState();

                _shader.CurrentState.AddSampler("InTex", new()
                {
                    SamplerID = 0,
                    Texture = _texture,
                });

                _shader.CurrentState.AddUniform("texture_depth", (float) _renderData.depth_id);
                _shader.CurrentState.AddUniform("mipmap", (float) _renderData.mipmap_id);
                _shader.CurrentState.AddUniform("aspect_ratio", (float) _texture.Data.Height / _texture.Data.Width);
                _shader.CurrentState.AddUniform("scale", _scale);
                _shader.CurrentState.AddUniform("offset", offset);
                _shader.CurrentState.AddUniform("channelToggle", _renderData.channelToggle);

                renderer.EnableShaderProgram(_shader);

                NbMesh nm = EngineRef.GetMesh(NbHasher.Hash("default_renderquad"));
                renderer.RenderQuad(nm, _shader, _shader.CurrentState);
            }

        }

    }
}
