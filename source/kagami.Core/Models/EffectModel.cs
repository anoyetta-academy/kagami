using System;
using System.Xml.Serialization;
using kagami.Helpers;
using Prism.Mvvm;

namespace kagami.Models
{
    public enum EffectTarget
    {
        Self,
        Party,
        Enemy
    }

    [Serializable]
    public class EffectModel :
        BindableBase
    {
        private Job job;

        [XmlAttribute(AttributeName = "job")]
        public Job Job
        {
            get => this.job;
            set => this.SetProperty(ref this.job, value);
        }

        private string masterName;

        [XmlAttribute(AttributeName = "name")]
        public string MasterName
        {
            get => this.masterName;
            set => this.SetProperty(ref this.masterName, value);
        }

        private EffectTarget target;

        [XmlAttribute(AttributeName = "target")]
        public EffectTarget Target
        {
            get => this.target;
            set => this.SetProperty(ref this.target, value);
        }

        private bool isPlayer;

        [XmlIgnore]
        public bool IsPlayer
        {
            get => this.isPlayer;
            set => this.SetProperty(ref this.isPlayer, value);
        }

        private string localName;

        [XmlIgnore]
        public string LocalName
        {
            get => this.localName;
            set => this.SetProperty(ref this.localName, value);
        }

        private bool isEnabled;

        [XmlIgnore]
        public bool IsEnabled
        {
            get => this.isEnabled;
            set => this.SetProperty(ref this.isEnabled, value);
        }
    }
}
