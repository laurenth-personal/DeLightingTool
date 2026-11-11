using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UnityEditor.Experimental
{
    public enum GUILayoutType
    {
        Vertical,
        Horizontal,
    }

    public enum Alignment
    {
        None,
        HorizontalCenter,
        VerticalCenter,
        BothCenter,
    }

    public class StyleSheet
    {
        public class Style
        {
            public GUIStyle guiStyle = GUIStyle.none;
            public GUILayoutType layout = GUILayoutType.Vertical;
            public Alignment alignment = Alignment.None;
        }

        public Style GetStyleFor(string[] classes, Type type)
        {
            return new Style();
        }
    }
}

namespace UnityEditor.Experimental.MVVM
{
    public interface IClassMethod
    {
        string MethodName { get; }
        void Execute(object instance);
    }

    public interface IClassMethod<T> : IClassMethod
    {
    }

    public class ClassProperty
    {
        public string propertyName;
        public ClassProperty(string propertyName)
        {
            this.propertyName = propertyName;
        }
    }

    public class ClassProperty<T> : ClassProperty
    {
        public ClassProperty(string propertyName) : base(propertyName) { }

        public virtual T GetValue(object instance)
        {
            if (instance == null)
                return default(T);

            var type = instance.GetType();
            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
                return (T)property.GetValue(instance, null);

            var field = type.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
                return (T)field.GetValue(instance);

            return default(T);
        }

        public virtual void SetValue(object instance, T value)
        {
            if (instance == null)
                return;

            var type = instance.GetType();
            var property = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                property.SetValue(instance, value, null);
                return;
            }

            var field = type.GetField(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(instance, value);
        }

        public virtual bool ValueEquals(object instance, T value)
        {
            var current = GetValue(instance);
            return EqualityComparer<T>.Default.Equals(current, value);
        }
    }

    public class StaticClassProperty<TValue, TOwner> : ClassProperty<TValue>
    {
        public StaticClassProperty(string propertyName) : base(propertyName) { }

        public override TValue GetValue(object instance)
        {
            var type = typeof(TOwner);
            var property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
                return (TValue)property.GetValue(null, null);

            var field = type.GetField(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return field != null ? (TValue)field.GetValue(null) : default(TValue);
        }

        public override void SetValue(object instance, TValue value)
        {
            var type = typeof(TOwner);
            var property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
            {
                property.SetValue(null, value, null);
                return;
            }

            var field = type.GetField(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
                field.SetValue(null, value);
        }
    }

    public class DynamicClassProperty<T> : ClassProperty<T>
    {
        public DynamicClassProperty(string propertyName) : base(propertyName) { }
    }

    public class StaticClassMethod<TOwner> : IClassMethod
    {
        public string MethodName { get; private set; }

        public StaticClassMethod(string methodName)
        {
            MethodName = methodName;
        }

        public void Execute(object instance)
        {
            var method = typeof(TOwner).GetMethod(MethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(null, null);
        }
    }

    public class DynamicClassMethod : IClassMethod
    {
        public string MethodName { get; private set; }

        public DynamicClassMethod(string methodName)
        {
            MethodName = methodName;
        }

        public void Execute(object instance)
        {
            if (instance == null)
                return;

            var method = instance.GetType().GetMethod(MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(instance, null);
        }
    }

    public class ActionDependencyProperty<TOwner>
    {
        public string propertyName;
        public ActionDependencyProperty(string propertyName)
        {
            this.propertyName = propertyName;
        }
    }
}

namespace UnityEditor.Experimental.ViewModel
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool m_RegisteredForUpdate;
        private readonly HashSet<UnityEditor.Experimental.MVVM.ClassProperty> m_ChangedProperties = new HashSet<UnityEditor.Experimental.MVVM.ClassProperty>();

        public void SetPropertyChanged(params UnityEditor.Experimental.MVVM.ClassProperty[] properties)
        {
            if (!m_RegisteredForUpdate)
            {
                m_RegisteredForUpdate = true;
                EditorApplication.update += DoPropertyChanged;
            }
            foreach (var property in properties)
            {
                if (property != null)
                    m_ChangedProperties.Add(property);
            }
        }

        private void DoPropertyChanged()
        {
            EditorApplication.update -= DoPropertyChanged;
            m_RegisteredForUpdate = false;

            if (m_ChangedProperties.Count == 0)
                return;

            var copy = m_ChangedProperties.ToArray();
            m_ChangedProperties.Clear();

            foreach (var property in copy)
            {
                var name = property != null ? property.propertyName : string.Empty;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class SerializedViewModelBase : ViewModelBase { }
}

namespace UnityEditor.Experimental.VisualElements
{
    public interface IVisualElement : IDisposable
    {
        IVisualElement parent { get; set; }
        object dataContext { get; set; }
    }

    public interface IIMGUIVisualElement : IVisualElement
    {
        void OnGUI();
        void Repaint();
        event Action RepaintRequested;
    }

    public class Binding
    {
        public IVisualElement element;
        public string propertyName;
        public virtual bool ShouldTrigger(string propName)
        {
            return string.IsNullOrEmpty(propName) || propName == propertyName;
        }
        public virtual void SetViewPropertyValueFromContext(object dataContext) { }
    }

    public class Binding<TProp> : Binding
    {
        private readonly VisualElement m_ViewElement;
        private readonly UnityEditor.Experimental.MVVM.ClassProperty<TProp> m_VMProperty;

        public Binding(VisualElement viewElement, UnityEditor.Experimental.MVVM.ClassProperty<TProp> vProperty, UnityEditor.Experimental.MVVM.ClassProperty<TProp> vmProperty)
        {
            m_ViewElement = viewElement;
            m_VMProperty = vmProperty;
            propertyName = vmProperty.propertyName;
        }

        public Binding(VisualElement viewElement, UnityEditor.Experimental.MVVM.ClassProperty<TProp> vProperty, UnityEditor.Experimental.MVVM.IClassMethod<TProp> vmProperty)
        {
            m_ViewElement = viewElement;
            propertyName = vmProperty.MethodName;
        }

        public override void SetViewPropertyValueFromContext(object dataContext)
        {
            if (dataContext == null || m_ViewElement == null)
                return;

            var value = m_VMProperty != null ? m_VMProperty.GetValue(dataContext) : default(TProp);
            var property = m_ViewElement.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null)
                property.SetValue(m_ViewElement, value, null);
        }
    }

    public class ActionBinding<TOwner> : Binding where TOwner : IVisualElement
    {
        private readonly VisualElement m_ViewElement;
        private readonly UnityEditor.Experimental.MVVM.IClassMethod m_Method;

        public ActionBinding(VisualElement viewElement, UnityEditor.Experimental.MVVM.ActionDependencyProperty<TOwner> vProperty, UnityEditor.Experimental.MVVM.IClassMethod vmProperty)
        {
            m_ViewElement = viewElement;
            m_Method = vmProperty;
            propertyName = vmProperty.MethodName;
        }

        public override void SetViewPropertyValueFromContext(object dataContext)
        {
            if (dataContext == null)
                return;

            var method = dataContext.GetType().GetMethod(m_Method.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null)
                method.Invoke(dataContext, null);
        }
    }

    public class VisualElement : IVisualElement
    {
        private IVisualElement m_Parent;
        private object m_DataContext;
        public HashSet<string> m_Classes = new HashSet<string>();
        public float? widthOverride { get; set; }
        public float? heightOverride { get; set; }

        public IVisualElement parent
        {
            get { return m_Parent; }
            set { m_Parent = value; }
        }

        public object dataContext
        {
            get { return m_DataContext ?? (m_Parent != null ? m_Parent.dataContext : null); }
            set { m_DataContext = value; }
        }

        public string[] classes => m_Classes.ToArray();
        public bool HasClass(string c) => m_Classes.Contains(c);
        public void AddClass(params string[] classes) => m_Classes.UnionWith(classes);
        public void RemoveClass(params string[] classes) => m_Classes.ExceptWith(classes);

        public virtual void Dispose() { }

        public Binding AddBinding<TProp>(UnityEditor.Experimental.MVVM.ClassProperty<TProp> vProperty, UnityEditor.Experimental.MVVM.ClassProperty<TProp> vmProperty)
        {
            var binding = new Binding<TProp>(this, vProperty, vmProperty);
            binding.element = this;
            return binding;
        }

        public Binding AddBinding<TProp>(UnityEditor.Experimental.MVVM.ClassProperty<TProp> vProperty, UnityEditor.Experimental.MVVM.IClassMethod<TProp> vmProperty)
        {
            var binding = new Binding<TProp>(this, vProperty, vmProperty);
            binding.element = this;
            return binding;
        }

        public Binding AddBinding<TOwner>(UnityEditor.Experimental.MVVM.ActionDependencyProperty<TOwner> vProperty, UnityEditor.Experimental.MVVM.IClassMethod vmProperty) where TOwner : IVisualElement
        {
            var binding = new ActionBinding<TOwner>(this, vProperty, vmProperty);
            binding.element = this;
            return binding;
        }

        public void RemoveBinding(Binding binding) { }

        protected TPropertyType GetValue<TPropertyType>(UnityEditor.Experimental.MVVM.ClassProperty<TPropertyType> property, TPropertyType defaultValue = default(TPropertyType))
        {
            var resolved = dataContext;
            if (resolved == null)
                return defaultValue;
            try { return property.GetValue(resolved); }
            catch { return defaultValue; }
        }

        protected bool SetValue<TPropertyType>(UnityEditor.Experimental.MVVM.ClassProperty<TPropertyType> property, TPropertyType value)
        {
            var resolved = dataContext;
            if (resolved == null)
                return false;
            try
            {
                if (!property.ValueEquals(resolved, value))
                {
                    property.SetValue(resolved, value);
                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        protected void ExecuteCommand(UnityEditor.Experimental.MVVM.IClassMethod method)
        {
            var resolved = dataContext;
            if (resolved == null)
                return;
            method.Execute(resolved);
        }
    }

    public class VisualContainer : VisualElement
    {
        private readonly List<IVisualElement> m_Children = new List<IVisualElement>();
        public int childrenCount => m_Children.Count;

        public virtual void AddChild(IVisualElement element)
        {
            element.parent = this;
            m_Children.Add(element);
        }

        public virtual void RemoveChild(IVisualElement element)
        {
            m_Children.Remove(element);
            if (element != null)
                element.parent = null;
        }

        public IVisualElement GetChildAt(int index)
        {
            return index >= 0 && index < m_Children.Count ? m_Children[index] : null;
        }
    }

    public class IMGUIVisualContainer : VisualContainer, IIMGUIVisualElement
    {
        public event Action RepaintRequested;
        protected GUILayoutOption[] guiLayoutOptions => new GUILayoutOption[0];

        public virtual void OnGUI()
        {
            for (int i = 0; i < childrenCount; i++)
            {
                var child = GetChildAt(i) as IIMGUIVisualElement;
                if (child != null)
                    child.OnGUI();
            }
        }

        public void Repaint()
        {
            if (RepaintRequested != null)
                RepaintRequested();
            if (parent is IIMGUIVisualElement)
                ((IIMGUIVisualElement)parent).Repaint();
        }

        public override void Dispose()
        {
            base.Dispose();
            RepaintRequested = null;
        }
    }
}
