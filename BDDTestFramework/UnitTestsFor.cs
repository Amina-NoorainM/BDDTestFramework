using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDDTestFramework
{
    public abstract class UnitTestsFor<TObjectUnderTest>
        where TObjectUnderTest : class
    {
        protected TObjectUnderTest ObjectUnderTest;
        private MockRepository _mockRepository;

        protected UnitTestsFor()
        {
            _mockRepository = new MockRepository(MockBehavior.Loose);
            MockContainer = new AutoMockContainer(_mockRepository);
        }

        /// <summary>
        /// Changes mocks behavior to <see cref="MockBehavior.Strict"/>.
        /// </summary>
        protected void UseStrict()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            MockContainer = new AutoMockContainer(_mockRepository);
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="MockRepository.VerifyAll"/> will be used or <see cref="MockRepository.Verify"/>.
        /// </summary>
        /// <value><c>true</c> to use <see cref="MockRepository.VerifyAll"/>; otherwise, <c>false</c>.</value>
        /// <remarks><c>false</c> be default.</remarks>
        public bool VerifyAll { get; set; }

        /// <summary>
        /// Forces factory verification.
        /// </summary>
        /// <remarks>This is called automatically by xUnit.</remarks>
        public virtual void Dispose()
        {
            if (VerifyAll)
                _mockRepository.VerifyAll();
            else
                _mockRepository.Verify();
        }

        /// <summary>
        /// Provides access to the auto mocking container
        /// </summary>
        public AutoMockContainer MockContainer
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes the <see cref="ObjectUnderTest"/> field using an auto-mocking container.
        /// This method must be called in the test's TestInitialize method.
        /// </summary>
        public virtual void InitObjectUnderTest()
        {
            ObjectUnderTest = MockContainer.Create<TObjectUnderTest>();
        }

        /// <summary>
        /// Convenience method for retrieving a Mock that was generated for the <see cref="ObjectUnderTest"/>.
        /// </summary>
        /// <typeparam name="T">The type for which to retrieve the <see cref="Mock{T}"/>.</typeparam>
        /// <returns>A mock object for the given type.</returns>
        protected Mock<T> For<T>() where T : class
        {
            return MockContainer.GetMock<T>();
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        /// <typeparam name="T">A type to create.</typeparam>
        /// <returns>
        /// Object of the type <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>Usually used to create objects to test. Any non-existing dependencies
        /// are mocked.
        /// <para>Container is used to resolve build dependencies.</para></remarks>
        public T Create<T>() where T : class
        {
            return MockContainer.Create<T>();
        }

        /// <summary>
        /// Creates an object of the specified type.
        /// </summary>
        /// <typeparam name="T">A type to create.</typeparam>
        /// <param name="activator">The activator used to create object.</param>
        /// <returns>
        /// Object of the type <typeparamref name="T"/>.
        /// </returns>
        /// <remarks>Usually used to create objects to test. Any non-existing dependencies
        /// are mocked.
        /// <para>Container is used to resolve build dependencies.</para></remarks>
        /// <example>Using activator</example>
        /// <code>this.Create( c => new Apple( c.Resolve&lt; ITree> ()) )</code>
        /// <seealso href="http://code.google.com/p/autofac/wiki/ResolveParameters"/>
        public T Create<T>(Func<IResolve, T> activator) where T : class
        {
            return MockContainer.Create(activator);
        }

        /// <summary>
        /// Resolves an object from the container.
        /// </summary>
        /// <typeparam name="T">Type to resolve for.</typeparam>
        /// <returns>Resolved object.</returns>
        public T Resolve<T>() where T : class
        {
            return MockContainer.Resolve<T>();
        }

        /// <summary>
        /// Captures which class to use to provide specified service.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <typeparam name="TImplementation">The type of the implementation.</typeparam>
        public void Register<TService, TImplementation>()
        {
            MockContainer.Register<TService, TImplementation>();
        }

        /// <summary>
        /// Captures <paramref name="instance"/> as the object to provide <typeparamref name="TService"/> for mocking.
        /// </summary>
        /// <typeparam name="TService">The type of the service.</typeparam>
        /// <param name="instance">The instance.</param>
        public void Register<TService>(TService instance)
        {
            MockContainer.Register(instance);
        }

        /// <summary>
        /// Registers the given service creation delegate on the container.
        /// </summary>
        /// <typeparam name="TService">Service type.</typeparam>
        public void Register<TService>(Func<IResolve, TService> activator)
        {
            MockContainer.Register(activator);
        }
    }
}
