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
            CustomUILib.AddCustomInspectorBefore<Comment>((comment, ui) =>
            {
                ui.Text("This will show above the component's default fields!");
            });
            CustomUILib.AddCustomInspectorAfter<Comment>((comment, ui) =>
            {
                ui.Text("This will show below the component's default fields!");
            });
            CustomUILib.AddCustomInspector<Comment>(CommentCustomUI);
        }

        private static IEnumerable<object> CommentCustomUI(Comment comp, UIBuilder ui)
        {
            // All custom UI functions for the same component will 'sync' at each yield
            // the first yield marks the generation of the original ui.
            ui.Text("This text renders above the default component fields, because it's created before the original fields are generated!");
            yield return null;
            ui.Button("Example Button! (below the default fields)");
            ui.Text("The above button does absolutely nothing! It's just for fun :D");
            yield return null;
            ui.Button("Example Button2!");
        }
    }
}
