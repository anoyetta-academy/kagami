using System;
using Prism.Mvvm;

namespace kagami.Models
{
    public class EffectMasterModel :
        BindableBase
    {
        #region Lazy Instance

        private readonly static Lazy<EffectMasterModel> LazyInstance = new Lazy<EffectMasterModel>(() => new EffectMasterModel());

        public static EffectMasterModel Instance => LazyInstance.Value;

        private EffectMasterModel()
        {
        }

        #endregion Lazy Instance
    }
}
