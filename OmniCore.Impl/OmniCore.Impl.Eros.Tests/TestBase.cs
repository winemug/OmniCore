using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace OmniCore.Impl.Eros.Tests
{
    public class TestBase
    {

        protected IUnityContainer Container;

        [TestInitialize]
        public void TestInitialize()
        {
            Container = new UnityContainer();
            ErosTestEnvironment.SetupRepositories(Container);
            ErosTestEnvironment.SetupRadioProvider(Container);
            OmniCore.Impl.Eros.Initializer.RegisterTypes(Container);
        }
    }
}
