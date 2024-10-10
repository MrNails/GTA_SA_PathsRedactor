using System;
using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using GTA_SA_PathsRedactor.Services;

namespace GTA_SA_PathsRedactor.Models
{
    [Serializable]
    public enum Resolution
    {
        _1080x850,
        _1280x1024,
        _1680x1050,
        _1920x1080,
    }

    public sealed partial class GlobalSettings : ObservableObject
    {
        private readonly PointTransformationData m_defaultPTD;
        
        [ObservableProperty]
        private Resolution _resolution;
        
        private PointTransformationData? _ptd;

        #region Constructors

        // Static constructor needed for resolution deltas, 
        // because I haven't been able to calculate the delta for all resolutions.
        // As main resolution I take first resolution (i.e. 1080x850), so delta for this
        // resolution is 1 for all properties.
        // static GlobalSettings()
        // {
        //     m_globalSettings = new GlobalSettings();
        //
        //     deltaFor1080x850 = new PointTransformationData(1, 1, 1, 1, 1, 1, "Deltas for resolution 1080x850");
        //     deltaFor1280x1024 = new PointTransformationData(0.8082706766917293, 0.8195020746887967, 1.221238938053097,
        //                                                     1.216, 0.8120300751879699, 0.8193240226364156,
        //                                                     "Deltas for resolution 1280x1024");
        //     deltaFor1680x1050 = new PointTransformationData(0.589041095890411, 0.797979797979798, 1.682926829268293,
        //                                                     1.256198347107438, 0.5901639344262295, 0.7977857540063697,
        //                                                     "Deltas for resolution 1680x1050");
        //     deltaFor1920x1080 = new PointTransformationData(0.5058823529411765, 0.7745098039215686, 1.971428571428571,
        //                                                     1.299145299145299, 0.5070422535211268, 0.7742995927579608,
        //                                                     "Deltas for resolution 1920x1080");
        // }

        public GlobalSettings()
        {
            var fileSource = new string(System.Text.Encoding.UTF8.GetChars(AppResources.DefaultPointSettings));
            PTD = JsonSerializer.Deserialize<PointTransformationData>(fileSource);

            m_defaultPTD = PTD;
            _resolution = Resolution._1280x1024;
        }

        #endregion

        public PointTransformationData DefaultPTD => m_defaultPTD;

        public PointTransformationData? PTD
        {
            get => _ptd;
            set
            {
                if (value == null)
                    _ptd = DefaultPTD;
                else
                    _ptd = value;

                OnPropertyChanged();
            }
        }
    }
}