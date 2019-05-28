using System;
using System.Collections.Generic;
using NUnit.Framework;
using strange.extensions.signal.api;
using strange.extensions.signal.impl;

namespace strange.unittests
{
    [TestFixture]
    public class TestSignal
    {
        [SetUp]
        public void SetUp()
        {
            testValue = 0;
        }

        private int testValue;
        private int testInt = 1;
        private int testIntTwo = 2;
        private int testIntThree = 3;
        private int testIntFour = 4;
        private int intToIncrement;
        private const int baseSignalValue = 1000;

        private IList<Type> GetThreeExpectedTypes()
        {
            var expected = new List<Type>();
            expected.Add(typeof(int));
            expected.Add(typeof(string));
            expected.Add(typeof(float));
            return expected;
        }

        private void NoArgSignalCallback()
        {
            testValue++;
        }

        private void NoArgSignalCallbackTwo()
        {
            testValue += 10;
        }

        private void OneArgSignalCallback(int argInt)
        {
            testValue += argInt;
        }

        private void OneArgSignalCallbackTwo(int argInt)
        {
            testValue++;
        }

        private void TwoArgSignalCallback(int one, int two)
        {
            testValue += one;
            testValue += two;
        }

        private void TwoArgSignalCallbackTwo(int one, int two)
        {
            testValue *= one;
            testValue *= two;
        }

        private void ThreeArgSignalCallback(int one, int two, int three)
        {
            testValue += one;
            testValue += two;
            testValue *= three;
        }

        private void ThreeArgSignalCallbackTwo(int one, int two, int three)
        {
            testValue *= one;
            testValue *= two;
            testValue *= three;
        }

        private void FourArgSignalCallback(int one, int two, int three, int four)
        {
            testValue += one;
            testValue += two;
            testValue *= three;
            testValue -= four;
        }

        private void FourArgSignalCallbackTwo(int one, int two, int three, int four)
        {
            testValue *= one;
            testValue *= two;
            testValue *= three;
            testValue *= four;
        }

        private void SimpleSignalCallback()
        {
            intToIncrement++;
        }

        private int GetTwoIntExpected()
        {
            return testInt + testIntTwo;
        }

        private void TwoArgCallback(int arg1, int arg2)
        {
            testValue += arg1 + arg2;
        }

        private void ThreeArgCallback(int arg1, int arg2, int arg3)
        {
            testValue += arg1 + arg2 + arg3;
        }

        private int GetThreeIntExpected()
        {
            return GetTwoIntExpected() + testIntThree;
        }

        private int GetFourIntExpected()
        {
            return GetThreeIntExpected() + testIntFour;
        }

        private void CreateFourInts()
        {
            CreateThreeInts();
            testIntFour = 4;
        }

        private void CreateThreeInts()
        {
            CreateTwoInts();
            testIntThree = 3;
        }

        private void CreateTwoInts()
        {
            CreateOneInt();
            testIntTwo = 2;
        }

        private void CreateOneInt()
        {
            testInt = 1;
        }

        private void FourArgCallback(int arg1, int arg2, int arg3, int arg4)
        {
            testValue += arg1 + arg2 + arg3 + arg4;
        }

        private void BaseSignalCallback(IBaseSignal signal, object[] args)
        {
            testValue += baseSignalValue;
        }

        [Test]
        public void AddListener_SignalWithFourTypesGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal<int, int, int, int>();
            CreateFourInts();
            signal.AddListener(FourArgCallback);
            signal.AddListener(FourArgCallback);

            signal.Dispatch(testInt, testIntTwo, testIntThree, testIntFour);

            var expected = GetFourIntExpected();
            Assert.AreEqual(expected, testValue);
        }

        [Test]
        public void AddListener_SignalWithNoTypeGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal();
            intToIncrement = 0;
            signal.AddListener(SimpleSignalCallback);
            signal.AddListener(SimpleSignalCallback);

            signal.Dispatch();

            Assert.AreEqual(1, intToIncrement);
        }

        [Test]
        public void AddListener_SignalWithOneTypeGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal<int>();
            testInt = 42;
            signal.AddListener(OneArgSignalCallback);
            signal.AddListener(OneArgSignalCallback);

            signal.Dispatch(testInt);

            Assert.AreEqual(testInt, testValue);
        }

        [Test]
        public void AddListener_SignalWithThreeTypesGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal<int, int, int>();
            CreateThreeInts();
            signal.AddListener(ThreeArgCallback);
            signal.AddListener(ThreeArgCallback);

            signal.Dispatch(testInt, testIntTwo, testIntThree);

            var expected = GetThreeIntExpected();
            Assert.AreEqual(expected, testValue);
        }

        [Test]
        public void AddListener_SignalWithTwoTypesGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal<int, int>();
            CreateTwoInts();
            signal.AddListener(TwoArgCallback);
            signal.AddListener(TwoArgCallback);

            signal.Dispatch(testInt, testIntTwo);

            var expected = GetTwoIntExpected();
            Assert.AreEqual(expected, testValue);
        }

        [Test]
        public void AddOnce_SignalWithFourTypesGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal<int, int, int, int>();
            CreateFourInts();
            signal.AddOnce(FourArgCallback);
            signal.AddOnce(FourArgCallback);

            signal.Dispatch(testInt, testIntTwo, testIntThree, testIntFour);

            var expected = GetFourIntExpected();
            Assert.AreEqual(expected, testValue);
        }

        [Test]
        public void AddOnce_SignalWithNoTypeGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal();
            intToIncrement = 0;
            signal.AddOnce(SimpleSignalCallback);
            signal.AddOnce(SimpleSignalCallback);

            signal.Dispatch();

            Assert.AreEqual(1, intToIncrement);
        }

        [Test]
        public void AddOnce_SignalWithOneTypeGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal<int>();
            testInt = 42;
            signal.AddOnce(OneArgSignalCallback);
            signal.AddOnce(OneArgSignalCallback);

            signal.Dispatch(testInt);

            Assert.AreEqual(testInt, testValue);
        }

        [Test]
        public void AddOnce_SignalWithThreeTypesGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal<int, int, int>();
            CreateThreeInts();
            signal.AddOnce(ThreeArgCallback);
            signal.AddOnce(ThreeArgCallback);

            signal.Dispatch(testInt, testIntTwo, testIntThree);

            var expected = GetThreeIntExpected();
            Assert.AreEqual(expected, testValue);
        }

        [Test]
        public void AddOnce_SignalWithTwoTypesGivenSameCallbackMultipleTimes_ExpectsDelegateCalledOnlyOnce()
        {
            var signal = new Signal<int, int>();
            CreateTwoInts();
            signal.AddOnce(TwoArgCallback);
            signal.AddOnce(TwoArgCallback);

            signal.Dispatch(testInt, testIntTwo);

            var expected = GetTwoIntExpected();
            Assert.AreEqual(expected, testValue);
        }

        [Test]
        public void GetTypes_FourType_ExpectsTypesReturnedInList()
        {
            var signal = new Signal<int, string, float, Signal>();

            var actual = signal.GetTypes();

            var expected = GetThreeExpectedTypes();
            expected.Add(typeof(Signal));
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GetTypes_ThreeType_ExpectsTypesReturnedInList()
        {
            var signal = new Signal<int, string, float>();

            var actual = signal.GetTypes();

            var expected = GetThreeExpectedTypes();
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void RemoveListener_NoType_ExpectsListenerRemoved()
        {
            var signal = new Signal();

            signal.AddListener(NoArgSignalCallback);

            signal.Dispatch();
            Assert.AreEqual(1, testValue);

            signal.RemoveListener(NoArgSignalCallback);
            signal.Dispatch();
            Assert.AreEqual(1, testValue);
        }

        [Test]
        public void TestBaseOnce()
        {
            var signal = new BaseSignal();
            signal.AddOnce(BaseSignalCallback);

            signal.Dispatch(new object[] { });
            Assert.AreEqual(testValue, baseSignalValue);

            signal.Dispatch(new object[] { });
            Assert.AreEqual(testValue, baseSignalValue);
        }

        [Test]
        public void TestFourArgSignal()
        {
            var signal = new Signal<int, int, int, int>();

            var intendedResult = (testInt + testIntTwo) * testIntThree - testIntFour;
            signal.AddListener(FourArgSignalCallback);
            signal.Dispatch(testInt, testIntTwo, testIntThree, testIntFour);
            Assert.AreEqual(intendedResult, testValue);

            signal.RemoveListener(FourArgSignalCallback);
            signal.Dispatch(testInt, testIntTwo, testIntThree, testIntFour);
            Assert.AreEqual(intendedResult, testValue); //no-op due to remove

            intendedResult += testInt;
            intendedResult += testIntTwo;
            intendedResult *= testIntThree;
            intendedResult -= testIntFour;

            signal.AddOnce(FourArgSignalCallback);
            signal.Dispatch(testInt, testIntTwo, testIntThree, testIntFour);
            Assert.AreEqual(intendedResult, testValue);

            signal.Dispatch(testInt, testIntTwo, testIntThree, testIntFour);
            Assert.AreEqual(intendedResult, testValue); //Add once should result in no-op
        }

        [Test]
        public void TestMultipleCallbacks()
        {
            var signal = new Signal<int>();

            signal.AddListener(OneArgSignalCallback);
            signal.AddListener(OneArgSignalCallbackTwo);

            signal.Dispatch(testInt);

            Assert.AreEqual(testInt + 1, testValue);
        }

        [Test]
        public void TestNoArgSignal()
        {
            var signal = new Signal();

            signal.AddListener(NoArgSignalCallback);
            signal.Dispatch();
            Assert.AreEqual(1, testValue);
        }

        [Test]
        public void TestOnce()
        {
            var signal = new Signal<int>();

            signal.AddOnce(OneArgSignalCallback);
            signal.Dispatch(testInt);
            Assert.AreEqual(testInt, testValue);

            signal.Dispatch(testInt);
            Assert.AreEqual(testInt, testValue); //should not fire a second time
        }

        [Test]
        public void TestOneArgSignal()
        {
            var signal = new Signal<int>();

            signal.AddListener(OneArgSignalCallback);
            signal.Dispatch(testInt);
            Assert.AreEqual(testInt, testValue);
        }

        [Test]
        public void TestRemoveAllListeners()
        {
            var signal = new Signal();

            signal.AddListener(NoArgSignalCallback);
            signal.AddListener(NoArgSignalCallbackTwo);

            signal.RemoveAllListeners();
            signal.Dispatch();

            Assert.AreEqual(0, testValue);
        }

        [Test]
        public void TestRemoveAllListenersFour()
        {
            var signal = new Signal<int, int, int, int>();

            signal.AddListener(FourArgSignalCallback);
            signal.AddListener(FourArgSignalCallbackTwo);

            signal.RemoveAllListeners();
            signal.Dispatch(testInt, testIntTwo, testIntThree, testIntFour);

            Assert.AreEqual(0, testValue);
        }

        [Test]
        public void TestRemoveAllListenersOne()
        {
            var signal = new Signal<int>();

            signal.AddListener(OneArgSignalCallback);
            signal.AddListener(OneArgSignalCallbackTwo);

            signal.RemoveAllListeners();
            signal.Dispatch(testInt);

            Assert.AreEqual(0, testValue);
        }

        [Test]
        public void TestRemoveAllListenersThree()
        {
            var signal = new Signal<int, int, int>();

            signal.AddListener(ThreeArgSignalCallback);
            signal.AddListener(ThreeArgSignalCallbackTwo);

            signal.RemoveAllListeners();
            signal.Dispatch(testInt, testIntTwo, testIntThree);

            Assert.AreEqual(0, testValue);
        }

        [Test]
        public void TestRemoveAllListenersTwo()
        {
            var signal = new Signal<int, int>();

            signal.AddListener(TwoArgSignalCallback);
            signal.AddListener(TwoArgSignalCallbackTwo);

            signal.RemoveAllListeners();
            signal.Dispatch(testInt, testIntTwo);

            Assert.AreEqual(0, testValue);
        }

        [Test]
        public void TestRemoveAllRemovesOnce()
        {
            var signal = new Signal();
            signal.AddOnce(NoArgSignalCallback);
            signal.AddOnce(NoArgSignalCallbackTwo);

            signal.RemoveAllListeners();
            signal.Dispatch();

            Assert.AreEqual(0, testValue);
        }

        [Test]
        public void TestRemoveListener()
        {
            var signal = new Signal<int>();

            signal.AddListener(OneArgSignalCallback);

            signal.Dispatch(testInt);
            Assert.AreEqual(testInt, testValue);

            signal.RemoveListener(OneArgSignalCallback);
            signal.Dispatch(testInt);
            Assert.AreEqual(testInt, testValue);
        }

        [Test]
        public void TestRemoveListenerDoesntBlowUp()
        {
            var signal = new Signal();
            signal.RemoveListener(NoArgSignalCallback);
        }

        [Test]
        public void TestRemoveListenerDoesntBlowUpFour()
        {
            var signal = new Signal<int, int, int, int>();
            signal.RemoveListener(FourArgSignalCallback);
        }

        [Test]
        public void TestRemoveListenerDoesntBlowUpOne()
        {
            var signal = new Signal<int>();
            signal.RemoveListener(OneArgSignalCallback);
        }

        [Test]
        public void TestRemoveListenerDoesntBlowUpThree()
        {
            var signal = new Signal<int, int, int>();
            signal.RemoveListener(ThreeArgSignalCallback);
        }

        [Test]
        public void TestRemoveListenerDoesntBlowUpTwo()
        {
            var signal = new Signal<int, int>();
            signal.RemoveListener(TwoArgSignalCallback);
        }

        [Test]
        public void TestThreeArgSignal()
        {
            var signal = new Signal<int, int, int>();

            var intendedResult = (testInt + testIntTwo) * testIntThree;
            signal.AddListener(ThreeArgSignalCallback);
            signal.Dispatch(testInt, testIntTwo, testIntThree);
            Assert.AreEqual(intendedResult, testValue);

            signal.RemoveListener(ThreeArgSignalCallback);
            signal.Dispatch(testInt, testIntTwo, testIntThree);
            Assert.AreEqual(intendedResult, testValue); //no-op due to remove

            intendedResult += testInt;
            intendedResult += testIntTwo;
            intendedResult *= testIntThree;

            signal.AddOnce(ThreeArgSignalCallback);
            signal.Dispatch(testInt, testIntTwo, testIntThree);
            Assert.AreEqual(intendedResult, testValue);

            signal.Dispatch(testInt, testIntTwo, testIntThree);
            Assert.AreEqual(intendedResult, testValue); //Add once should result in no-op
        }

        //These are testing base functions, but also that ordering is correct using mathematical operators
        [Test]
        public void TestTwoArgSignal()
        {
            var signal = new Signal<int, int>();

            signal.AddListener(TwoArgSignalCallback);
            signal.Dispatch(testInt, testIntTwo);
            Assert.AreEqual(testInt + testIntTwo, testValue);

            signal.RemoveListener(TwoArgSignalCallback);
            signal.Dispatch(testInt, testIntTwo);
            Assert.AreEqual(testInt + testIntTwo, testValue); //Removed listener should have no-op

            signal.AddOnce(TwoArgSignalCallback);
            signal.Dispatch(testInt, testIntTwo);
            Assert.AreEqual((testInt + testIntTwo) * 2, testValue);

            signal.Dispatch(testInt, testIntTwo);
            Assert.AreEqual((testInt + testIntTwo) * 2, testValue); //addonce should result in no-op
        }
    }
}