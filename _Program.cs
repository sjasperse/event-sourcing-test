using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSourcingTest.Domain;

namespace EventSourcingTest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var eventStore = new EventStore();
            var thing = Thing.CreateNew("Initial name", null);

            thing.ChangeDescription("Now I want a description");
            thing.ChangeDescription("I didn't like that last description");
            thing.ChangeDescription("Eh. I like this one better");
            thing.ChangeName("Better name");

            var id = thing.Id;

            eventStore.Store(thing.Id, thing.Metadata.NewEvents);
            thing = Thing.LoadFromEventStream(eventStore.GetEventStream(id));

            thing.ChangeName("Final Name");

            eventStore.Store(thing.Id, thing.Metadata.NewEvents);
            thing = Thing.LoadFromEventStream(eventStore.GetEventStream(id));

            thing.ChangeDescription("Final description");

            eventStore.Store(thing.Id, thing.Metadata.NewEvents);
            thing = Thing.LoadFromEventStream(eventStore.GetEventStream(id));
        }
    }
}