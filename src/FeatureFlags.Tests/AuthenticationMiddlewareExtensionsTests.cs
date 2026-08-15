using System;
using System.Collections.Generic;
using FeatureFlags.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace FeatureFlags.Tests
{
    public class AuthenticationMiddlewareExtensionsTests
    {
        private readonly Mock<IApplicationBuilder> _appMock;

        public AuthenticationMiddlewareExtensionsTests()
        {
            _appMock = new Mock<IApplicationBuilder>();
            
            // Mock IApplicationBuilder.Use method, which UseMiddleware calls internally
            _appMock.Setup(app => app.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()))
                    .Returns(_appMock.Object);
        }

        [Fact]
        public void UseAuthenticationMiddleware_Default_CallsUse()
        {
            _appMock.Object.UseAuthenticationMiddleware();

            _appMock.Verify(app => app.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
        }

        [Fact]
        public void UseAuthenticationMiddleware_ConfigureOptions_CallsUse()
        {
            _appMock.Object.UseAuthenticationMiddleware(options => { });

            _appMock.Verify(app => app.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
        }

        [Fact]
        public void UseAuthenticationMiddleware_ValidApiKeys_CallsUse()
        {
            _appMock.Object.UseAuthenticationMiddleware(new List<string> { "key1" });

            _appMock.Verify(app => app.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
        }

        [Fact]
        public void UseAuthenticationMiddleware_RequireApiKey_CallsUse()
        {
            _appMock.Object.UseAuthenticationMiddleware(true);

            _appMock.Verify(app => app.Use(It.IsAny<Func<RequestDelegate, RequestDelegate>>()), Times.Once);
        }

        [Fact]
        public void UseAuthenticationMiddleware_NullApp_ThrowsException()
        {
            IApplicationBuilder app = null!;
            Assert.Throws<NullReferenceException>(() => app.UseAuthenticationMiddleware());
        }

        [Fact]
        public void UseAuthenticationMiddleware_Bool_NullApp_ThrowsArgumentNullException()
        {
            IApplicationBuilder app = null!;
            Assert.Throws<ArgumentNullException>(() => app.UseAuthenticationMiddleware(true));
        }

        [Fact]
        public void UseAuthenticationMiddleware_ConfigureOptions_NullConfigureOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _appMock.Object.UseAuthenticationMiddleware((Action<AuthenticationOptions>)null!));
        }

        [Fact]
        public void UseAuthenticationMiddleware_ValidApiKeys_NullApp_ThrowsArgumentNullException()
        {
            IApplicationBuilder app = null!;
            Assert.Throws<ArgumentNullException>(() => app.UseAuthenticationMiddleware(new List<string>()));
        }

        [Fact]
        public void UseAuthenticationMiddleware_ValidApiKeys_NullValidApiKeys_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => _appMock.Object.UseAuthenticationMiddleware((IEnumerable<string>)null!));
        }
    }
}
