﻿using NUnit.Framework;
using strange.extensions.context.impl;
using strange.extensions.injector.impl;

namespace strange.unittests
{
    /**
     * Some functionality for Cross ViewedContext will be impossible to test
     * The goal here is to test whatever we can, but the actual implementation will 
     * vary per users and rely heavily on Unity in most cases, which we can't test here.
     **/
    [TestFixture]
    public class TestCrossContext
    {
        [SetUp]
        public void SetUp()
        {
            Context.firstContext = null;
            view = new object();
            Parent = new CrossContext( true);
            ChildOne = new CrossContext(
                true); //Ctr will automatically add to ViewedContext.firstcontext. No need to call it manually (and you should not).
            ChildTwo = new CrossContext( true);
        }

        private object view;
        private CrossContext Parent;
        private CrossContext ChildOne;
        private CrossContext ChildTwo;

        [Test]
        public void TestCorrectInjector() //Issue #189
        {
            Parent.injectionBinder.Bind<IVehicle>().To<Car>().CrossContext();

            ChildOne.injectionBinder.Bind<IEngine>().To<GasEngine>();
            ChildTwo.injectionBinder.Bind<IEngine>().To<ElectricEngine>();

            var carOne = ChildOne.injectionBinder.GetInstance<IVehicle>();
            var carTwo = ChildTwo.injectionBinder.GetInstance<IVehicle>();

            Assert.NotNull(carOne);
            Assert.NotNull(carTwo);

            Assert.IsTrue(carOne.Engine is GasEngine);
            Assert.IsTrue(carTwo.Engine is ElectricEngine);
        }

        [Test]
        public void TestFactory()
        {
            var parentModel = new TestModel();
            Parent.injectionBinder.Bind<TestModel>().To<TestModel>().CrossContext();

            var parentModelTwo = Parent.injectionBinder.GetInstance<TestModel>();

            Assert.AreNotSame(parentModel, parentModelTwo); //As it's a factory, we should not have the same objects

            var childOneModel = ChildOne.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(childOneModel);
            var childTwoModel = ChildTwo.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(childTwoModel);
            Assert.AreNotSame(childOneModel, childTwoModel); //These two should be DIFFERENT

            Assert.AreEqual(0, parentModel.Value);

            parentModel.Value++;
            Assert.AreEqual(0, childOneModel.Value); //doesn't change

            parentModel.Value++;
            Assert.AreEqual(0, childTwoModel.Value); //doesn't change
        }

        //test that local bindings will override cross bindings
        [Test]
        public void TestLocalOverridesCrossContext()
        {
            Parent.injectionBinder.Bind<TestModel>().ToSingleton().CrossContext(); //bind the cross context binding.
            var initialChildOneModel = new TestModel();
            initialChildOneModel.Value = 1000;


            ChildOne.injectionBinder.Bind<TestModel>()
                .ToValue(initialChildOneModel); //Bind a local override in this child

            var parentModel =
                Parent.injectionBinder
                    .GetInstance<TestModel>(); //Get the instance from the parent injector (The cross context binding)


            var childOneModel = ChildOne.injectionBinder.GetInstance<TestModel>();
            Assert.AreSame(initialChildOneModel,
                childOneModel); // The value from getInstance is the same as the value we just mapped as a value locally
            Assert.AreNotSame(childOneModel,
                parentModel); //The value from getinstance is NOT the same as the cross context value. We have overidden the cross context value locally


            var childTwoModel = ChildTwo.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(childTwoModel);
            Assert.AreNotSame(childOneModel,
                childTwoModel); //These two are different objects, the childTwoModel being cross context, and childone being the override
            Assert.AreSame(parentModel, childTwoModel); //Both cross context models are the same


            parentModel.Value++;
            Assert.AreEqual(1, childTwoModel.Value); //cross context model should be changed

            parentModel.Value++;
            Assert.AreEqual(1000, childOneModel.Value); //local model is not changed


            Assert.AreEqual(2, parentModel.Value); //cross context model is changed
        }

        [Test]
        public void TestNamed()
        {
            var name = "Name";
            var parentModel = new TestModel();
            Parent.injectionBinder.Bind<TestModel>().ToValue(parentModel).ToName(name)
                .CrossContext(); //bind it once here and it should be accessible everywhere

            var parentModelTwo = Parent.injectionBinder.GetInstance<TestModel>(name);

            Assert.AreSame(parentModel, parentModelTwo); //Assure that this value is correct

            var childOneModel = ChildOne.injectionBinder.GetInstance<TestModel>(name);
            Assert.IsNotNull(childOneModel);
            var childTwoModel = ChildTwo.injectionBinder.GetInstance<TestModel>(name);
            Assert.IsNotNull(childTwoModel);
            Assert.AreSame(childOneModel, childTwoModel); //These two should be the same object

            Assert.AreEqual(0, parentModel.Value);

            parentModel.Value++;
            Assert.AreEqual(1, childOneModel.Value);

            parentModel.Value++;
            Assert.AreEqual(2, childTwoModel.Value);
        }

        [Test]
        public void TestSimpleUnbind()
        {
            Parent.injectionBinder.Bind<TestModel>().ToSingleton().CrossContext();
            Assert.IsNotNull(Parent.injectionBinder.GetInstance<TestModel>());

            Parent.injectionBinder.Unbind<TestModel>();
            Assert.IsNull(Parent.injectionBinder.GetBinding<TestModel>());

            Assert.That(() => Parent.injectionBinder.GetInstance<TestModel>(),
                Throws.Exception
                    .TypeOf<InjectionException>());
        }

        [Test]
        public void TestSingleton()
        {
            Parent.injectionBinder.Bind<TestModel>().ToSingleton()
                .CrossContext(); //bind it once here and it should be accessible everywhere

            var parentModel = Parent.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(parentModel);

            var childOneModel = ChildOne.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(childOneModel);
            var childTwoModel = ChildTwo.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(childTwoModel);

            Assert.AreSame(parentModel, childOneModel);
            Assert.AreSame(parentModel, childTwoModel);
            Assert.AreSame(childOneModel, childTwoModel);

            var binding = Parent.injectionBinder.GetBinding<TestModel>();
            Assert.IsNotNull(binding);
            Assert.IsTrue(binding.isCrossContext);

            var childBinding = ChildOne.injectionBinder.GetBinding<TestModel>();
            Assert.IsNotNull(childBinding);
            Assert.IsTrue(childBinding.isCrossContext);


            Assert.AreEqual(0, parentModel.Value);

            parentModel.Value++;
            Assert.AreEqual(1, childOneModel.Value);

            parentModel.Value++;
            Assert.AreEqual(2, childTwoModel.Value);
        }

        [Test]
        public void TestSingletonUnbind()
        {
            Parent.injectionBinder.Bind<TestModel>().ToSingleton()
                .CrossContext(); //bind it once here and it should be accessible everywhere

            var parentModel = Parent.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(parentModel);

            var childOneModel = ChildOne.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(childOneModel);
            var childTwoModel = ChildTwo.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(childTwoModel);

            //Lots of lines that tell us they're all the same binding

            Assert.AreSame(parentModel, childOneModel);
            Assert.AreSame(parentModel, childTwoModel);
            Assert.AreSame(childOneModel, childTwoModel);

            var binding = Parent.injectionBinder.GetBinding<TestModel>();
            Assert.IsNotNull(binding);
            Assert.IsTrue(binding.isCrossContext);

            var childBinding = ChildOne.injectionBinder.GetBinding<TestModel>();
            Assert.IsNotNull(childBinding);
            Assert.IsTrue(childBinding.isCrossContext);


            Assert.AreEqual(0, parentModel.Value);

            parentModel.Value++;
            Assert.AreEqual(1, childOneModel.Value);

            parentModel.Value++;
            Assert.AreEqual(2, childTwoModel.Value);


            //unbinding the parent should unbind the children who do not have a specific local binding

            Parent.injectionBinder.Unbind<TestModel>();

            binding = Parent.injectionBinder.GetBinding<TestModel>();
            Assert.IsNull(binding);
            childBinding = ChildOne.injectionBinder.GetBinding<TestModel>();
            Assert.IsNull(childBinding);
            childBinding = ChildTwo.injectionBinder.GetBinding<TestModel>();
            Assert.IsNull(childBinding);
        }


        [Test]
        public void TestValue()
        {
            var parentModel = new TestModel();
            Parent.injectionBinder.Bind<TestModel>().ToValue(parentModel)
                .CrossContext(); //bind it once here and it should be accessible everywhere

            var parentModelTwo = Parent.injectionBinder.GetInstance<TestModel>();

            Assert.AreSame(parentModel, parentModelTwo); //Assure that this value is correct

            var childOneModel = ChildOne.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(childOneModel);
            var childTwoModel = ChildTwo.injectionBinder.GetInstance<TestModel>();
            Assert.IsNotNull(childTwoModel);
            Assert.AreSame(childOneModel, childTwoModel); //These two should be the same object

            Assert.AreEqual(0, parentModel.Value);

            parentModel.Value++;
            Assert.AreEqual(1, childOneModel.Value);

            parentModel.Value++;
            Assert.AreEqual(2, childTwoModel.Value);
        }
    }


    public class TestModel
    {
        public int Value;
    }

    #region - Bug specific test classes ISSUE 189

    internal interface IVehicle
    {
        IEngine Engine { get; }
    }

    internal interface IEngine
    {
        string Name { get; }
    }

    internal class Car : IVehicle
    {
        [Inject] public IEngine Engine { get; set; }
    }

    internal class GasEngine : IEngine
    {
        public string Name => "GAS";
    }

    internal class ElectricEngine : IEngine
    {
        public string Name => "ELECTRIC";
    }

    #endregion
}