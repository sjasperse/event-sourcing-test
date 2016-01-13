using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EventSourcingTest.Events;

namespace EventSourcingTest.Models
{
    public abstract class BaseDomainModel<TModel>
        where TModel : BaseDomainModel<TModel>
    {
        protected BaseDomainModel(IEnumerable<Event> events, bool eventsAreNew)
        {
            this.metadata = new DomainModelMetadata();
            this.metadata.Events = Enumerable.Empty<Event>();
            this.metadata.NewEvents = Enumerable.Empty<Event>();

            this.Metadata = new DomainModelMetadataView(this.metadata);

            foreach (var @event in events)
            {
                this.ApplyEvent(@event, eventsAreNew);
            }
        }

        /// <summary>
        /// Apply an event to the model. This should be the primary event handler
        /// </summary>
        /// <param name="event"></param>
        /// <param name="eventIsNew"></param>
        protected void ApplyEvent(Event @event, bool eventIsNew = true)
        {
            if (this.metadata.Version == Guid.Empty)
            {
                if (@event.AppliesToVersion.HasValue) throw new Exception("This event can only be applied to a fresh model.");
            }
            else
            {
                if (@event.AppliesToVersion != this.metadata.Version) throw new Exception("Cannot apply this event to this model version");
            }

            var eventAppliers = this.GetEventAppliers();
            if (eventAppliers.ContainsKey(@event.GetType()))
            {
                eventAppliers[@event.GetType()]((TModel)this, @event);

                this.metadata.Version = @event.Version;
            }

            // This is just for sample purposes. Probably a real implementation wouldn't have this
            this.metadata.Events = this.metadata.Events.Union(new[] { @event }).ToArray();

            // event is new, and will need to be persisted
            if (eventIsNew) this.metadata.NewEvents = this.metadata.NewEvents.Union(new[] { @event }).ToArray();
        }

        #region Private Stuff

        // private mutable metadata object. Only a read-only is available as public
        private DomainModelMetadata metadata;

        // TODO: Should be cached, but this isn't a performance POC
        private Dictionary<Type, Action<TModel, Event>> GetEventAppliers()
        {
            var methods = new Dictionary<Type, Action<TModel, Event>>();

            var methodFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            foreach (var method in typeof(TModel).GetMethods(methodFlags))
            {
                var parameters = method.GetParameters();
                if (method.Name == "Apply"
                    && parameters.Count() == 1
                    && typeof(Event).IsAssignableFrom(parameters.Single().ParameterType))
                {
                    Type eventType = parameters.Single().ParameterType;

                    methods.Add(eventType, (dm, @event) =>
                    {
                        method.Invoke(dm, new[] { @event });
                    });
                }
            }

            return methods;
        }

        #endregion Private Stuff

        // I didn't like how all these properties were cluttering the model
        public DomainModelMetadataView Metadata { get; private set; }
    }

    public class DomainModelMetadata
    {
        public Guid Version;

        // This is just for sample purposes. Probably a real implementation wouldn't have this
        public IEnumerable<Event> Events;

        // needed for saving
        public IEnumerable<Event> NewEvents;
    }

    public class DomainModelMetadataView
    {
        private readonly DomainModelMetadata metadata;

        public DomainModelMetadataView(DomainModelMetadata metadata)
        {
            this.metadata = metadata;
        }

        public Guid Version { get { return this.metadata.Version; } }

        // This is just for sample purposes. Probably a real implementation wouldn't have this
        public IEnumerable<Event> Events { get { return this.metadata.Events; } }

        // needed for saving
        public IEnumerable<Event> NewEvents { get { return this.metadata.NewEvents; } }
    }
}