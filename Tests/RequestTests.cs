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
        public void Consume()
        {
            var request = _world.GetRequest<TestRequest>();

            request.Publish(new TestRequest {value = 1f});
            request.Publish(new TestRequest {value = 2f});
            request.Publish(new TestRequest {value = 3f});

            var consumed = new List<float>();

            foreach (var it in request.Consume())
            {
                consumed.Add(it.value);
            }

            Assert.That(consumed, Is.EquivalentTo(new List<float> {1f, 2f, 3f}));
        }

        [Test]
        public void ConsumePartial()
        {
            var request = _world.GetRequest<TestRequest>();

            request.Publish(new TestRequest {value = 1f});
            request.Publish(new TestRequest {value = 2f});
            request.Publish(new TestRequest {value = 3f});

            var consumed = new List<float>();

            foreach (var it in request.Consume())
            {
                consumed.Add(it.value);
                break;
            }

            foreach (var it in request.Consume())
            {
                consumed.Add(it.value);
            }

            Assert.That(consumed, Is.EquivalentTo(new List<float> {1f, 2f, 3f}));
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
        public void Consume_Publish_ErrorLog()
        {
            var request = _world.GetRequest<TestRequest>();

            request.Consume();

            request.Publish(new TestRequest {value = 1f});

            LogAssert.Expect(LogType.Error,
                "[MORPEH] The request was already consumed in the current frame. Reorder systems or set allowNextFrame parameter");
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