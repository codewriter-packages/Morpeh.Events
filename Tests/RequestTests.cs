using System.Collections.Generic;
using NUnit.Framework;
using Scellecs.Morpeh;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class RequestTests
    {
        private const float DT = 0.01f;

        private World _world;
        private EventWorldPlugin _eventWorldPlugin;

        [SetUp]
        public void SetUp()
        {
            _world = World.Create();
            _eventWorldPlugin = new EventWorldPlugin();

            _eventWorldPlugin.Initialize(_world);
        }

        [TearDown]
        public void TearDown()
        {
            _eventWorldPlugin.Deinitialize(_world);

            _world.Dispose();
        }

        [Test]
        public void Clear()
        {
            var request = _world.GetRequest<TestRequest>();
            var consumed = new List<float>();

            request.Publish(new TestRequest {value = 1f});
            request.Publish(new TestRequest {value = 2f});
            request.Publish(new TestRequest {value = 3f});

            request.Clear();

            foreach (var it in request.Consume())
            {
                consumed.Add(it.value);
            }

            Assert.That(consumed, Is.EquivalentTo(new List<float>()));
        }

        [Test]
        public void ClearOnConsume()
        {
            var request = _world.GetRequest<TestRequest>();
            var consumed = new List<float>();

            request.Publish(new TestRequest {value = 1f});
            request.Publish(new TestRequest {value = 2f});
            request.Publish(new TestRequest {value = 3f});

            foreach (var it in request.Consume())
            {
                consumed.Add(it.value);

                request.Clear();
            }

            Assert.That(consumed, Is.EquivalentTo(new List<float> {1f}));
        }

        [Test]
        public void Consume()
        {
            var request = _world.GetRequest<TestRequest>();
            var consumed = new List<float>();

            request.Publish(new TestRequest {value = 1f});
            request.Publish(new TestRequest {value = 2f});
            request.Publish(new TestRequest {value = 3f});

            foreach (var it in request.Consume())
            {
                consumed.Add(it.value);
            }

            request.lastConsumeFrame = -1; // hack to disable frame error

            request.Publish(new TestRequest {value = 4f});
            request.Publish(new TestRequest {value = 5f});

            foreach (var it in request.Consume())
            {
                consumed.Add(it.value);
                break;
            }

            request.lastConsumeFrame = -1; // hack to disable frame error

            request.Publish(new TestRequest {value = 6f});

            foreach (var it in request.Consume())
            {
                consumed.Add(it.value);
            }

            request.lastConsumeFrame = -1; // hack to disable frame error

            request.Publish(new TestRequest {value = 70f});
            request.Publish(new TestRequest {value = 80f});

            Assert.That(consumed, Is.EquivalentTo(new List<float> {1f, 2f, 3f, 4f, 5f, 6f}));
        }

        [Test]
        public void ConsumeCleanup()
        {
            var request = _world.GetRequest<TestRequest>();

            request.Publish(new TestRequest {value = 1f});
            request.Publish(new TestRequest {value = 2f});
            request.Publish(new TestRequest {value = 3f});

            var consumed = new List<TestRequest>();

            foreach (var it in request.Consume())
            {
                // ignore
            }

            foreach (var it in request.Consume())
            {
                consumed.Add(it);
            }

            Assert.AreEqual(0, consumed.Count);
        }

        [Test]
        public void InternalMemoryCleanup()
        {
            var request = _world.GetRequest<TestRequest>();

            request.Publish(new TestRequest {value = 1f});
            request.Publish(new TestRequest {value = 2f});
            request.Publish(new TestRequest {value = 3f});

            foreach (var it in request.Consume())
            {
            }

            Assert.That(request.changes.data, Is.All.EqualTo(default(TestRequest)));
            Assert.AreEqual(0, request.changes.length);
            Assert.AreEqual(0, request.lastConsumedIndex);
        }

        [Test]
        public void Consume_Publish_ErrorLog()
        {
            var request = _world.GetRequest<TestRequest>();

            request.Consume();

            request.Publish(new TestRequest {value = 1f});

            LogAssert.Expect(LogType.Error,
                "[MORPEH] Request<TestRequest> was already consumed in the current frame. Reorder systems or set allowNextFrame parameter");
        }

        [Test]
        public void Consume_PublishNextFrame_Valid()
        {
            var request = _world.GetRequest<TestRequest>();

            request.Consume();

            request.Publish(new TestRequest {value = 1f}, allowNextFrame: true);

            Assert.Pass();
        }

        private struct TestRequest : IRequestData
        {
            public float value;
        }
    }
}