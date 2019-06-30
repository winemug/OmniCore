using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Linq;
using System.Reflection;

namespace OmniCore.Mobile.Base
{
    public class PropertyChangedDependencyHandler : IDisposable
    {
        private Dictionary<INotifyPropertyChanged, Dictionary<string,IList<Tuple<string, PropertyChangedEventArgs>>>> NotifyLookup;
        private Dictionary<string, INotifyPropertyChanged> PathLookup;
        private Dictionary<PropertyInfo, DependencyPathAttribute[]> DependencyAttributes;

        private PropertyChangedImpl TargetInstance;
        public PropertyChangedDependencyHandler(PropertyChangedImpl instance)
        {
            TargetInstance = instance;
            InitializeDependencies();
            CreateDependencies();
        }

        private void InitializeDependencies()
        {
            DependencyAttributes = new Dictionary<PropertyInfo, DependencyPathAttribute[]>();
            var allProperties = TargetInstance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in allProperties)
            {
                var attributes = (DependencyPathAttribute[])property.GetCustomAttributes(typeof(DependencyPathAttribute), true);
                if (attributes != null && attributes.Length > 0)
                {
                    DependencyAttributes.Add(property, attributes);
                    foreach(var attribute in attributes)
                    {
                        var sourceType = TargetInstance.GetType();
                        for(int i=0; i< attribute.PropertiesInPath.Length; i++)
                        {
                            var pi = sourceType.GetProperty(attribute.PropertiesInPath[i], BindingFlags.Public | BindingFlags.Instance);
                            attribute.PropertyInfos[i] = pi;
                            sourceType = pi.PropertyType;
                        }
                    }
                }
            }
        }

        private void CreateDependencies()
        {
            NotifyLookup = new Dictionary<INotifyPropertyChanged, Dictionary<string, IList<Tuple<string, PropertyChangedEventArgs>>>>();
            PathLookup = new Dictionary<string, INotifyPropertyChanged>();

            foreach (var targetProperty in DependencyAttributes.Keys)
            {
                foreach(var attrib in DependencyAttributes[targetProperty])
                {
                    UpdateDependencyChain(TargetInstance, attrib.PropertyInfos, targetProperty.Name);
                }
            }
        }

        private void UpdateDependencyChain(INotifyPropertyChanged instance, PropertyInfo[] sourcePropertyInfos, string targetPropertyName)
        {
            string sourcePath = "";
            foreach (var sourcePropertyInfo in sourcePropertyInfos)
            {
                sourcePath += "." + sourcePropertyInfo.Name;
                if (instance != null)
                {
                    CreateUpdateDependency(instance, sourcePropertyInfo.Name, sourcePath, targetPropertyName);
                    instance = sourcePropertyInfo.GetValue(instance) as INotifyPropertyChanged;
                }
                else
                {
                    break;
                }
            }
        }

        public void CreateUpdateDependency(INotifyPropertyChanged source, string sourcePropertyName, string sourcePath, string targetPropertyName)
        {
            if (PathLookup.ContainsKey(sourcePath))
            {
                var oldSource = PathLookup[sourcePath];
                if (oldSource != source)
                {
                    oldSource.PropertyChanged -= PropertyChangedHandler;
                    NotifyLookup.Remove(oldSource);
                }
            }
            else
            {
                PathLookup.Add(sourcePath, source);
                source.PropertyChanged += PropertyChangedHandler;
            }

            if (!NotifyLookup.ContainsKey(source))
            {
                NotifyLookup.Add(source, new Dictionary<string, IList<Tuple<string, PropertyChangedEventArgs>>>());
            }
            if (!NotifyLookup[source].ContainsKey(sourcePropertyName))
            {
                NotifyLookup[source].Add(sourcePropertyName, new List<Tuple<string, PropertyChangedEventArgs>>());
            }

            NotifyLookup[source][sourcePropertyName].Add(new Tuple<string, PropertyChangedEventArgs>(sourcePath, new PropertyChangedEventArgs(targetPropertyName)));
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            var sourceInstance = (INotifyPropertyChanged)sender;
            var source = NotifyLookup[sourceInstance];
            if (string.IsNullOrEmpty(e.PropertyName) || source.ContainsKey(e.PropertyName))
            {
                var list = source[e.PropertyName].ToList();
                foreach (var t in list)
                {
                    TargetInstance.OnPropertyChanged(TargetInstance, t.Item2);
                }

                foreach (var t in list)
                {
                    CreateUpdateDependency(sourceInstance, e.PropertyName, t.Item1, t.Item2.PropertyName);
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
                    foreach(var handler in NotifyLookup.Keys)
                    {
                        handler.PropertyChanged -= PropertyChangedHandler;
                    }
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
}
