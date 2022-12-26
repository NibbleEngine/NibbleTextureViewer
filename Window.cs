using NbCore.Common;
using NbCore.Platform.Windowing;
using NbCore;
using OpenTK.Windowing.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NibbleTextureViewer
{
    public class TextureRenderer : NbOpenGLWindow
    {
        private NbTexture _texture; //Texture handle
        private NbCore.Math.NbVector2 offset = new(0.0f);

        //Application Layers
        private ApplicationLayerStack stack;
        private RenderLayer _renderLayer;
        private UILayer _UILayer;
        private NbLogger _logger = new();

        public TextureRenderer(Engine e) : base(new NbCore.Math.NbVector2i(900, 600), e)
        {
            Title = "Nibble Texture Viewer " + Version.GetString();
            SetVSync(true);
            SetRenderFrameFrequency(30);
            Callbacks.Log = _logger.Log;

            //Register Methods
            OnWindowLoad += Load;
            OnRenderUpdate += RenderUpdate;
            OnFrameUpdate += (double dt) =>
            {
                //pass
            };
        }

        private void OnCloseWindowEvent(object sender, string data)
        {
            Console.WriteLine("EVENT TRIGGERED");
            Console.WriteLine(data);
        }

        private void OpenFile(object sender, string filepath)
        {
            if (_texture != null)
                _texture.Dispose();

            _texture = Engine.CreateTexture(filepath, NbTextureWrapMode.Repeat, NbTextureFilter.NearestMipmapLinear, NbTextureFilter.Nearest);
            _renderLayer.SetTexture(_texture);
            _UILayer.SetTexture(_texture);
        }

        private void ImportTextureLayer(object sender, string filepath, int depth_id)
        {
            //Layer Texture
            NbTexture layer = new NbTexture(filepath);
            DDSImage.ReplaceTextureLayer((DDSImage)layer.Data,
                                         (DDSImage)_texture.Data,
                                         depth_id);
            layer.Dispose();
            NbCore.Platform.Graphics.GraphicsAPI.UploadTexture(_texture);
        }

        private void Load()
        {
            Callbacks.SetDefaultCallbacks();

            //Init Engine
            Engine.Init();
            
            //Initialize Application Layers
            _renderLayer = new(this, Engine); //render layer
            _UILayer = new(this, Engine);

            //Organize them into a stack
            stack = new();
            stack.AddApplicationLayer(_renderLayer);
            stack.AddApplicationLayer(_UILayer);

            //Subscribe to layer events
            _UILayer.CloseWindowEvent += OnCloseWindowEvent;
            _UILayer.OpenFileEvent += OpenFile;
            _UILayer.ImportLayerEvent += ImportTextureLayer;
            _UILayer.ConsumeInputEvent += _renderLayer.CaptureInput;
            _UILayer.RenderTextureDataChanged += _renderLayer.OnRenderTextureDataChanged;
            OnResize += _UILayer.OnResize;
            OnResize += _renderLayer.OnResize;

            //GL.Enable(EnableCap.DepthTest);

            //Setup Texture
            //string texturepath = "E:\\SteamLibrary1\\steamapps\\common\\No Man's Sky\\GAMEDATA\\TEXTURES\\PLANETS\\BIOMES\\WEIRD\\BEAMSTONE\\BEAMGRADIENT.DDS";
            //string texturepath = "E:\\SteamLibrary1\\steamapps\\common\\No Man's Sky\\GAMEDATA\\TEXTURES\\PLANETS\\BIOMES\\WEIRD\\BEAMSTONE\\SCROLLINGCLOUD.DDS";
            //string texturepath = "E:\\SSD_SteamLibrary1\\steamapps\\common\\No Man's Sky\\GAMEDATA\\TEXTURES\\COMMON\\ROBOTS\\QUADRUPED.DDS";
            //string texturepath = "D:\\Downloads\\TILEMAP.DDS";
            //string texturepath = "D:\\Downloads\\TILEMAP.HSV.DDS";
            //string texturepath = "D:\\Downloads\\TILEMAP.NORMAL.DDS";

            //_texture = new Texture(Callbacks.getResource("default.dds"), 
            //                       true, "default");

        }

        private void RenderUpdate(double dt)
        {
            //Prepare Engine Layer Data Queue
            stack.OnRenderFrame(dt);
        }

        public void DisposeLayers()
        {
            _renderLayer.Dispose();

        }

        
    }
}
