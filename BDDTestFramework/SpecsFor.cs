using Moq;
using System;
using System.Collections.Generic;

namespace BDDTestFramework
{
    public static class Specs
    {
        public static IWhen<TObjectUnderTest> For<TObjectUnderTest>() where TObjectUnderTest : class
        {
            return new SpecsCore<TObjectUnderTest>();
        }

        private class SpecsCore<TObjectUnderTest> : UnitTestsFor<TObjectUnderTest>, ISpecsCore<TObjectUnderTest> where TObjectUnderTest : class
        {
            private class ThenAction<TSut> where TSut : class
            {
                public ThenAction(Action<VerifyingContext<TSut>> action, string testName)
                {
                    if (action == null)
                        throw new ArgumentNullException("action");

                    if (string.IsNullOrEmpty(testName))
                        throw new ArgumentNullException("testName");

                    TestName = testName;
                    Action = action;
                }
                public string TestName { get; private set; }
                public Action<VerifyingContext<TSut>> Action { get; private set; }
            }

            readonly List<ThenAction<TObjectUnderTest>> _thenActions = new List<ThenAction<TObjectUnderTest>>();
            readonly List<string> _errors = new List<string>();

            public SpecsCore()
            {
                ObjectUnderTest = MockContainer.Create<TObjectUnderTest>();
            }


            IWhen<TObjectUnderTest> IGiven<TObjectUnderTest>.Given(Action<AutoMockContainer> action)
            {
                action(MockContainer);

                return this;
            }

            IFirstThen<TObjectUnderTest> IWhen<TObjectUnderTest>.When(Action<TObjectUnderTest> action)
            {
                action(ObjectUnderTest);

                return this;
            }

            IThen<TObjectUnderTest> IFirstThen<TObjectUnderTest>.Then(string testName, Action<VerifyingContext<TObjectUnderTest>> action)
            {
                _thenActions.Add(new ThenAction<TObjectUnderTest>(action, testName));

                return this;
            }

            void IThen<TObjectUnderTest>.Assert()
            {
                foreach (var thenAction in _thenActions)
                {
                    try
                    {
                        var ctx = new VerifyingContext<TObjectUnderTest>(ObjectUnderTest, MockContainer);
                        thenAction.Action(ctx);
                    }
                    catch (Exception ex)
                    {
                        var exMessage = thenAction.TestName + ex.Message;

                        _errors.Add(exMessage);
                    }
                }

                if (_errors.Count > 0)
                {
                    var errorMessage = Environment.NewLine + Environment.NewLine +
                                       string.Join(Environment.NewLine + Environment.NewLine, _errors);

                    Microsoft.VisualStudio.TestTools.UnitTesting.Assert.Fail(errorMessage);
                }
            }
        }
    }

    public interface IGiven<TObjectUnderTest> where TObjectUnderTest : class
    {
        IWhen<TObjectUnderTest> Given(Action<AutoMockContainer> action);
    }

    public interface IWhen<TObjectUnderTest> : IGiven<TObjectUnderTest> where TObjectUnderTest : class
    {
        IFirstThen<TObjectUnderTest> When(Action<TObjectUnderTest> action);
    }

    public interface IFirstThen<TObjectUnderTest> where TObjectUnderTest : class
    {
        IThen<TObjectUnderTest> Then(string testName, Action<VerifyingContext<TObjectUnderTest>> action);
    }

    public interface IThen<TObjectUnderTest> : IFirstThen<TObjectUnderTest> where TObjectUnderTest : class
    {
        void Assert();
    }

    public interface ISpecsCore<TObjectUnderTest> : IWhen<TObjectUnderTest>, IThen<TObjectUnderTest> where TObjectUnderTest : class
    {
    }

    public static class AutoMockContainerExtensions
    {
        public static Mock<T> For<T>(this AutoMockContainer container) where T : class
        {
            return container.GetMock<T>();
        }
    }

    public class VerifyingContext<TObjectUnderTest> where TObjectUnderTest : class
    {
        private readonly TObjectUnderTest _sut;
        private readonly AutoMockContainer _mockContainer;

        public VerifyingContext(TObjectUnderTest sut, AutoMockContainer mockContainer)
        {
            if (sut == null)
                throw new ArgumentNullException("sut");

            if (mockContainer == null)
                throw new ArgumentNullException("mockContainer");

            _sut = sut;
            _mockContainer = mockContainer;
        }

        public TObjectUnderTest Sut
        {
            get { return _sut; }
        }

        public Mock<T> For<T>() where T : class
        {
            return _mockContainer.GetMock<T>();
        }
    }
}
