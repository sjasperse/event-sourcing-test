using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSourcingTest.Events;

namespace EventSourcingTest
{
    public class InMemoryEventStore
    {
        private readonly Dictionary<Guid, List<Event>> _eventsByAggId = new Dictionary<Guid, List<Event>>();

        public void Store(Guid aggregateId, IEnumerable<Event> events)
        {
            List<Event> aggEvents = null;
            if (_eventsByAggId.ContainsKey(aggregateId))
            {
                aggEvents = _eventsByAggId[aggregateId];
            }
            else
            {
                aggEvents = new List<Event>();
                _eventsByAggId[aggregateId] = aggEvents;
            }

            // store event
            aggEvents.AddRange(events);
        }

        public IEnumerable<Event> GetEventStream(Guid aggregateId)
        {
            if (_eventsByAggId.ContainsKey(aggregateId))
            {
                return _eventsByAggId[aggregateId].ToArray();
            }

            return Enumerable.Empty<Event>();
        }
    }
}