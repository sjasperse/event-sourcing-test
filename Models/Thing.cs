using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EventSourcingTest.Events;

namespace EventSourcingTest.Models
{
    public class Thing : BaseDomainModel<Thing>
    {
        #region Static Methods

        public static Thing CreateNew(Guid id, string name, string description)
        {
            return new Thing(new[] { new ThingCreated(id, name, description) }, true);
        }

        public static Thing LoadFromEventStream(IEnumerable<Event> events)
        {
            return new Thing(events, false);
        }

        #endregion Static Methods

        private Thing(IEnumerable<Event> events, bool eventsAreNew)
            : base(events, eventsAreNew)
        {
        }

        #region Event appliers

        // these need to be created with a name of "Apply", and a single parameter that is an event.
        // Do not call these directly. They are called via reflection from ApplyEvent

        private void Apply(ThingCreated @event)
        {
            this.Id = @event.Id;
            this.Name = @event.Name;
            this.Description = @event.Description;
        }

        private void Apply(ThingNameChanged @event)
        {
            this.Name = @event.NewName;
        }

        private void Apply(ThingDescriptionChanged @event)
        {
            this.Description = @event.NewDescription;
        }

        #endregion Event appliers

        #region Commands

        public void ChangeName(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new Exception("Name is not allowed to be null");
            if (name.Length > 100) throw new Exception("Name has a max length of 100");

            this.ApplyEvent(new ThingNameChanged(name, this.Metadata.Version));
        }

        public void ChangeDescription(string description)
        {
            // no validation on description. anything is allowed

            this.ApplyEvent(new ThingDescriptionChanged(description, this.Metadata.Version));
        }

        #endregion Commands

        #region Properties

        public Guid Id { get; private set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        #endregion Properties
    }
}