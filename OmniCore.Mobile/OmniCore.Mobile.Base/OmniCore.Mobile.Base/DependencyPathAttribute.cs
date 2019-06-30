using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OmniCore.Mobile.Base
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
    public class DependencyPathAttribute : Attribute
    {
        public string[] PropertiesInPath { get; private set; }
        public PropertyInfo[] PropertyInfos { get; set; }

        public DependencyPathAttribute(params string[] propertiesInPath)
        {
            PropertiesInPath = propertiesInPath;
            PropertyInfos = new PropertyInfo[propertiesInPath.Length];
        }
    }
}
