using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventSourcingTest.Domain;

namespace EventSourcingTest.Events
{
    public abstract class Event
    {
        public readonly DateTime Timestamp;
        public readonly Guid Version;
        public readonly Guid? AppliesToVersion;

        public Event(Guid? appliesToVersion)
        {
            this.Timestamp = DateTime.Now;
            this.Version = Guid.NewGuid();
            this.AppliesToVersion = appliesToVersion;
        }
    }

    public class ThingCreated : Event
    {
        public readonly Guid Id;

        public readonly string Name;

        public readonly string Description;

        public ThingCreated(string name, string description)
            : this(Guid.NewGuid(), name, description)
        {
        }

        public ThingCreated(Guid id, string name, string description)
            : base(null)
        {
            this.Id = id;
            this.Name = name;
            this.Description = description;
        }
    }

    public class ThingNameChanged : Event
    {
        public ThingNameChanged(string newName, Guid appliesToVersion)
            : base(appliesToVersion)
        {
            this.NewName = newName;
        }

        public readonly string NewName;
    }

    public class ThingDescriptionChanged : Event
    {
        public ThingDescriptionChanged(string newDescription, Guid appliesToVersion)
            : base(appliesToVersion)
        {
            this.NewDescription = newDescription;
        }

        public readonly string NewDescription;
    }
}