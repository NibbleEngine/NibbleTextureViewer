using System;
using NbCore;
using NbCore.Common;

namespace NibbleTextureViewer
{
    public class App
    {
        [STAThread]
        public static void Main()
        {
            //Initialize Engine
            Engine e = new Engine();
            RenderState.engineRef = e;
            
            TextureRenderer win = new TextureRenderer(e);
            win.Run();
        }
    }
}

