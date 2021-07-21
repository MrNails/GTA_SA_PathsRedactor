using System;
using System.Linq;
using System.Collections.Generic;
using GTA_SA_PathsRedactor.Services;

namespace GTA_SA_PathsRedactor.Models
{
    public class VOState : IStorableValue
    {
        private VisualObject m_visualObject;
        private Core.Models.GTA_SA_Point? m_oldPoint;
        private Core.Models.GTA_SA_Point? m_newPoint;
        private int m_voIndex;
        private State m_state;

        public VOState(VisualObject visualObject, Core.Models.GTA_SA_Point? oldPoint) :
            this(visualObject, oldPoint, -1, State.Moved)
        { }
        public VOState(VisualObject visualObject, Core.Models.GTA_SA_Point? oldPoint, int voIndex) :
            this(visualObject, oldPoint, voIndex, State.Moved)
        { }
        public VOState(VisualObject visualObject, Core.Models.GTA_SA_Point? oldPoint, int voIndex, State state)
        {
            m_newPoint = (Core.Models.GTA_SA_Point)visualObject.Point.Clone();
            m_visualObject = visualObject;
            m_oldPoint = oldPoint;
            m_voIndex = voIndex;
            m_state = state;
        }

        public VisualObject VisualObject => m_visualObject;

        public Core.Models.GTA_SA_Point OldPoint => m_state == State.Moved ? m_oldPoint : m_visualObject.Point;
        public Core.Models.GTA_SA_Point NewPoint => m_newPoint;

        public int VOIndex => m_voIndex;

        public object Value => new Tuple<VisualObject, Core.Models.GTA_SA_Point, int>(m_visualObject, m_oldPoint, VOIndex);

        public State State => m_state;
    }

    public class VOGroupState : IStorableValue
    {
        private IReadOnlyCollection<VOState> m_visualObjects;
        private State m_state;

        public VOGroupState(IReadOnlyCollection<VOState> visualObjects) :
            this(visualObjects, State.Moved)
        { }
        public VOGroupState(IReadOnlyCollection<VOState> visualObjects, State state)
        {
            m_visualObjects = visualObjects;

            m_state = state;
        }

        public IReadOnlyCollection<VOState> VisualObjects => m_visualObjects;

        public object Value => m_visualObjects;

        public State State => m_state;
    }
}
