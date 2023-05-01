using HarmonyLib;
using NeosModLoader;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Reflection.Emit;

namespace CustomUILib
{
    public class CustomUILib : NeosMod
    {
        public override string Name => "CustomUILib";
        public override string Author => "art0007i";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/art0007i/CustomUILib/";

        static Dictionary<Type, Action<Worker, UIBuilder, Predicate<ISyncMember>>> injectedCustomUI = new();
        static DynamicMethod reentrantMethod;

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("me.art0007i.CustomUILib");
            var orig = typeof(WorkerInspector).GetMethod("BuildInspectorUI");
            reentrantMethod = (DynamicMethod)harmony.Patch(orig);
            harmony.PatchAll();

            //Example.Main();
        }

        public static void BuildInspectorUI(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter = null)
        {
            reentrantMethod.Invoke(null, new object[] { worker, ui, memberFilter });
        }

        public static void AddCustomInspector<T>(Action<T, UIBuilder> uiGenerator) where T : Worker
        {
            AddCustomInspector(typeof(T), (a, b, _) => uiGenerator((T)a, b));
        }
        public static void AddCustomInspector<T>(Action<T, UIBuilder, Predicate<ISyncMember>> uiGenerator) where T : Worker
        {
            AddCustomInspector(typeof(T), (a, b, c) => uiGenerator((T)a, b, c));
        }
        public static void AddCustomInspector(Type type, Action<Worker, UIBuilder> uiGenerator)
        {
            AddCustomInspector(type, (a, b, _) => uiGenerator(a, b));
        }
        public static void AddCustomInspector(Type type, Action<Worker, UIBuilder, Predicate<ISyncMember>> uiGenerator)
        {
            injectedCustomUI.Add(type, uiGenerator);
        }

        private static bool BuildProxy(Worker comp, UIBuilder ui, Predicate<ISyncMember> memberFilter)
        {
            var filteredList = injectedCustomUI.Where((pair) =>
            {
                if (pair.Key.IsAssignableFrom(comp.GetType()))
                {
                    return true;
                }
                return false;
            });
            foreach (var pair in filteredList)
            {
                pair.Value(comp, ui, memberFilter);
            }
            return filteredList.Count() > 0;
        }

        [HarmonyPatch(typeof(WorkerInspector), "BuildInspectorUI")]
        class CustomUILibPatch
        {
            public static bool Prefix(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter)
            {
                if (BuildProxy(worker, ui, memberFilter)) return false;
                return true;
            }
        }
    }
}