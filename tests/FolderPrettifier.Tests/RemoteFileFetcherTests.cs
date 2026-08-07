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
    public class RemoteFileFetcherTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Responder { get; set; }

            public List<HttpRequestMessage> Requests { get; private set; }

            public FakeHttpMessageHandler()
            {
                Requests = new List<HttpRequestMessage>();
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Requests.Add(request);
                return Responder(request, cancellationToken);
            }
        }

        private static HttpResponseMessage Ok(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) };
        }

        private string _tempDir;

        [SetUp]
        public void SetUp()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), "fpf-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_tempDir);
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
        public async Task FetchAsync_200_WritesCacheAndReturnsContent()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(Ok("catalog-data")) };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);
            string cachePath = Path.Combine(_tempDir, "cached.txt");

            string result = await fetcher.FetchAsync("http://test.local/file.txt", cachePath);

            Assert.That(result, Is.EqualTo("catalog-data"));
            Assert.That(File.ReadAllText(cachePath), Is.EqualTo("catalog-data"));
            Assert.That(handler.Requests.Count, Is.EqualTo(1));
            Assert.That(handler.Requests[0].Headers.IfModifiedSince.HasValue, Is.False);
        }

        [Test]
        public async Task FetchAsync_304_ReturnsCachedContent()
        {
            string cachePath = Path.Combine(_tempDir, "cached.txt");
            File.WriteAllText(cachePath, "old-content");
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified)) };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);

            string result = await fetcher.FetchAsync("http://test.local/file.txt", cachePath);

            Assert.That(result, Is.EqualTo("old-content"));
            Assert.That(handler.Requests[0].Headers.IfModifiedSince.HasValue, Is.True);
        }

        [Test]
        public async Task FetchAsync_304_NoCache_ReturnsNull()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotModified)) };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);

            string result = await fetcher.FetchAsync("http://test.local/file.txt", Path.Combine(_tempDir, "missing.txt"));

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task FetchAsync_500_ReturnsNullAndWritesNoCache()
        {
            string cachePath = Path.Combine(_tempDir, "cached.txt");
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)) };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);

            string result = await fetcher.FetchAsync("http://test.local/file.txt", cachePath);

            Assert.That(result, Is.Null);
            Assert.That(File.Exists(cachePath), Is.False);
        }

        [Test]
        public async Task FetchAsync_EmptyBody_ReturnsNull()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(Ok("   ")) };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);

            string result = await fetcher.FetchAsync("http://test.local/file.txt", Path.Combine(_tempDir, "cached.txt"));

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task FetchAsync_HandlerThrows_ReturnsNull()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => { throw new IOException("network down"); } };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);

            string result = await fetcher.FetchAsync("http://test.local/file.txt", Path.Combine(_tempDir, "cached.txt"));

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task FetchAsync_CreatesCacheDirectory()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(Ok("data")) };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);
            string cachePath = Path.Combine(_tempDir, "sub", "deep", "file.txt");

            await fetcher.FetchAsync("http://test.local/file.txt", cachePath);

            Assert.That(File.Exists(cachePath), Is.True);
        }

        [Test]
        public async Task CheckAsync_Success_ReturnsTrue()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(Ok("ok")) };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);

            Assert.That(await fetcher.CheckAsync("http://test.local/ping", TimeSpan.FromSeconds(5)), Is.True);
        }

        [Test]
        public async Task CheckAsync_NonSuccess_ReturnsFalse()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler { Responder = (r, ct) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadGateway)) };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);

            Assert.That(await fetcher.CheckAsync("http://test.local/ping", TimeSpan.FromSeconds(5)), Is.False);
        }

        [Test]
        public async Task CheckAsync_SlowResponse_TimesOutAndReturnsFalse()
        {
            FakeHttpMessageHandler handler = new FakeHttpMessageHandler
            {
                Responder = async (r, ct) =>
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                    return Ok("ok");
                }
            };
            RemoteFileFetcher fetcher = new RemoteFileFetcher(handler);

            Assert.That(await fetcher.CheckAsync("http://test.local/ping", TimeSpan.FromMilliseconds(200)), Is.False);
        }
    }
}
