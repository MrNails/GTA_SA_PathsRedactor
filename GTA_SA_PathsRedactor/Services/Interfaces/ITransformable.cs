namespace GTA_SA_PathsRedactor.Services
{
    public interface ITransformable
    {
        void Transform(PointTransformationData? pointTransformationData);
        void TransformBack(PointTransformationData? pointTransformationData);
    }
}
