using FolderPrettifier;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FolderPrettifier.Tests
{
    [TestFixture]
    public class CatalogBaseUrlResolverTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Responder { get; set; }

            public List<HttpRequestMessage> Requests { get; } = new List<HttpRequestMessage>();

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Responder(request, cancellationToken);
            }
        }

        private const string RepoApiUrl = "http://test.local/repos/folder-prettifier";
        private const string RawBaseUrlTemplate = "http://test.local/raw/{0}/catalogs/";

        private string _tempDir;
        private string _repoInfoCachePath;
        private FakeHttpMessageHandler _handler;
        private RemoteFileFetcher _fetcher;
        private CatalogBaseUrlResolver _resolver;

        private void Respond(HttpStatusCode status, string content)
        {
            _handler.Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(status)
            {
                Content = new StringContent(content ?? string.Empty)
            });
        }

        private void WriteCache(string content)
        {
            File.WriteAllText(_repoInfoCachePath, content);
        }

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "fpburt-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
            _repoInfoCachePath = Path.Combine(_tempDir, "repo-info.json");
            _handler = new FakeHttpMessageHandler
            {
                Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{ \"default_branch\": \"main\" }")
                })
            };
            _fetcher = new RemoteFileFetcher(_handler);
            _resolver = new CatalogBaseUrlResolver(_fetcher, RepoApiUrl, RawBaseUrlTemplate, _repoInfoCachePath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_tempDir))
            {
                Directory.Delete(_tempDir, true);
            }
        }

        [Test]
        public async Task ResolveBaseUrlAsync_NetworkOk_ReturnsUrlWithDefaultBranch()
        {
            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.EqualTo("http://test.local/raw/main/catalogs/"));
            Assert.That(_handler.Requests.Count, Is.EqualTo(1));
            Assert.That(_handler.Requests[0].RequestUri.AbsoluteUri, Is.EqualTo(RepoApiUrl));
        }

        [Test]
        public async Task ResolveBaseUrlAsync_NetworkOk_WritesCacheFile()
        {
            await _resolver.ResolveBaseUrlAsync();

            Assert.That(File.Exists(_repoInfoCachePath), Is.True);
            Assert.That(File.ReadAllText(_repoInfoCachePath), Does.Contain("main"));
        }

        [Test]
        public async Task ResolveBaseUrlAsync_NetworkFailure_NoCache_ReturnsNull()
        {
            Respond(HttpStatusCode.InternalServerError, null);

            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ResolveBaseUrlAsync_NetworkFailure_ValidCacheFallback_ReturnsUrlFromCache()
        {
            Respond(HttpStatusCode.InternalServerError, null);
            WriteCache("{ \"default_branch\": \"develop\" }");

            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.EqualTo("http://test.local/raw/develop/catalogs/"));
        }

        [Test]
        public async Task ResolveBaseUrlAsync_NetworkFailure_CorruptCache_ReturnsNull()
        {
            Respond(HttpStatusCode.InternalServerError, null);
            WriteCache("not json at all");

            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ResolveBaseUrlAsync_InvalidJson_ReturnsNull()
        {
            Respond(HttpStatusCode.OK, "{ this is not valid json");

            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ResolveBaseUrlAsync_MissingDefaultBranch_ReturnsNull()
        {
            Respond(HttpStatusCode.OK, "{ \"name\": \"repo\" }");

            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ResolveBaseUrlAsync_EmptyDefaultBranch_ReturnsNull()
        {
            Respond(HttpStatusCode.OK, "{ \"default_branch\": \"\" }");

            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ResolveBaseUrlAsync_WhitespaceOnlyResponse_ReturnsNull()
        {
            Respond(HttpStatusCode.OK, "   ");

            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task ResolveBaseUrlAsync_BranchWithSlashes_FormatsTemplateCorrectly()
        {
            Respond(HttpStatusCode.OK, "{ \"default_branch\": \"feature/stable-v2\" }");

            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.EqualTo("http://test.local/raw/feature/stable-v2/catalogs/"));
        }

        [Test]
        public async Task ResolveBaseUrlAsync_NotModified_ReadsCacheAndFormatsUrl()
        {
            WriteCache("{ \"default_branch\": \"cached-branch\" }");
            _handler.Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified));

            string result = await _resolver.ResolveBaseUrlAsync();

            Assert.That(result, Is.EqualTo("http://test.local/raw/cached-branch/catalogs/"));
        }
    }
}
