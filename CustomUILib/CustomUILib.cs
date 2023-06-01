using HarmonyLib;
using NeosModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using FrooxEngine;
using FrooxEngine.UIX;
using System.Reflection.Emit;
using BaseX;

namespace CustomUILib
{
    public class CustomUILib : NeosMod
    {
        public override string Name => "CustomUILib";
        public override string Author => "art0007i";
        public override string Version => "1.1.0";
        public override string Link => "https://github.com/art0007i/CustomUILib/";

        static List<KeyValuePair<Type, Func<Worker, UIBuilder, Predicate<ISyncMember>, IEnumerable<object>>>> injectedCustomUI = new();
        static DynamicMethod reentrantMethod;

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony("me.art0007i.CustomUILib");
            var orig = typeof(WorkerInspector).GetMethod("BuildInspectorUI");
            reentrantMethod = (DynamicMethod)harmony.Patch(orig);
            harmony.PatchAll();

            //Example.Main();
        }

        internal static IEnumerable<object> EnumerableBefore(Action<Worker, UIBuilder, Predicate<ISyncMember>> action, Worker w, UIBuilder ui, Predicate<ISyncMember> filter)
        {
            action(w, ui, filter);
            yield return null;
        }
        internal static IEnumerable<object> EnumerableAfter(Action<Worker, UIBuilder, Predicate<ISyncMember>> action, Worker w, UIBuilder ui, Predicate<ISyncMember> filter)
        {
            yield return null;
            action(w, ui, filter);
        }
        [Obsolete("Since CustomUILib 1.1.0 a different way of generating ui has been added.")]
        public static void BuildInspectorUI(Worker w, UIBuilder ui, Predicate<ISyncMember> memberFilter)
        {
            Error("Obsolete BuildInspectorUI function has been called!");
        }

        // because I changed how ui generation works, these methods are obsolete but still work (they always generate after)
        [Obsolete("Use `AddCustomInspectorAfter` or `AddCustomInspectorBefore` instead.")]
        public static void AddCustomInspector<T>(Action<T, UIBuilder> uiGenerator) where T : Worker
        {
            AddCustomInspectorAfter(uiGenerator);
        }
        [Obsolete("Use `AddCustomInspectorAfter` or `AddCustomInspectorBefore` instead.")]
        public static void AddCustomInspector<T>(Action<T, UIBuilder, Predicate<ISyncMember>> uiGenerator) where T : Worker
        {
            AddCustomInspectorAfter(uiGenerator);
        }
        [Obsolete("Use `AddCustomInspectorAfter` or `AddCustomInspectorBefore` instead.")]
        public static void AddCustomInspector(Type type, Action<Worker, UIBuilder> uiGenerator)
        {
            AddCustomInspectorAfter(uiGenerator);
        }

        public static void AddCustomInspectorBefore<T>(Action<T, UIBuilder> uiGenerator) where T : Worker
        {
            AddCustomInspector(typeof(T), (a, b, _) => uiGenerator((T)a, b), false);
        }
        public static void AddCustomInspectorBefore<T>(Action<T, UIBuilder, Predicate<ISyncMember>> uiGenerator) where T : Worker
        {
            AddCustomInspector(typeof(T), (a, b, c) => uiGenerator((T)a, b, c), false);
        }
        public static void AddCustomInspectorBefore(Type type, Action<Worker, UIBuilder> uiGenerator)
        {
            AddCustomInspector(type, (a, b, _) => uiGenerator(a, b), false);
        }
        public static void AddCustomInspectorAfter<T>(Action<T, UIBuilder> uiGenerator) where T : Worker
        {
            AddCustomInspector(typeof(T), (a, b, _) => uiGenerator((T)a, b), true);
        }
        public static void AddCustomInspectorAfter<T>(Action<T, UIBuilder, Predicate<ISyncMember>> uiGenerator) where T : Worker
        {
            AddCustomInspector(typeof(T), (a, b, c) => uiGenerator((T)a, b, c), true);
        }
        public static void AddCustomInspectorAfter(Type type, Action<Worker, UIBuilder> uiGenerator)
        {
            AddCustomInspector(type, (a, b, _) => uiGenerator(a, b), true);
        }
        public static void AddCustomInspector<T>(Func<T, UIBuilder, IEnumerable<object>> uiGenerator) where T : Worker
        {
            AddCustomInspector(typeof(T), (a, b, _) => uiGenerator((T)a, b));
        }
        public static void AddCustomInspector<T>(Func<T, UIBuilder, Predicate<ISyncMember>, IEnumerable<object>> uiGenerator) where T : Worker
        {
            AddCustomInspector(typeof(T), (a, b, c) => uiGenerator((T)a, b, c));
        }
        public static void AddCustomInspector(Type type, Func<Worker, UIBuilder, IEnumerable<object>> uiGenerator)
        {
            AddCustomInspector(type, (a, b, _) => uiGenerator(a, b));
        }
        public static void AddCustomInspector(Type type, Action<Worker, UIBuilder, Predicate<ISyncMember>> uiGenerator, bool after)
        {
            AddCustomInspector(type, (a, b, c) => after ? EnumerableAfter(uiGenerator, a, b, c) : EnumerableBefore(uiGenerator, a, b, c));
        }

        public static void AddCustomInspector(Type type, Func<Worker, UIBuilder, Predicate<ISyncMember>, IEnumerable<object>> uiGenerator)
        {
            injectedCustomUI.Add(new(type, uiGenerator));
        }



        private static void CallOriginalUIFunc(Worker worker, UIBuilder ui, Predicate<ISyncMember> memberFilter = null)
        {
            reentrantMethod.Invoke(null, new object[] { worker, ui, memberFilter });
        }
        private static bool BuildProxy(Worker comp, UIBuilder ui, Predicate<ISyncMember> memberFilter)
        {
            var list = Pool.BorrowList<IEnumerator<object>>();
            list.AddRange(injectedCustomUI.Where((pair) => pair.Key.IsAssignableFrom(comp.GetType())).Select((p) => p.Value(comp, ui, memberFilter).GetEnumerator()));
            var first = true;
            if (list.Count == 0) return false;
            while (list.Count > 0) {
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    if (!list[i].MoveNext())
                    {
                        list.RemoveAt(i);
                    }
                }
                if (first)
                {
                    CallOriginalUIFunc(comp, ui, memberFilter);
                    first = false;
                }
            }
            Pool.Return(ref list);
            return true;
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