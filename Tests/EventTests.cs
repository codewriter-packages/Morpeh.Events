using NUnit.Framework;
using Scellecs.Morpeh;

namespace Tests
{
    public class EventTests
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
        public void SendEvent_CurrentFrameScheduled()
        {
            var evt = _world.GetEvent<TestEvent>();

            evt.NextFrame(new TestEvent {value = 1f});
            evt.NextFrame(new TestEvent {value = 2f});
            evt.NextFrame(new TestEvent {value = 3f});

            Assert.IsTrue(evt.IsScheduled);
            Assert.IsFalse(evt.IsPublished);
            Assert.AreEqual(3, evt.ScheduledChanges.length);
            Assert.AreEqual(0, evt.BatchedChanges.length);
            Assert.AreEqual(1f, evt.ScheduledChanges.data[0].value);
            Assert.AreEqual(2f, evt.ScheduledChanges.data[1].value);
            Assert.AreEqual(3f, evt.ScheduledChanges.data[2].value);
        }

        [Test]
        public void SendEvent_NextFramePublished()
        {
            var evt = _world.GetEvent<TestEvent>();

            evt.NextFrame(new TestEvent {value = 1f});
            evt.NextFrame(new TestEvent {value = 2f});
            evt.NextFrame(new TestEvent {value = 3f});

            _world.Update(DT);
            _world.CleanupUpdate(DT);

            Assert.IsFalse(evt.IsScheduled);
            Assert.IsTrue(evt.IsPublished);
            Assert.AreEqual(0, evt.ScheduledChanges.length);
            Assert.AreEqual(3, evt.BatchedChanges.length);
            Assert.AreEqual(1f, evt.BatchedChanges.data[0].value);
            Assert.AreEqual(2f, evt.BatchedChanges.data[1].value);
            Assert.AreEqual(3f, evt.BatchedChanges.data[2].value);
        }

        [Test]
        public void SendEvent_ToFramesCleared()
        {
            var evt = _world.GetEvent<TestEvent>();

            evt.NextFrame(new TestEvent {value = 1f});
            evt.NextFrame(new TestEvent {value = 2f});
            evt.NextFrame(new TestEvent {value = 3f});

            _world.Update(DT);
            _world.CleanupUpdate(DT);

            _world.Update(DT);
            _world.CleanupUpdate(DT);

            Assert.IsFalse(evt.IsScheduled);
            Assert.IsFalse(evt.IsPublished);
            Assert.AreEqual(0, evt.ScheduledChanges.length);
            Assert.AreEqual(0, evt.BatchedChanges.length);
        }

        [Test]
        public void SendEvent_Callback_FrameOne_NoCall()
        {
            var calls = 0;

            var evt = _world.GetEvent<TestEvent>();

            evt.Subscribe(changes =>
            {
                ++calls;

                Assert.AreEqual(3, changes.length);
                Assert.AreEqual(1f, evt.BatchedChanges.data[0].value);
                Assert.AreEqual(2f, evt.BatchedChanges.data[1].value);
                Assert.AreEqual(3f, evt.BatchedChanges.data[2].value);
            });

            evt.NextFrame(new TestEvent {value = 1f});
            evt.NextFrame(new TestEvent {value = 2f});
            evt.NextFrame(new TestEvent {value = 3f});

            _world.Update(DT);
            _world.CleanupUpdate(DT);

            Assert.AreEqual(0, calls);
        }

        [Test]
        public void SendEvent_Callback_FrameTwo_OneCall()
        {
            var calls = 0;

            var evt = _world.GetEvent<TestEvent>();

            evt.Subscribe(changes =>
            {
                ++calls;

                Assert.AreEqual(3, changes.length);
                Assert.AreEqual(1f, evt.BatchedChanges.data[0].value);
                Assert.AreEqual(2f, evt.BatchedChanges.data[1].value);
                Assert.AreEqual(3f, evt.BatchedChanges.data[2].value);
            });

            evt.NextFrame(new TestEvent {value = 1f});
            evt.NextFrame(new TestEvent {value = 2f});
            evt.NextFrame(new TestEvent {value = 3f});

            _world.Update(DT);
            _world.CleanupUpdate(DT);

            _world.Update(DT);
            _world.CleanupUpdate(DT);

            Assert.AreEqual(1, calls);
        }

        [Test]
        public void SendEvent_Callback_FrameThree_OneCall()
        {
            var calls = 0;

            var evt = _world.GetEvent<TestEvent>();

            evt.Subscribe(changes =>
            {
                ++calls;

                Assert.AreEqual(3, changes.length);
                Assert.AreEqual(1f, evt.BatchedChanges.data[0].value);
                Assert.AreEqual(2f, evt.BatchedChanges.data[1].value);
                Assert.AreEqual(3f, evt.BatchedChanges.data[2].value);
            });

            evt.NextFrame(new TestEvent {value = 1f});
            evt.NextFrame(new TestEvent {value = 2f});
            evt.NextFrame(new TestEvent {value = 3f});

            _world.Update(DT);
            _world.CleanupUpdate(DT);

            _world.Update(DT);
            _world.CleanupUpdate(DT);

            _world.Update(DT);
            _world.CleanupUpdate(DT);

            Assert.AreEqual(1, calls);
        }

        private struct TestEvent : IEventData
        {
            public float value;
        }
    }
}