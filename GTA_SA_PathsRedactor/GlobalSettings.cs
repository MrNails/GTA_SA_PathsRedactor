﻿using System;
using System.ComponentModel;
using GTA_SA_PathsRedactor.Services;
using Newtonsoft.Json;

namespace GTA_SA_PathsRedactor
{
    [Serializable]
    public enum Resolution
    {
        _1080x850,
        _1280x1024,
        _1680x1050,
        _1920x1080,
    }

    [Serializable]
    public sealed class GlobalSettings : IDisposable, INotifyPropertyChanged
    {
        private static readonly PointTransformationData deltaFor1080x850;
        private static readonly PointTransformationData deltaFor1280x1024;
        private static readonly PointTransformationData deltaFor1680x1050;
        private static readonly PointTransformationData deltaFor1920x1080;
        private static readonly GlobalSettings m_globalSettings;

        [NonSerialized] private readonly object m_locker;

        [NonSerialized] private readonly PointTransformationData m_defaultPTD;

        private Core.PointSaver n_currentSaver;
        private Core.PointLoader n_currentLoader;
        private PointTransformationData? m_PTD;
        private Resolution resolution;

        [field: NonSerialized] public event PropertyChangedEventHandler? PropertyChanged;

        #region Constructors
        // Static constructor needed for resolution deltas, 
        // because I haven't been able to calculate the delta for all resolutions.
        // As main resolution I take first resolution (i.e. 1080x850), so delta for this
        // resolution is 1 for all properties.
        static GlobalSettings()
        {
            m_globalSettings = new GlobalSettings();

            deltaFor1080x850 = new PointTransformationData(1, 1, 1, 1, 1, 1, "Deltas for resolution 1080x850");
            deltaFor1280x1024 = new PointTransformationData(0.8082706766917293, 0.8195020746887967, 1.221238938053097,
                                                            1.216, 0.8120300751879699, 0.8193240226364156,
                                                            "Deltas for resolution 1280x1024");
            deltaFor1680x1050 = new PointTransformationData(0.589041095890411, 0.797979797979798, 1.682926829268293,
                                                            1.256198347107438, 0.5901639344262295, 0.7977857540063697,
                                                            "Deltas for resolution 1680x1050");
            deltaFor1920x1080 = new PointTransformationData(0.5058823529411765, 0.7745098039215686, 1.971428571428571,
                                                            1.299145299145299, 0.5070422535211268, 0.7742995927579608,
                                                            "Deltas for resolution 1920x1080");
        }

        private GlobalSettings()
        {
            m_locker = new object();

            var fileSource = new string(System.Text.Encoding.UTF8.GetChars(AppResources.DefaultPointSettings));
            PTD = JsonConvert.DeserializeObject<PointTransformationData>(fileSource);

            m_defaultPTD = PTD;

            n_currentSaver = new DefaultPointSaver();
            n_currentLoader = new DefaultPointLoader();

            resolution = Resolution._1280x1024;
        }
        #endregion

        [JsonIgnore] public PointTransformationData DefaultPTD => m_defaultPTD;

        [JsonConverter(typeof(Services.JsonConverters.PointSaverConverter))]
        public Core.PointSaver CurrentSaver
        {
            get => n_currentSaver;
            set
            {
                lock (m_locker)
                {
                    n_currentSaver = value;
                }

                OnPropertyChanged();
            }
        }
        [JsonConverter(typeof(Services.JsonConverters.PointLoaderConverter))]
        public Core.PointLoader CurrentLoader
        {
            get => n_currentLoader;
            set
            {
                lock (this)
                {
                    n_currentLoader = value;
                }

                OnPropertyChanged();
            }
        }
        public PointTransformationData? PTD
        {
            get => m_PTD;
            set
            {
                lock (m_locker)
                {
                    if (value == null)
                        m_PTD = DefaultPTD;
                    else
                        m_PTD = value;
                }

                OnPropertyChanged();
            }
        }

        public Resolution Resolution
        {
            get => resolution;
            set
            {
                resolution = value;
                OnPropertyChanged();
            }
        }

        public PointTransformationData? GetCurrentTranfromationData()
        {
            var currentResolutionPTD = GetCurrentResolutionPTD(Resolution);

            var newPTD = new PointTransformationData
            {
                OffsetX = PTD.OffsetX / currentResolutionPTD.OffsetX,
                OffsetY = PTD.OffsetY / currentResolutionPTD.OffsetY,
                PointScaleX = PTD.PointScaleX / currentResolutionPTD.PointScaleX,
                PointScaleY = PTD.PointScaleY / currentResolutionPTD.PointScaleY,
                OriginalMapHeight = PTD.OriginalMapHeight / currentResolutionPTD.OriginalMapHeight,
                OriginalMapWidth = PTD.OriginalMapWidth / currentResolutionPTD.OriginalMapWidth,
                InvertVertically = PTD.InvertVertically,
                InvertHorizontally = PTD.InvertHorizontally,
                TransformName = PTD.TransformName,
            };

            return newPTD;
        }

        public void Dispose()
        {
            CurrentSaver?.Dispose();
            CurrentLoader?.Dispose();
        }

        public static GlobalSettings GetInstance()
        {
            return m_globalSettings;
        }

        private static PointTransformationData GetCurrentResolutionPTD(Resolution resolution) => resolution switch
        {
            Resolution._1080x850 => deltaFor1080x850,
            Resolution._1280x1024 => deltaFor1280x1024,
            Resolution._1680x1050 => deltaFor1680x1050,
            Resolution._1920x1080 => deltaFor1920x1080,
            _ => deltaFor1080x850
        };

        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string prop = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }
    }
}
