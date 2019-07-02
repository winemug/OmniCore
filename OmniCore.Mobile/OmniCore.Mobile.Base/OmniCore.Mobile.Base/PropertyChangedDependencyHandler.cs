using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Diagnostics;

namespace OmniCore.Mobile.Base
{
    public class PropertyChangedDependencyHandler : IDisposable
    {
        private DependencyNode Root;

        public PropertyChangedDependencyHandler(PropertyChangedImpl instance)
        {
            InitializeDependencies(instance);
        }

        private void InitializeDependencies(PropertyChangedImpl targetInstance)
        {
            var allProperties = targetInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

            Root = new DependencyNode(targetInstance);

            foreach (var targetProperty in allProperties)
            {
                var attributes = (DependencyPathAttribute[])targetProperty.GetCustomAttributes(typeof(DependencyPathAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    foreach (var attribute in attributes)
                    {
                        var currentNode = Root;
                        foreach (var pathPropertyName in attribute.PropertiesInPath)
                            currentNode = currentNode.AddChild(pathPropertyName);
                        currentNode.AddTargetPropertyName(targetProperty.Name, targetInstance);
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    Root.DisposeChildren();
                    Root.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PropertyChangedDependencyHandler()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
    class DependencyNode : IDisposable
    {
        string PropertyName;
        DependencyNode Parent;
        List<DependencyNode> Children;

        private INotifyPropertyChanged privateInstance;
        INotifyPropertyChanged Instance
        {
            get
            {
                return privateInstance;
            }
            set
            {
                if (value != privateInstance)
                {
                    if (privateInstance != null)
                        privateInstance.PropertyChanged -= Instance_PropertyChanged;
                    privateInstance = value;
                    if (privateInstance != null)
                        privateInstance.PropertyChanged += Instance_PropertyChanged;
                }
            }
        }
        PropertyChangedImpl TargetInstance;
        HashSet<string> TargetPropertyNames;
        PropertyInfo PInfo;

        private DependencyNode() { }

        public DependencyNode(INotifyPropertyChanged rootInstance)
        {
            Instance = rootInstance;
        }

        public void DisposeChildren()
        {
            if (Children != null)
            {
                foreach(var child in Children)
                {
                    child.DisposeChildren();
                    child.Dispose();
                }
            }
        }

        public DependencyNode AddChild(string propertyName)
        {
            DependencyNode child = null;

            if (Children != null)
            {
                child = Children.Where(c => c.PropertyName == propertyName).FirstOrDefault();
            }
            else
            {
                Children = new List<DependencyNode>();
                if (Parent != null)
                    EvaluateInstance();
            }

            if (child == null)
            {
                Type instanceType = this.Instance?.GetType() ?? this.PInfo.PropertyType;
                child = new DependencyNode()
                {
                    Parent = this,
                    PropertyName = propertyName,
                    PInfo = instanceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance),
                };
                Children.Add(child);
            }
            return child;
        }

        private void EvaluateInstance()
        {
            if (Children != null)
            {
                if (Parent.Instance == null)
                    Instance = null;
                else
                    Instance = PInfo.GetValue(Parent.Instance) as INotifyPropertyChanged;

                foreach (var child in Children)
                    child.EvaluateInstance();
            }

            RaiseTargets(null);
        }

        private void Instance_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"PPPPPPPPPPPPPPPPPPPPPPPPPPP: {sender} {e.PropertyName}");
            if (Children != null)
            {
                foreach (var child in Children.Where(c => c.PInfo.Name == e.PropertyName))
                    child.EvaluateInstance();
            }

            RaiseTargets(e.PropertyName);
        }

        private void RaiseTargets(string propertyName)
        {
            if (TargetPropertyNames != null && PInfo != null && (propertyName == null || PInfo.Name == propertyName))
            {
                foreach (var targetPropertyName in TargetPropertyNames)
                {
                    Debug.WriteLine($"PPPPPPPPPPPPPPPPPPPPPPPPPPP Raising: {targetPropertyName}");
                    TargetInstance.OnPropertyChanged(this, new PropertyChangedEventArgs(targetPropertyName));
                }
            }
        }

        public void AddTargetPropertyName(string propertyName, PropertyChangedImpl targetInstance)
        {
            if (TargetPropertyNames == null)
            {
                TargetPropertyNames = new HashSet<string>();
            }
            TargetPropertyNames.Add(propertyName);
            TargetInstance = targetInstance;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (Instance != null)
                        Instance.PropertyChanged -= Instance_PropertyChanged;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~DependencyNode()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }
}
