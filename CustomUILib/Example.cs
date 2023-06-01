using FrooxEngine;
using FrooxEngine.UIX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomUILib
{
    internal class Example
    {
        public static void Main()
        {
            // I recommend calling this from OnEngineInit, but you can call it from anywhere
            CustomUILib.AddCustomInspector<Comment>(CommentCustomUI);
            CustomUILib.AddCustomInspector<Comment>((cm, ui) =>
            {
                CustomUILib.BuildInspectorUI(cm, ui);
                ui.Button("button 2");
            });
        }

        private static void CommentCustomUI(Comment comp, UIBuilder ui)
        {
            // Call CustomUILib.BuildInspectorUI to call the original base method
            // If you call the ACTUAL original base method your game will freeze due to an infinite loop
            ui.Text("This text renders above the default component fields, because it's created before the base method is called!");
            CustomUILib.BuildInspectorUI(comp, ui);
            ui.Button("Example Button!");
            ui.Text("The above button does absolutely nothing! It's just for fun :D");
        }
    }
}
