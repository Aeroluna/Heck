using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Chroma.Utils {
    public static class ReflectionUtil {

        [Obsolete]
        public static void SetPrivateField(this object obj, string fieldName, object value) {
            obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(obj, value);
        }

        [Obsolete]
        public static T GetPrivateField<T>(this object obj, string fieldName) {
            return (T)((object)obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(obj));
        }

        [Obsolete]
        public static void SetPrivateProperty(this object obj, string propertyName, object value) {
            obj.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(obj, value, null);
        }

        [Obsolete]
        public static void InvokePrivateMethod(this object obj, string methodName, object[] methodParams) {
            obj.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(obj, methodParams);
        }

        /*[Obsolete]
        public static Component CopyComponent(Component original, Type originalType, Type overridingType, GameObject destination) {
            Component component = destination.AddComponent(overridingType);
            FieldInfo[] fields = originalType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField);
            for (int i = 0; i < fields.Length; i++) {
                FieldInfo fieldInfo = fields[i];
                fieldInfo.SetValue(component, fieldInfo.GetValue(original));
            }
            return component;
        }*/

        private const BindingFlags _allBindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        //Sets the value of a (static?) field in object "obj" with name "fieldName"
        public static void SetField(this object obj, string fieldName, object value) {
            (obj is Type ? (Type)obj : obj.GetType())
                .GetField(fieldName, _allBindingFlags)
                .SetValue(obj, value);
        }

        //Gets the value of a (static?) field in object "obj" with name "fieldName"
        public static object GetField(this object obj, string fieldName) {
            return (obj is Type ? (Type)obj : obj.GetType())
                .GetField(fieldName, _allBindingFlags)
                .GetValue(obj);
        }

        //Gets the value of a (static?) field in object "obj" with name "fieldName" (TYPED)
        public static T GetField<T>(this object obj, string fieldName) => (T)GetField(obj, fieldName);

        //Sets the value of a (static?) Property specified by the object "obj" and the name "propertyName"
        public static void SetProperty(this object obj, string propertyName, object value) {
            (obj is Type ? (Type)obj : obj.GetType())
                .GetProperty(propertyName, _allBindingFlags)
                .SetValue(obj, value, null);
        }

        //Gets the value of a (static?) Property specified by the object "obj" and the name "propertyName"
        public static object GetProperty(this object obj, string propertyName) {
            return (obj is Type ? (Type)obj : obj.GetType())
                .GetProperty(propertyName, _allBindingFlags)
                .GetValue(obj);
        }

        //Gets the value of a (static?) Property specified by the object "obj" and the name "propertyName" (TYPED)
        public static T GetProperty<T>(this object obj, string propertyName) => (T)GetProperty(obj, propertyName);

        //Invokes a (static?) private method with name "methodName" and params "methodParams", returns an object of the specified type
        public static T InvokeMethod<T>(this object obj, string methodName, params object[] methodParams) => (T)InvokeMethod(obj, methodName, methodParams);

        //Invokes a (static?) private method with name "methodName" and params "methodParams"
        public static object InvokeMethod(this object obj, string methodName, params object[] methodParams) {
            return (obj is Type ? (Type)obj : obj.GetType())
                .GetMethod(methodName, _allBindingFlags)
                .Invoke(obj, methodParams);
        }

        //Returns a constructor with the specified parameters to the specified type or object
        public static object InvokeConstructor(this object obj, params object[] constructorParams) {
            Type[] types = new Type[constructorParams.Length];
            for (int i = 0; i < constructorParams.Length; i++) types[i] = constructorParams[i].GetType();
            return (obj is Type ? (Type)obj : obj.GetType())
                .GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, types, null)
                .Invoke(constructorParams);
        }

        //Returns a Type object which can be used to invoke static methods with the above helpers
        public static Type GetStaticType(string clazz) {
            return Type.GetType(clazz);
        }

        //Returns a list (of strings) of the names of all loaded assemblies
        public static IEnumerable<Assembly> ListLoadedAssemblies() {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        //Returns a list of all loaded namespaces
        //TODO: Check up on time complexity here, could potentially be parallelized
        public static IEnumerable<string> ListNamespacesInAssembly(Assembly assembly) {
            IEnumerable<string> ret = Enumerable.Empty<string>();
            ret = ret.Concat(assembly.GetTypes()
                    .Select(t => t.Namespace)
                    .Distinct()
                    .Where(n => n != null));
            return ret.Distinct();
        }

        //Returns a list of classes in a namespace
        //TODO: Check up on time complexity here, could potentially be parallelized
        public static IEnumerable<string> ListClassesInNamespace(string ns) {
            //For each loaded assembly
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                //If the assembly contains the desired namespace
                if (assembly.GetTypes().Where(t => t.Namespace == ns).Any()) {
                    //Select the types we want from the namespace and return them
                    return assembly.GetTypes()
                        .Where(t => t.IsClass)
                        .Select(t => t.Name);
                }
            }
            return null;

            //Code to list reflectable classes
            /*
            ReflectionUtil.ListLoadedAssemblies().ToList().ForEach(x => {
                if (x.GetName().Name == "BeatSaberMultiplayer")
                {
                    Logger.Success($"ASSEMBLY: {x.GetName().Name}");
                    ReflectionUtil.ListNamespacesInAssembly(x).ToList().ForEach(y =>
                    {
                        Logger.Warning($"NAMESPACE: {y}");
                        ReflectionUtil.ListClassesInNamespace(y).ToList().ForEach(z =>
                        {
                            Logger.Warning($"CLASS: {z} : {((ReflectionUtil.GetStaticType(y + "." + z + "," + x) != null) ? "REFLECTABLE" : "NOT")}");
                        });
                    });
                }
            });
            */
        }

        //(Created by taz?) Copies a component to a destination object, keeping all its field values?
        public static Behaviour CopyComponent(Behaviour original, Type originalType, Type overridingType, GameObject destination) {
            Behaviour copy = null;

            try {
                copy = destination.AddComponent(overridingType) as Behaviour;
            } catch (Exception) {

            }

            copy.enabled = false;

            //Copy types of super classes as well as our class
            Type type = originalType;
            while (type != typeof(MonoBehaviour)) {
                CopyForType(type, original, copy);
                type = type.BaseType;
            }

            copy.enabled = true;
            return copy;
        }

        //(Created by taz?) Copies a Component of Type type, and all its fields
        private static void CopyForType(Type type, Component source, Component destination) {
            FieldInfo[] myObjectFields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField);

            foreach (FieldInfo fi in myObjectFields) {
                fi.SetValue(destination, fi.GetValue(source));
            }
        }
    }
}
